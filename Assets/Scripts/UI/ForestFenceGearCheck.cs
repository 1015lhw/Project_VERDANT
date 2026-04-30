using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ForestFenceGearCheck : MonoBehaviour
{
    [Header("Required Gear IDs (InventorySystem)")]
    [SerializeField] private string mapItemId = "Map";
    [SerializeField] private string compassItemId = "Compass";

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI (Canvas/BAG/notice TMP)")]
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private string blockMessage = "森林太危险了 你需要伙伴手里的装备";
    [SerializeField] private float messageDuration = 2f;

    private Collider fenceCollider;
    private Coroutine hideNoticeCoroutine;

    private void Awake()
    {
        fenceCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        RefreshFenceState();
        HideNotice();
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        if (CanPass())
        {
            return;
        }

        ShowNotice();
    }

    private void HandleInventoryChanged()
    {
        RefreshFenceState();
    }

    private bool CanPass()
    {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            return false;
        }

        bool hasMap = inventory.Has(mapItemId);
        bool hasCompass = inventory.Has(compassItemId);

        // 需求：没有指南针和地图时拦住玩家。
        return hasMap || hasCompass;
    }

    private void RefreshFenceState()
    {
        if (fenceCollider == null)
        {
            return;
        }

        fenceCollider.enabled = !CanPass();
    }

    private void ShowNotice()
    {
        if (noticeText == null)
        {
            return;
        }

        noticeText.gameObject.SetActive(true);
        noticeText.text = blockMessage;

        if (hideNoticeCoroutine != null)
        {
            StopCoroutine(hideNoticeCoroutine);
        }

        hideNoticeCoroutine = StartCoroutine(HideNoticeDelayed());
    }

    private System.Collections.IEnumerator HideNoticeDelayed()
    {
        yield return new WaitForSeconds(messageDuration);
        HideNotice();
        hideNoticeCoroutine = null;
    }

    private void HideNotice()
    {
        if (noticeText == null)
        {
            return;
        }

        noticeText.text = string.Empty;
        noticeText.gameObject.SetActive(false);
    }
}
