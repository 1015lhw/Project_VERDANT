using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskRewardPopupEntry : MonoBehaviour
{
    [Header("Template Bindings")]
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;

    public void SetContent(string itemName, int amount, Sprite icon, string amountPrefix)
    {
        if (amountText != null)
        {
            amountText.text = $"{amountPrefix}{amount}";
        }

        if (nameText != null)
        {
            nameText.text = itemName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = icon != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        }
    }
}