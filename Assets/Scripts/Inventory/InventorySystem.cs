using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    // 最小实现：只存数量（后面要扩展再升级成ItemData）
    private readonly Dictionary<string, int> counts = new Dictionary<string, int>();

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

    public void Add(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return;

        int cur = GetCount(id);
        counts[id] = cur + amount;

        OnInventoryChanged?.Invoke();
        Debug.Log($"[InventorySystem] Add {id} +{amount} => {counts[id]}");
    }
}