using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskRewardPopupUI : MonoBehaviour
{
    [System.Serializable]
    private class ItemPopupStyle
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
    }

    [Header("References")]
    [Tooltip("弹窗生成的父节点（通常是屏幕中央的 UI 空节点）。")]
    [SerializeField] private RectTransform popupRoot;
    [Tooltip("弹窗模板（建议默认 inactive，内含 TMP_Text 和可选 Image）。")]
    [SerializeField] private GameObject popupTemplate;

    [Header("Animation")]
    [Min(0.01f)]
    [SerializeField] private float moveDistance = 120f;
    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 1.1f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private float stackSpacing = 34f;

    [Header("Content")]
    [SerializeField] private string amountPrefix = "+";
    [SerializeField] private List<ItemPopupStyle> itemStyles = new List<ItemPopupStyle>();

    private InventorySystem inventory;
    private readonly List<RectTransform> liveEntries = new List<RectTransform>();

    private void OnEnable()
    {
        TryBindInventory();
    }

    private void Start()
    {
        if (popupTemplate != null && popupTemplate.activeSelf)
        {
            popupTemplate.SetActive(false);
        }
    }

    private void Update()
    {
        TryBindInventory();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnTaskRewardAdded -= HandleTaskRewardAdded;
            inventory = null;
        }
    }

    private void TryBindInventory()
    {
        if (inventory == InventorySystem.Instance)
        {
            return;
        }

        if (inventory != null)
        {
            inventory.OnTaskRewardAdded -= HandleTaskRewardAdded;
        }

        inventory = InventorySystem.Instance;

        if (inventory != null)
        {
            inventory.OnTaskRewardAdded += HandleTaskRewardAdded;
        }
    }

    private void HandleTaskRewardAdded(string itemId, int amount)
    {
        if (!isActiveAndEnabled || amount <= 0)
        {
            return;
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

        GameObject popupObj = Instantiate(popupTemplate, parent);
        popupObj.SetActive(true);

        RectTransform rect = popupObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            liveEntries.Add(rect);
            RepositionLiveEntries();
        }

        CanvasGroup canvasGroup = popupObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popupObj.AddComponent<CanvasGroup>();
        }

        TMP_Text text = popupObj.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = $"{amountPrefix}{amount} {ResolveDisplayName(itemId)}";
        }

        Image iconImage = popupObj.GetComponentInChildren<Image>(true);
        ItemPopupStyle style = itemStyles.Find(x => x != null && x.itemId == itemId);
        if (iconImage != null)
        {
            if (style != null && style.icon != null)
            {
                iconImage.sprite = style.icon;
                iconImage.color = Color.white;
            }
            else
            {
                // 没配置图标时隐藏 icon（避免显示错图）
                if (iconImage.gameObject != popupObj)
                {
                    iconImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }

        StartCoroutine(AnimateAndRecycle(rect, canvasGroup, popupObj));
    }

    private IEnumerator AnimateAndRecycle(RectTransform rect, CanvasGroup canvasGroup, GameObject popupObj)
    {
        Vector2 basePos = rect != null ? rect.anchoredPosition : Vector2.zero;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            float moveT = moveCurve != null ? moveCurve.Evaluate(t) : t;
            float alphaT = alphaCurve != null ? alphaCurve.Evaluate(t) : 1f - t;

            if (rect != null)
            {
                rect.anchoredPosition = basePos + Vector2.up * (moveDistance * moveT);
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

        if (popupObj != null)
        {
            Destroy(popupObj);
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

            Vector2 anchored = rect.anchoredPosition;
            anchored.y = -stackSpacing * i;
            rect.anchoredPosition = anchored;
        }
    }

    private string ResolveDisplayName(string itemId)
    {
        ItemPopupStyle style = itemStyles.Find(x => x != null && x.itemId == itemId);
        if (style != null && !string.IsNullOrEmpty(style.displayName))
        {
            return style.displayName;
        }

        return itemId;
    }
}
