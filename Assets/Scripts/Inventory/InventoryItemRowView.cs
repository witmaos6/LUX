using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DropItem;

/// <summary>References authored on one reusable inventory-item row prefab.</summary>
public sealed class InventoryItemRowView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    public void SetItem(ItemCode item, InventoryItemDatabase database)
    {
        Sprite icon = database != null ? database.GetIcon(item) : null;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (nameText != null)
        {
            nameText.text = database != null ? database.GetDisplayName(item) : item.ToString();
            if (database != null && database.UIFont != null) nameText.font = database.UIFont;
        }

        if (descriptionText != null)
        {
            descriptionText.text = database != null ? database.GetDescription(item) : string.Empty;
            if (database != null && database.UIFont != null) descriptionText.font = database.UIFont;
        }
    }
}
