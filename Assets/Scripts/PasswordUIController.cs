using System.Collections;
using TMPro;
using UnityEngine;

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
        displayText.text = string.IsNullOrEmpty(currentInput) ? "----" : currentInput;
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
