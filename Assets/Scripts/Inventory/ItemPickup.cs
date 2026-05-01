using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("InventorySystem 内的物品 ID,需与 DynamicInventoryUI.itemLibrary 中配置的 id 一致(如 \"Mushroom\")。")]
    public string itemId = "Mushroom";

    [Tooltip("拾取数量。如果 DynamicInventoryUI.itemLibrary 中配置了 configuredAmount,则优先使用配置值。")]
    [Min(1)]
    public int amount = 1;

    [Tooltip("拾取一次后是否禁用本物体(避免重复拾取)。")]
    public bool oneTime = true;

    [Tooltip("拾取后是否销毁本物体。oneTime=true 且本项=true 时销毁;false 时仅 SetActive(false)。")]
    public bool destroyOnPickup = true;

    [Header("Wwise Events")]
    public AK.Wwise.Event pickupEvent;

    [Header("Press E UI")]
    [Tooltip("玩家进入碰撞体时显示的 \"按 E 拾取\" 提示 UI。沿用对话用的同一个 PressE Prompt 即可。")]
    public GameObject pressEUI;

    private bool playerInRange;
    private bool consumed;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    private void Update()
    {
        if (consumed) return;
        if (!playerInRange) return;
        if (OpeningLock.IsLocked) return;
        if (!GameStateManager.IsNormal) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning($"[{nameof(ItemPickup)}] itemId is empty on {name}.", this);
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning($"[{nameof(ItemPickup)}] InventorySystem.Instance is null. Place an InventorySystem in the scene.", this);
            return;
        }

        int configuredAmount = DynamicInventoryUI.GetConfiguredAmountById(itemId);
        int finalAmount = configuredAmount > 0 ? configuredAmount : Mathf.Max(1, amount);

        // AddTaskReward triggers OnTaskRewardAdded -> TaskRewardPopupUI shows a popup,
        // and OnInventoryChanged -> DynamicInventoryUI refreshes its slots.
        InventorySystem.Instance.AddTaskReward(itemId, finalAmount);

        consumed = true;

        if (pickupEvent != null)
        {
            pickupEvent.Post(gameObject);
        }

        PressEPromptCoordinator.SetRequest(pressEUI, this, false);

        if (oneTime)
        {
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        bool shouldShow = !consumed
            && playerInRange
            && !OpeningLock.IsLocked
            && GameStateManager.IsNormal
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);

        PressEPromptCoordinator.SetRequest(pressEUI, this, shouldShow);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (!OpeningLock.IsLocked
            && GameStateManager.IsNormal
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
        {
            PressEPromptCoordinator.SetRequest(pressEUI, this, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    private void OnDisable()
    {
        PressEPromptCoordinator.ClearRequester(this);
    }
}
