using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleService : MonoBehaviour
{
    const string PrefKey = "lux.locale";

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

    static string DetectDefault() => Application.systemLanguage == SystemLanguage.Korean ? "ko" : "en";

    public static void Apply(string code, bool save = true)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale == null)
            return;

        LocalizationSettings.SelectedLocale = locale;
        if(save)
        {
            PlayerPrefs.SetString(PrefKey, code);
        }
    }

    /* 코드로 텍스트를 넣을 때 추가할 코드
     * [SerializeField] LocalizedString messageRef;   // Inspector에서 테이블/키 지정
     * [SerializeField] TMP_Text label;
     * void OnEnable()  { messageRef.StringChanged += OnChanged; }
     * void OnDisable() { messageRef.StringChanged -= OnChanged; }
     * void OnChanged(string s) => label.text = s;
     */
}
