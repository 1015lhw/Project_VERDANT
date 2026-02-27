using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ItemUISlot
{
    public Image iconImage;
    public TMP_Text countText;
    public Button clickButton;
}

[System.Serializable]
public class ItemAsset
{
    public string id;       // 对应 InventorySystem 里的 key (如 "Berry")
    public Sprite sprite;   // 左侧格子图标
    public Sprite detailSprite; // 右侧详情大图（为空时回退 sprite）
    public string displayName;
    [TextArea]
    public string description;
}

public class DynamicInventoryUI : MonoBehaviour
{
    [Header("UI Slots (按顺序拖入你的格子)")]
    public List<ItemUISlot> uiSlots = new List<ItemUISlot>();

    [Header("Item Data Library (配置 ID 对应的图片)")]
    public List<ItemAsset> itemLibrary = new List<ItemAsset>();

    [Header("Detail Panel (右侧详情区)")]
    public Image detailIconImage;
    public TMP_Text detailNameText;
    public TMP_Text detailDescriptionText;

    [Header("Detail Icon Display Options")]
    public bool detailPreserveAspect = true;
    public bool detailUseNativeSize = false;

    private InventorySystem inventory;
    private readonly List<string> currentOwnedItems = new List<string>();
    private int selectedIndex = -1;

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
        currentOwnedItems.Clear();
        currentOwnedItems.AddRange(ownedItems);

        bool selectionKept = selectedIndex >= 0 && selectedIndex < ownedItems.Count;
        if (!selectionKept)
        {
            // 打开背包时如果有物品，默认选中左上角第一个
            selectedIndex = ownedItems.Count > 0 ? 0 : -1;
        }

        // 2. 清空并根据顺序填充格子
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < ownedItems.Count)
            {
                // 这个格子有东西
                string itemId = ownedItems[i];
                UpdateSlot(uiSlots[i], itemId, i);
            }
            else
            {
                // 这个格子没东西，重置为空
                ResetSlot(uiSlots[i]);
            }
        }

        UpdateDetailPanel();
    }

    private void UpdateSlot(ItemUISlot slot, string itemId, int slotIndex)
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

        if (slot.clickButton != null)
        {
            slot.clickButton.interactable = true;
            slot.clickButton.onClick.RemoveAllListeners();
            slot.clickButton.onClick.AddListener(() => SelectIndex(slotIndex));
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

        if (slot.clickButton != null)
        {
            slot.clickButton.interactable = false;
            slot.clickButton.onClick.RemoveAllListeners();
        }
    }

    private void SelectIndex(int index)
    {
        if (index < 0 || index >= currentOwnedItems.Count)
        {
            return;
        }

        selectedIndex = index;
        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        if (selectedIndex < 0 || selectedIndex >= currentOwnedItems.Count)
        {
            ClearDetailPanel();
            return;
        }

        string itemId = currentOwnedItems[selectedIndex];
        ItemAsset itemData = itemLibrary.Find(x => x.id == itemId);

        Sprite detailSprite = itemData != null && itemData.detailSprite != null ? itemData.detailSprite : itemData?.sprite;
        if (detailIconImage != null)
        {
            detailIconImage.sprite = detailSprite;
            detailIconImage.preserveAspect = detailPreserveAspect;

            if (detailSprite != null && detailUseNativeSize)
            {
                detailIconImage.SetNativeSize();
            }

            detailIconImage.color = detailSprite != null ? Color.white : new Color(1, 1, 1, 0);
        }

        if (detailNameText != null)
        {
            detailNameText.text = itemData != null && !string.IsNullOrEmpty(itemData.displayName) ? itemData.displayName : itemId;
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = itemData != null ? itemData.description : string.Empty;
        }
    }

    private void ClearDetailPanel()
    {
        if (detailIconImage != null)
        {
            detailIconImage.sprite = null;
            detailIconImage.color = new Color(1, 1, 1, 0);
        }

        if (detailNameText != null)
        {
            detailNameText.text = "";
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = "";
        }
    }
}
