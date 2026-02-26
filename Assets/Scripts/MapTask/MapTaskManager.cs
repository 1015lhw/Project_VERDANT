using TMPro;
using UnityEngine;

public class MapTaskManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text counterText;

    [Header("Settings")]
    [SerializeField] private int totalRocks = 5;
    [SerializeField] private string rewardItemID = "Map";

    private int clearedCount;
    private bool isCompleted;

    private void Awake()
    {
        if (counterText == null)
        {
            counterText = transform.Find("TaskWindow/CounterText")?.GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        if (InventorySystem.Instance != null && InventorySystem.Instance.Has(rewardItemID))
        {
            isCompleted = true;
            clearedCount = totalRocks;
        }

        UpdateCounter();
    }

    public void NotifyRockCleared()
    {
        if (isCompleted)
        {
            return;
        }

        clearedCount = Mathf.Min(clearedCount + 1, totalRocks);

        if (clearedCount >= totalRocks)
        {
            CompleteTask();
        }
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }

    private void CompleteTask()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        UpdateCounter();

        if (InventorySystem.Instance != null)
        {
            if (!InventorySystem.Instance.Has(rewardItemID))
            {
                InventorySystem.Instance.AddItem(rewardItemID);
            }
        }
        else
        {
            Debug.LogWarning("[MapTaskManager] InventorySystem.Instance is null. Map reward was not granted.");
        }
    }

    private void UpdateCounter()
    {
        if (counterText == null)
        {
            return;
        }

        counterText.text = isCompleted ? "1 / 1" : "0 / 1";
    }
}
