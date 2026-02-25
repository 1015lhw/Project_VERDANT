using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ItemUISlot
{
    public Image iconImage;
    public TMP_Text countText;
}

[System.Serializable]
public class ItemAsset
{
    public string id;       // 对应 InventorySystem 里的 key (如 "Berry")
    public Sprite sprite;   // 对应的图标
}

public class DynamicInventoryUI : MonoBehaviour
{
    [Header("UI Slots (按顺序拖入你的格子)")]
    public List<ItemUISlot> uiSlots = new List<ItemUISlot>();

    [Header("Item Data Library (配置 ID 对应的图片)")]
    public List<ItemAsset> itemLibrary = new List<ItemAsset>();

    private void OnEnable()
    {
        Refresh();
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (InventorySystem.Instance == null) return;

        // 1. 获取当前所有有数量的物品 ID
        // 注意：InventorySystem 需要提供获取所有 key 的方法，或者我们遍历 Library
        List<string> ownedItems = new List<string>();
        foreach (var asset in itemLibrary)
        {
            if (InventorySystem.Instance.GetCount(asset.id) > 0)
            {
                ownedItems.Add(asset.id);
            }
        }

        // 2. 清空并根据顺序填充格子
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < ownedItems.Count)
            {
                // 这个格子有东西
                string itemId = ownedItems[i];
                UpdateSlot(uiSlots[i], itemId);
            }
            else
            {
                // 这个格子没东西，重置为空
                ResetSlot(uiSlots[i]);
            }
        }
    }

    private void UpdateSlot(ItemUISlot slot, string itemId)
    {
        int count = InventorySystem.Instance.GetCount(itemId);
        Sprite s = itemLibrary.Find(x => x.id == itemId)?.sprite;

        if (slot.iconImage != null)
        {
            slot.iconImage.sprite = s;
            slot.iconImage.color = Color.white;
        }

        if (slot.countText != null)
        {
            slot.countText.text = count > 1 ? count.ToString() : "";
        }
    }

    private void ResetSlot(ItemUISlot slot)
    {
        if (slot.iconImage != null)
        {
            slot.iconImage.sprite = null;
            slot.iconImage.color = new Color(1, 1, 1, 0); // 隐藏
        }
        if (slot.countText != null)
        {
            slot.countText.text = "";
        }
    }
}