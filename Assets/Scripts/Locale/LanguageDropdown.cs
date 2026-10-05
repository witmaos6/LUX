using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;

    static readonly (string code, string label)[] Options =
    {
        ("ko", "ÇÑ±¹¾î"),
        ("en", "English")
    };

    private void Start()
    {
        dropdown.ClearOptions();

        foreach(var option in Options)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option.label));
        }

        string current = UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code;

        int Index = System.Array.FindIndex(Options, option => option.code == current);
        dropdown.SetValueWithoutNotify(Mathf.Max(Index, 0));
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.AddListener(i => LocaleService.Apply(Options[i].code));
    }
}
