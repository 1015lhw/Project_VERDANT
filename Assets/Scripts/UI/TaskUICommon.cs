using System;
using UnityEngine;

public static class TaskUICommon
{
    public static void GrantReward(string rewardItemID)
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning($"[{nameof(TaskUICommon)}] InventorySystem.Instance is null. Please place InventorySystem in scene.");
            return;
        }

        if (string.IsNullOrWhiteSpace(rewardItemID))
        {
            return;
        }

        if (InventorySystem.Instance.Has(rewardItemID))
        {
            return;
        }

        int configuredAmount = DynamicInventoryUI.GetConfiguredAmountById(rewardItemID);
        if (configuredAmount > 0)
        {
            InventorySystem.Instance.AddTaskReward(rewardItemID, configuredAmount);
        }
        else
        {
            InventorySystem.Instance.AddTaskReward(rewardItemID);
        }
    }

    public static void CloseTaskWindow(GameObject owner, TaskWindowSlide slide, Action onClosed)
    {
        if (slide != null)
        {
            slide.PlaySlideOut(() =>
            {
                owner.SetActive(false);
                GameStateManager.ResetToNormal();
                onClosed?.Invoke();
            });
            return;
        }

        owner.SetActive(false);
        GameStateManager.ResetToNormal();
        onClosed?.Invoke();
    }
}
