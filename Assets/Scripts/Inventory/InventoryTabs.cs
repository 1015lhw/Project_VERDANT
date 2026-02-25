using UnityEngine;

public class InventoryTabs : MonoBehaviour
{
    [Header("Tab Transforms")]
    public RectTransform bagTab;
    public RectTransform repTab;

    [Header("Pages")]
    public GameObject bagPage;
    public GameObject repPage;

    [Header("Position Settings")]
    [Tooltip("选中状态下的 Y 轴坐标")]
    public float highY = 20f; 
    [Tooltip("未选中状态下的 Y 轴坐标")]
    public float lowY = 0f;

    private bool isBagSelected = false;

    void Start()
    {
        // 强制初始化：先设为相反状态，确保首次打开时位置刷新
        isBagSelected = false; 
        SelectBag();
    }

    public void SelectBag()
    {
        if (isBagSelected) return;
        isBagSelected = true;

        ApplyPositions();
        
        // 切换页面显隐
        if (bagPage != null) bagPage.SetActive(true);
        if (repPage != null) repPage.SetActive(false);
    }

    public void SelectRep()
    {
        if (!isBagSelected) return;
        isBagSelected = false;

        ApplyPositions();

        // 切换页面显隐
        if (bagPage != null) bagPage.SetActive(false);
        if (repPage != null) repPage.SetActive(true);
    }

    private void ApplyPositions()
    {
        // 安全检查：确保拖入了引用
        if (bagTab == null || repTab == null)
        {
            Debug.LogError("InventoryTabs: 请在 Inspector 中拖入 BagTab 和 RepTab 的引用！");
            return;
        }

        // 使用绝对坐标赋值，不受原本偏移量干扰
        // Bag 选中时用 High，Rep 用 Low；反之亦然
        bagTab.anchoredPosition = new Vector2(bagTab.anchoredPosition.x, isBagSelected ? highY : lowY);
        repTab.anchoredPosition = new Vector2(repTab.anchoredPosition.x, isBagSelected ? lowY : highY);
    }
}