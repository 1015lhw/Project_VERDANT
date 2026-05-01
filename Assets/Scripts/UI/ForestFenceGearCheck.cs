using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Chapter Ending Overlay")]
    [SerializeField] private CanvasGroup endingOverlay;
    [SerializeField] private TMP_Text endingSubtitle;
    [SerializeField] private string endingSubtitleText = "Chapter II Coming soon";
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private string menuSceneName = "Menu";

    private Collider fenceCollider;
    private Coroutine hideNoticeCoroutine;
    private Coroutine endingCoroutine;
    private bool hasTriggeredEnding;
    private bool waitingForReturnToMenu;

    private void Awake()
    {
        fenceCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        RefreshFenceState();
        HideNotice();
        PrepareEndingOverlay();
    }

    private void OnEnable()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += HandleInventoryChanged;
        }
    }

    private void Update()
    {
        if (!waitingForReturnToMenu)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            ReturnToMenu();
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
            TriggerEnding();
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

        // 需求：必须同时拥有地图和指南针才能通过。
        return hasMap && hasCompass;
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

    private void PrepareEndingOverlay()
    {
        if (endingOverlay == null)
        {
            return;
        }

        endingOverlay.alpha = 0f;
        endingOverlay.interactable = false;
        endingOverlay.blocksRaycasts = false;
        endingOverlay.gameObject.SetActive(false);

        if (endingSubtitle != null)
        {
            endingSubtitle.text = string.Empty;
        }
    }

    private void TriggerEnding()
    {
        if (hasTriggeredEnding || endingOverlay == null)
        {
            return;
        }

        hasTriggeredEnding = true;

        if (endingCoroutine != null)
        {
            StopCoroutine(endingCoroutine);
        }

        endingCoroutine = StartCoroutine(FadeInEndingOverlay());
    }

    private System.Collections.IEnumerator FadeInEndingOverlay()
    {
        endingOverlay.gameObject.SetActive(true);
        endingOverlay.alpha = 0f;
        endingOverlay.blocksRaycasts = true;
        endingOverlay.interactable = true;

        if (endingSubtitle != null)
        {
            endingSubtitle.text = endingSubtitleText;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            endingOverlay.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        endingOverlay.alpha = 1f;
        waitingForReturnToMenu = true;
        endingCoroutine = null;
    }

    private void ReturnToMenu()
    {
        waitingForReturnToMenu = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
