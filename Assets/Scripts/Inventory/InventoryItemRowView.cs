using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using static DropItem;

public sealed class InventoryItemRowView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    public void SetIcon(Sprite icon)
    {
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }

    public void SetNameText(LocalizedString localizedString)
    {
        LocaleService.instance.SetLocaleText(nameText, localizedString);
    }

    public void SetDescriptionText(LocalizedString localizedString)
    {
        LocaleService.instance.SetLocaleText(descriptionText, localizedString);
    }
}
