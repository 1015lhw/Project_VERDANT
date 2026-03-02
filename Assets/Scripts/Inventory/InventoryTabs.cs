using UnityEngine;

public class InventoryTabs : MonoBehaviour
{
    private enum InventoryTab
    {
        Bag,
        Reputation,
        Objectives
    }

    [Header("Tab Transforms")]
    public RectTransform bagTab;
    public RectTransform repTab;
    public RectTransform objTab;

    [Header("Pages")]
    public GameObject bagPage;
    public GameObject repPage;
    public GameObject objPage;

    [Header("Position Settings")]
    [Tooltip("选中状态下的 Y 轴坐标")]
    public float highY = 20f; 
    [Tooltip("未选中状态下的 Y 轴坐标")]
    public float lowY = 0f;

    private InventoryTab selectedTab = InventoryTab.Reputation;

    void Start()
    {
        SelectBag();
    }

    public void SelectBag()
    {
        SelectTab(InventoryTab.Bag);
    }

    public void SelectRep()
    {
        SelectTab(InventoryTab.Reputation);
    }

    public void SelectObjectives()
    {
        SelectTab(InventoryTab.Objectives);
    }

    private void SelectTab(InventoryTab targetTab)
    {
        if (selectedTab == targetTab) return;

        selectedTab = targetTab;
        ApplyTabVisualState();
        ApplyPageVisibility();
    }

    private void ApplyTabVisualState()
    {
        if (bagTab == null || repTab == null)
        {
            Debug.LogError("InventoryTabs: 请在 Inspector 中拖入 BagTab 和 RepTab 的引用！");
            return;
        }

        bagTab.anchoredPosition = new Vector2(
            bagTab.anchoredPosition.x,
            selectedTab == InventoryTab.Bag ? highY : lowY
        );

        repTab.anchoredPosition = new Vector2(
            repTab.anchoredPosition.x,
            selectedTab == InventoryTab.Reputation ? highY : lowY
        );

        if (objTab != null)
        {
            objTab.anchoredPosition = new Vector2(
                objTab.anchoredPosition.x,
                selectedTab == InventoryTab.Objectives ? highY : lowY
            );
        }
    }

    private void ApplyPageVisibility()
    {
        if (bagPage != null) bagPage.SetActive(selectedTab == InventoryTab.Bag);
        if (repPage != null) repPage.SetActive(selectedTab == InventoryTab.Reputation);
        if (objPage != null) objPage.SetActive(selectedTab == InventoryTab.Objectives);
    }
}
