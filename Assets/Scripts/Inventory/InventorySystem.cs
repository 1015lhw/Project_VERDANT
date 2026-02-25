using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    // 存储物品数量
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();
    
    // ⭐ 新增：记录物品获取的先后顺序
    private readonly List<string> itemOrder = new List<string>();

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetCount(string id)
    {
        return counts.TryGetValue(id, out int v) ? v : 0;
    }

    // ⭐ 新增：获取按获取顺序排列的已拥有物品 ID 列表
    public List<string> GetOwnedItemsInOrder()
    {
        // 过滤掉数量为 0 的物品（如果以后有丢弃功能）
        return itemOrder.FindAll(id => GetCount(id) > 0);
    }

    public void Add(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return;

        // ⭐ 逻辑修改：如果是第一次获得该物品，记录到顺序列表中
        if (!counts.ContainsKey(id))
        {
            itemOrder.Add(id);
        }

        int cur = GetCount(id);
        counts[id] = cur + amount;

        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventorySystem] Add {id} +{amount} => {counts[id]} | Order: {string.Join(", ", itemOrder)}");
    }
}