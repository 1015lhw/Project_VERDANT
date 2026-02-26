using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIDragRock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private MapTaskManager mapTaskManager;
    [SerializeField] private float removeDistance = 250f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Vector2 initialAnchoredPosition;
    private bool isRemoved;
    private bool isInitialized;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
    }

    public void BindManager(MapTaskManager manager)
    {
        if (manager != null)
        {
            mapTaskManager = manager;
        }

        CacheReferences();
    }

    public void ResetRock()
    {
        if (!CacheReferences())
        {
            return;
        }

        isRemoved = false;
        gameObject.SetActive(true);
        rectTransform.anchoredPosition = initialAnchoredPosition;
    }

    public void MarkRemoved()
    {
        if (!CacheReferences())
        {
            return;
        }

        isRemoved = true;
        gameObject.SetActive(false);
        rectTransform.anchoredPosition = initialAnchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted))
        {
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted) || !CacheReferences())
        {
            return;
        }

        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted) || !CacheReferences())
        {
            return;
        }

        float distance = Vector2.Distance(rectTransform.anchoredPosition, initialAnchoredPosition);

        if (distance > removeDistance)
        {
            MarkRemoved();

            if (mapTaskManager != null)
            {
                mapTaskManager.NotifyRockCleared();
            }

            return;
        }

        rectTransform.anchoredPosition = initialAnchoredPosition;
    }

    private bool CacheReferences()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        if (mapTaskManager == null)
        {
            mapTaskManager = GetComponentInParent<MapTaskManager>();
        }

        Image rockImage = GetComponent<Image>();
        if (rockImage != null)
        {
            rockImage.raycastTarget = true;
        }

        if (rectTransform == null)
        {
            Debug.LogWarning($"[{nameof(UIDragRock)}] Missing RectTransform on {name}.", this);
            return false;
        }

        if (!isInitialized)
        {
            initialAnchoredPosition = rectTransform.anchoredPosition;
            isInitialized = true;
        }

        return true;
    }
}
