using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI_Berry : MonoBehaviour
{
    [Header("UI")]
    public Image berryIconImage;        // 指向你选定的那个slot Image
    public TMP_Text berryCountText;     // 右下角数量TMP

    [Header("Sprite")]
    public Sprite berrySprite;          // 树莓图标

    private void OnEnable()
    {
        // 打开背包时刷新一次
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

        int count = InventorySystem.Instance.GetCount("Berry");

        if (berryIconImage != null)
        {
            if (count > 0)
            {
                berryIconImage.sprite = berrySprite;
                berryIconImage.color = Color.white;
            }
            else
            {
                berryIconImage.sprite = null;
                berryIconImage.color = new Color(1, 1, 1, 0);
            }
        }

        if (berryCountText != null)
            berryCountText.text = count > 1 ? count.ToString() : ""; // 1个可不显示也行
    }
}