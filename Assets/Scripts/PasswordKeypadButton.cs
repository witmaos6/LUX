using UnityEngine;
using UnityEngine.UI;

public enum KeypadButtonType { Number, Clear, Confirm, Cancel }

public class PasswordKeypadButton : MonoBehaviour
{
    public KeypadButtonType buttonType;
    public string numberValue;

    private PasswordUIController uiController;

    private void Awake()
    {
        uiController = GetComponentInParent<PasswordUIController>();

        Button button = GetComponent<Button>();
        if(button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    public void OnButtonClick()
    {
        if(uiController != null)
        {
            uiController.ReceiveButtonInput(buttonType, numberValue);
        }
    }
}
