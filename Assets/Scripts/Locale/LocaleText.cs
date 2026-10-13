using TMPro;
using UnityEngine;
using UnityEngine.Localization;

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
    void OnChanged(string s) => label.text = s;
}
