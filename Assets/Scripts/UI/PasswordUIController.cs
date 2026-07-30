using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PasswordUIController : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public int maxDisplayLength = 4;

    private string currentInput = "";

    private PasswordUnlockObject currentActiveLock;

    public void Init(PasswordUnlockObject targetLock)
    {
        currentActiveLock = targetLock;
        currentInput = "";
        displayText.color = Color.black;

        UpdateDisplay();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        for (int number = 0; number <= 9; number++)
        {
            if (IsNumberKeyPressed(keyboard, number))
            {
                ReceiveButtonInput(KeypadButtonType.Number, number.ToString());
                return;
            }
        }

        if (keyboard.backspaceKey.wasPressedThisFrame || keyboard.deleteKey.wasPressedThisFrame)
        {
            ReceiveButtonInput(KeypadButtonType.Clear, string.Empty);
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            ReceiveButtonInput(KeypadButtonType.Confirm, string.Empty);
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            ReceiveButtonInput(KeypadButtonType.Cancel, string.Empty);
        }
    }

    private static bool IsNumberKeyPressed(Keyboard keyboard, int number)
    {
        return number switch
        {
            0 => keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame,
            1 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
            2 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
            3 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
            4 => keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame,
            5 => keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame,
            6 => keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame,
            7 => keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame,
            8 => keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame,
            9 => keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame,
            _ => false
        };
    }

    public void ReceiveButtonInput(KeypadButtonType type, string value)
    {
        switch (type)
        {
            case KeypadButtonType.Number:
                if (currentInput.Length < maxDisplayLength)
                {
                    currentInput += value;
                    UpdateDisplay();
                }
                break;

            case KeypadButtonType.Clear:
                currentInput = "";
                UpdateDisplay();
                break;

            case KeypadButtonType.Confirm:
                ExecuteVerification();
                break;

            case KeypadButtonType.Cancel:
                CloseAndDestroyUI();
                break;
        }
    }

    private void UpdateDisplay()
    {
        string space = new string('-', maxDisplayLength);
        displayText.text = string.IsNullOrEmpty(currentInput) ? space : currentInput;
    }

    private void ExecuteVerification()
    {
        if (currentActiveLock == null) return;

        bool isSuccess = currentActiveLock.CheckPassword(currentInput);

        if (isSuccess)
        {
            HandleSuccess();
        }
        else
        {
            HandleFailure();
        }
    }

    private void HandleSuccess()
    {
        displayText.color = Color.green;
        displayText.text = "OPEN";

        Invoke(nameof(CloseAndDestroyUI), 1.0f);
    }

    private void HandleFailure()
    {
        currentInput = "";
        displayText.color = Color.red;
        displayText.text = "WRONG";

        StartCoroutine(ResetDisplayAfterDelay(0.8f));
    }

    private IEnumerator ResetDisplayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        displayText.color = Color.black;
        UpdateDisplay();
    }

    public void CloseAndDestroyUI()
    {
        Destroy(gameObject);
    }
}
