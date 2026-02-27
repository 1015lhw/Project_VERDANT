using UnityEngine;
using System.Collections;

public class InventoryToggle : MonoBehaviour
{
    public RectTransform panel;
    public CanvasGroup dimCanvasGroup;  // 用 CanvasGroup 控制透明度
    public GameObject bagRedDot;        // 背包图标红点

    public float slideSpeed = 10f;
    public float dimFadeTime = 0.15f;   

    private bool isOpen = false;
    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private InventorySystem inventory;

    void OnEnable()
    {
        TryBindInventory();
        RefreshRedDot();
    }

    void Start()
    {
        // 记录当前位置为显示位置，并计算隐藏位置
        shownPos = panel.anchoredPosition;
        hiddenPos = shownPos + Vector2.down * 1500f; 

        // 初始化：将面板移到屏幕下方
        panel.anchoredPosition = hiddenPos;

        if (dimCanvasGroup != null)
        {
            dimCanvasGroup.alpha = 0f;
            dimCanvasGroup.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        TryBindInventory();

        // 自愈逻辑：如果状态记录为 Inventory 但面板实际上关了，重置状态
        // 修复点：移除了 .Instance 直接访问类成员
        if (!isOpen && GameStateManager.CurrentState == GameState.Inventory)
        {
            GameStateManager.ResetToNormal();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (CanToggleInventory())
                Toggle();
        }

        AnimatePanel();
    }

    void Toggle()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // 确保面板物体在 Inspector 里被激活，解决“必须手动勾选”的问题
            if (panel != null) panel.gameObject.SetActive(true);

            GameStateManager.SetState(GameState.Inventory);

            if (inventory != null)
            {
                inventory.MarkTaskRewardSeen();
            }

            // 每次打开背包都主动刷新一次，避免错过事件导致显示空白
            foreach (var ui in panel.GetComponentsInChildren<DynamicInventoryUI>(true))
            {
                ui.Refresh();
            }
            
            if (dimCanvasGroup != null)
            {
                dimCanvasGroup.gameObject.SetActive(true);
                StartCoroutine(FadeDim(1f));
            }
        }
        else
        {
            GameStateManager.ResetToNormal();
            if (dimCanvasGroup != null)
            {
                StartCoroutine(FadeDim(0f));
            }
        }
    }

    bool CanToggleInventory()
    {
        // 允许在 Normal 状态打开，或者在 Inventory 状态且已经打开时关闭
        return GameStateManager.IsNormal || (GameStateManager.CurrentState == GameState.Inventory && isOpen);
    }

    void AnimatePanel()
    {
        if (panel == null) return;

        Vector2 target = isOpen ? shownPos : hiddenPos;

        // 平滑插值移动
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * slideSpeed
        );
    }

    IEnumerator FadeDim(float targetAlpha)
    {
        if (dimCanvasGroup == null) yield break;

        float startAlpha = dimCanvasGroup.alpha;
        float time = 0f;

        while (time < dimFadeTime)
        {
            time += Time.deltaTime;
            dimCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / dimFadeTime);
            yield return null;
        }

        dimCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
            dimCanvasGroup.gameObject.SetActive(false);
    }

    private void TryBindInventory()
    {
        if (inventory == InventorySystem.Instance)
        {
            return;
        }

        if (inventory != null)
        {
            inventory.OnTaskRewardNotificationChanged -= HandleTaskRewardNotificationChanged;
        }

        inventory = InventorySystem.Instance;

        if (inventory != null)
        {
            inventory.OnTaskRewardNotificationChanged += HandleTaskRewardNotificationChanged;
        }
    }

    private void HandleTaskRewardNotificationChanged(bool hasUnseenReward)
    {
        RefreshRedDot(hasUnseenReward);
    }

    private void RefreshRedDot()
    {
        RefreshRedDot(inventory != null && inventory.HasUnseenTaskReward);
    }

    private void RefreshRedDot(bool show)
    {
        if (bagRedDot != null)
        {
            bagRedDot.SetActive(show);
        }
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnTaskRewardNotificationChanged -= HandleTaskRewardNotificationChanged;
            inventory = null;
        }

        // 脚本禁用时清理状态，防止逻辑锁死
        if (GameStateManager.CurrentState == GameState.Inventory)
            GameStateManager.ResetToNormal();
    }
}
