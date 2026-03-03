using System;
using System.Collections.Generic;
using UnityEngine;


public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    // 存储数量
    private Dictionary<string, int> counts = new Dictionary<string, int>();

    // 记录首次获得顺序
    private List<string> itemOrder = new List<string>();

    public event Action OnInventoryChanged;
    public event Action<bool> OnTaskRewardNotificationChanged;
    public event Action<string, int> OnTaskRewardAdded;

    private bool hasUnseenTaskReward;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== 原有方法：添加物品 =====
    public void Add(string id, int amount)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (amount <= 0) return;

        if (counts.ContainsKey(id))
        {
            counts[id] += amount;
        }
        else
        {
            counts[id] = amount;
            itemOrder.Add(id); // 首次获得时记录顺序
        }

        Debug.Log($"[Inventory] Added {amount} x {id} (Total: {counts[id]})");

        OnInventoryChanged?.Invoke();
    }

    // ===== 原有方法：移除物品 =====
    public void Remove(string id, int amount)
    {
        if (!counts.ContainsKey(id)) return;

        counts[id] -= amount;

        if (counts[id] <= 0)
        {
            counts.Remove(id);
            itemOrder.Remove(id);
        }

        OnInventoryChanged?.Invoke();
    }

    // ===== 原有方法：获取数量 =====
    public int GetCount(string id)
    {
        return counts.ContainsKey(id) ? counts[id] : 0;
    }

    // ===== 原有方法：是否拥有 =====
    public bool Has(string id)
    {
        return counts.ContainsKey(id) && counts[id] > 0;
    }

    // ===== 兼容方法：按拥有顺序获取物品 =====
    // DynamicInventoryUI 依赖该方法名，返回现有顺序列表即可。
    public List<string> GetOwnedItemsInOrder()
    {
        return itemOrder;
    }

    // 任务奖励入口：添加物品并触发背包红点
    public void AddTaskReward(string id, int amount = 1)
    {
        Add(id, amount);
        SetTaskRewardNotification(true);
        OnTaskRewardAdded?.Invoke(id, amount);
    }

    public bool HasUnseenTaskReward => hasUnseenTaskReward;

    public void MarkTaskRewardSeen()
    {
        SetTaskRewardNotification(false);
    }

    private void SetTaskRewardNotification(bool value)
    {
        if (hasUnseenTaskReward == value)
        {
            return;
        }

        hasUnseenTaskReward = value;
        OnTaskRewardNotificationChanged?.Invoke(hasUnseenTaskReward);
    }
}
