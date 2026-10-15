using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocaleText : MonoBehaviour
{
    [SerializeField] LocalizedString messageRef;
    [SerializeField] TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }
    void OnEnable() { messageRef.StringChanged += OnChanged; }
    void OnDisable() { messageRef.StringChanged -= OnChanged; }
    void OnChanged(string s)
    {
        LocaleService.instance.SetLocaleText(label, messageRef);
    }
}
