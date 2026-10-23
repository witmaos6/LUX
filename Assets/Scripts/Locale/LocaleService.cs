using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocaleService : MonoBehaviour
{
    static public LocaleService instance;

    [SerializeField] private TextProfileDatabase textProfileDatabase;
    [SerializeField] private CountryTextProfileTable countryTextProfileTable;

    private void Awake()
    {
        instance = this;
    }

    public string GetTableName()
    {
        return TableName;
    }

    const string PrefKey = "lux.locale";
    private const string TableName = "LocaleTable";

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        string code = PlayerPrefs.GetString(PrefKey, "");
        if(string.IsNullOrEmpty(code))
        {
            code = DetectDefault();
        }

        Apply(code, save: false);
    }

    static string DetectDefault() =>Application.systemLanguage == SystemLanguage.Korean ? "ko" : "en";

    public static void Apply(string code, bool save = true)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale == null)
        {
            Debug.Log($"{code} is invalid");
            return;
        }

        LocalizationSettings.SelectedLocale = locale;
        if(save)
        {
            PlayerPrefs.SetString(PrefKey, code);
        }
    }

    public void SetLocaleText(TMP_Text setText, LocalizedString messageRef)
    {
        string Key = GetKey(messageRef);
        if(string.IsNullOrEmpty(Key))
        {
            Debug.LogWarning("Key is null");
            return;
        }

        string profileName = textProfileDatabase.GetProfile(Key);
        if(string.IsNullOrEmpty(profileName))
        {
            Debug.LogWarning("ProfileId is null");
            return;
        }

        string localeCode = GetSelectedLocale();
        if(string.IsNullOrEmpty(localeCode))
        {
            Debug.LogWarning("selected Locale is null");
            return;
        }

        CountryTextProfileTable.Property property = countryTextProfileTable.GetProperty(localeCode, profileName);   
        if(property == null)
        {
            Debug.LogWarning("property is null");
            return;
        }

        string localizedString = messageRef.GetLocalizedString();
        if(string.IsNullOrEmpty(localizedString))
        {
            Debug.LogWarning("locale string is null");
            return;
        }

        setText.text = localizedString;
        setText.fontSize = property.fontSize;
    }

    string GetKey(LocalizedString messageRef)
    {
        var tableEntry = LocalizationSettings.StringDatabase.GetTableEntry(messageRef.TableReference, messageRef.TableEntryReference);
        return tableEntry.Entry.SharedEntry.Key;
    }

    string GetSelectedLocale()
    {
        var currentLocale = LocalizationSettings.SelectedLocale;
        if(currentLocale)
        {
            return currentLocale.Identifier.Code;
        }
        return null;
    }

    /* 코드로 텍스트를 넣을 때 추가할 코드
     * [SerializeField] LocalizedString messageRef;   // Inspector에서 테이블/키 지정
     * [SerializeField] TMP_Text label;
     * void OnEnable()  { messageRef.StringChanged += OnChanged; }
     * void OnDisable() { messageRef.StringChanged -= OnChanged; }
     * void OnChanged(string s) => label.text = s;
     */
}
