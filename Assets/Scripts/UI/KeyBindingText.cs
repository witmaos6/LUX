using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[DisallowMultipleComponent]
public class KeyBindingText : MonoBehaviour
{
    [SerializeField] private GameBinding binding = GameBinding.Interaction;
    [SerializeField] private string format = "{0}";
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] LocalizedString keyMessageRef;

    private void Reset()
    {
        FindTextComponent();
        Refresh();
    }

    private void Start()
    {
        FindTextComponent();
        Refresh();
    }

    private void OnEnable()
    {
        KeyBindingSettings.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        KeyBindingSettings.Changed -= Refresh;
    }

    private void OnValidate()
    {
        FindTextComponent();

        if (Application.isPlaying)
            Refresh();
    }

    public void Refresh()
    {
        string keyName = KeyBindingSettings.GetDisplayName(binding);
        string value = keyName;
        if (!string.IsNullOrEmpty(format))
        {
            try
            {
                value = string.Format(format, keyName);
            }
            catch (FormatException)
            {
                value = keyName;
            }
        }

        if (tmpText != null)
        {
            tmpText.text = value;
            if (LocaleService.instance != null && keyMessageRef != null)
            {
                string keyString = LocaleService.instance.GetLocaleText(keyMessageRef);

                tmpText.text += keyString;
            }
        }
    }

    public void SetBinding(GameBinding newBinding)
    {
        binding = newBinding;
        Refresh();
    }

    private void FindTextComponent()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }
}
