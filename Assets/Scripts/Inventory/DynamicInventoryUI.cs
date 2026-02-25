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

    private InventorySystem inventory;

    private void OnEnable()
    {
        TryBindInventory();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        inventory = null;
    }

    private void TryBindInventory()
    {
        if (inventory == InventorySystem.Instance) return;

        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        inventory = InventorySystem.Instance;

        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;
    }

    public void Refresh()
    {
        TryBindInventory();
        if (inventory == null) return;

        // 1. 按实际获取顺序拿到“背包里真正拥有”的物品
        List<string> ownedItems = inventory.GetOwnedItemsInOrder();

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
            slot.countText.text = count.ToString();
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
