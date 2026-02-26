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

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        initialAnchoredPosition = rectTransform.anchoredPosition;

        if (mapTaskManager == null)
        {
            mapTaskManager = GetComponentInParent<MapTaskManager>();
        }

        Image rockImage = GetComponent<Image>();
        rockImage.raycastTarget = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted()))
        {
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted()))
        {
            return;
        }

        float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRemoved || (mapTaskManager != null && mapTaskManager.IsCompleted()))
        {
            return;
        }

        float distance = Vector2.Distance(rectTransform.anchoredPosition, initialAnchoredPosition);

        if (distance > removeDistance)
        {
            isRemoved = true;

            if (mapTaskManager != null)
            {
                mapTaskManager.NotifyRockCleared();
            }

            gameObject.SetActive(false);
            return;
        }

        rectTransform.anchoredPosition = initialAnchoredPosition;
    }
}
