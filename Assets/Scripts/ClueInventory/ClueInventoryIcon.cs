using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ClueInventoryIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private Color existColor;

    public void SetExist(LocalizedString displayNameRef)
    {        
        image.color = existColor;
        LocaleService.instance.SetLocaleText(displayName, displayNameRef);
    }

    public bool IsExist()
    {
        return image.color == existColor;
    }
}
