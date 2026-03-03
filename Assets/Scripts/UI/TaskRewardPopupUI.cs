using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskRewardPopupUI : MonoBehaviour
{
    [System.Serializable]
    private class ItemPopupConfig
    {
        [Tooltip("与奖励里使用的 itemId 一致（例如 Berry / Map）。")]
        public string itemId;

        [Tooltip("弹窗显示名称。")]
        public string displayName;

        [Tooltip("弹窗显示图标。")]
        public Sprite icon;
    }

    [Header("References")]
    [Tooltip("弹窗实例生成父节点（通常放在屏幕中央锚点）。")]
    [SerializeField] private RectTransform popupRoot;

    [Tooltip("弹窗模板（背景图预制体）。模板上需挂 TaskRewardPopupEntry 并绑定 名字/数量/图标 引用。")]
    [SerializeField] private GameObject popupTemplate;

    [Header("Animation")]
    [Min(0.01f)]
    [SerializeField] private float moveDistance = 120f;

    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 1.1f;

    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Tooltip("多条弹窗时的垂直间隔。")]
    [SerializeField] private float stackSpacing = 36f;

    [Header("Trigger Timing")]
    [Tooltip("奖励事件触发后，延迟多久再开始弹窗（秒）。可用于等任务窗口关闭后再弹出。")]
    [Min(0f)]
    [SerializeField] private float popupDelay = 0f;

    [Header("Content")]
    [SerializeField] private string amountPrefix = "+";

    [Tooltip("在这里折叠配置每个 item 的显示名和图标。")]
    [SerializeField] private List<ItemPopupConfig> itemConfigs = new List<ItemPopupConfig>();

    private InventorySystem inventory;
    private readonly List<RectTransform> liveEntries = new List<RectTransform>();

    private void Awake()
    {
        if (popupTemplate != null && popupTemplate.activeSelf)
        {
            popupTemplate.SetActive(false);
        }
    }

    private void OnEnable()
    {
        TryBindInventory();
    }

    private void Start()
    {
        TryBindInventory();
    }

    private void OnDisable()
    {
        UnbindInventory();
    }

    private void TryBindInventory()
    {
        if (inventory == InventorySystem.Instance)
        {
            return;
        }

        UnbindInventory();

        inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            inventory.OnTaskRewardAdded += HandleTaskRewardAdded;
        }
    }

    private void UnbindInventory()
    {
        if (inventory == null)
        {
            return;
        }

        inventory.OnTaskRewardAdded -= HandleTaskRewardAdded;
        inventory = null;
    }

    private void HandleTaskRewardAdded(string itemId, int amount)
    {
        if (!isActiveAndEnabled || amount <= 0)
        {
            return;
        }

        StartCoroutine(ShowPopupWithDelay(itemId, amount));
    }

    private IEnumerator ShowPopupWithDelay(string itemId, int amount)
    {
        if (popupDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(popupDelay);
        }

        if (!isActiveAndEnabled)
        {
            yield break;
        }

        ShowPopup(itemId, amount);
    }

    private void ShowPopup(string itemId, int amount)
    {
        if (popupTemplate == null)
        {
            Debug.LogWarning($"[{nameof(TaskRewardPopupUI)}] Missing popupTemplate.", this);
            return;
        }

        RectTransform parent = popupRoot != null ? popupRoot : transform as RectTransform;
        if (parent == null)
        {
            Debug.LogWarning($"[{nameof(TaskRewardPopupUI)}] Missing popupRoot RectTransform.", this);
            return;
        }

        GameObject popupObject = Instantiate(popupTemplate, parent);
        popupObject.SetActive(true);

        TaskRewardPopupEntry entry = popupObject.GetComponent<TaskRewardPopupEntry>();
        if (entry == null)
        {
            Debug.LogWarning($"[{nameof(TaskRewardPopupUI)}] popupTemplate missing TaskRewardPopupEntry.", popupObject);
            Destroy(popupObject);
            return;
        }

        RectTransform rect = popupObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            liveEntries.Add(rect);
            RepositionLiveEntries();
        }

        CanvasGroup canvasGroup = popupObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popupObject.AddComponent<CanvasGroup>();
        }

        ItemPopupConfig config = FindItemConfig(itemId);
        string itemName = config != null && !string.IsNullOrEmpty(config.displayName)
            ? config.displayName
            : itemId;

        Sprite itemIcon = config != null ? config.icon : null;
        if (config == null)
        {
            Debug.LogWarning($"[{nameof(TaskRewardPopupUI)}] Missing item config for reward id '{itemId}'.", this);
        }

        entry.SetContent(itemName, amount, itemIcon, amountPrefix);

        StartCoroutine(AnimateAndDestroy(rect, canvasGroup, popupObject));
    }

    private IEnumerator AnimateAndDestroy(RectTransform rect, CanvasGroup canvasGroup, GameObject popupObject)
    {
        Vector2 basePosition = rect != null ? rect.anchoredPosition : Vector2.zero;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            float moveT = moveCurve != null ? moveCurve.Evaluate(t) : t;
            float alphaT = alphaCurve != null ? alphaCurve.Evaluate(t) : 1f - t;

            if (rect != null)
            {
                rect.anchoredPosition = basePosition + Vector2.up * (moveDistance * moveT);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(alphaT);
            }

            yield return null;
        }

        if (rect != null)
        {
            liveEntries.Remove(rect);
            RepositionLiveEntries();
        }

        if (popupObject != null)
        {
            Destroy(popupObject);
        }
    }

    private void RepositionLiveEntries()
    {
        for (int i = 0; i < liveEntries.Count; i++)
        {
            RectTransform rect = liveEntries[i];
            if (rect == null)
            {
                continue;
            }

            Vector2 pos = rect.anchoredPosition;
            pos.y = -stackSpacing * i;
            rect.anchoredPosition = pos;
        }
    }

    private ItemPopupConfig FindItemConfig(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || itemConfigs == null)
        {
            return null;
        }

        string normalizedTarget = itemId.Trim();

        for (int i = 0; i < itemConfigs.Count; i++)
        {
            ItemPopupConfig config = itemConfigs[i];
            if (config == null || string.IsNullOrWhiteSpace(config.itemId))
            {
                continue;
            }

            if (string.Equals(config.itemId.Trim(), normalizedTarget, System.StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }

        return null;
    }
}
