using UnityEngine;
using TMPro;

public class MapTaskManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text counterText;

    [Header("Settings")]
    public int totalRocks = 5;
    public string rewardItemID = "Map";

    private int clearedCount = 0;
    private bool isCompleted = false;

    void Start()
    {
        UpdateCounter();
    }

    // 被石头调用
    public void NotifyRockCleared()
    {
        if (isCompleted) return;

        clearedCount++;

        if (clearedCount >= totalRocks)
        {
            CompleteTask();
        }
    }

    void CompleteTask()
    {
        isCompleted = true;

        if (counterText != null)
            counterText.text = "1 / 1";

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(rewardItemID);
        }
        else
        {
            Debug.LogError("InventorySystem Instance is NULL!");
        }
    }

    void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = "0 / 1";
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }
}