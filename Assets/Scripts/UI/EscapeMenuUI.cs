using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EscapeMenuUI : MonoBehaviour
{
    [Header("Canvas UI References")]
    [SerializeField] private GameObject escapeMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button keySettingsButton;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private KeyBindingSettingsUI keySettingsUI;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private bool pauseGameWhileOpen = true;

    private float timeScaleBeforePause = 1f;
    private bool pausedByMenu;
    private InputAction toggleMenuAction;

    public bool IsOpen => escapeMenuPanel != null && escapeMenuPanel.activeSelf;

    private void Awake()
    {
        toggleMenuAction = new InputAction(
            "EscapeMenu",
            InputActionType.Button,
            KeyBindingSettings.GetPath(GameBinding.EscapeMenu));
        toggleMenuAction.performed += OnToggleMenu;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (keySettingsButton != null)
            keySettingsButton.onClick.AddListener(OpenKeySettings);

        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(ReturnToLobby);

        if (escapeMenuPanel != null)
            escapeMenuPanel.SetActive(false);
    }

    private void OnEnable()
    {
        KeyBindingSettings.Changed += RefreshToggleBinding;
        toggleMenuAction?.Enable();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool escapePressed = keyboard.escapeKey.wasPressedThisFrame;
        if (!escapePressed)
            return;

        // A key pressed for rebinding must not also operate this menu.
        if (keySettingsUI != null &&
            (keySettingsUI.IsListening || keySettingsUI.ConsumedInputThisFrame))
            return;

        if (keySettingsUI != null && keySettingsUI.IsOpen)
        {
            keySettingsUI.Close();
            return;
        }

        if (IsOpen)
            Resume();
        else
            Open();
    }

    public void Open()
    {
        if (escapeMenuPanel == null)
            return;

        escapeMenuPanel.SetActive(true);

        if (pauseGameWhileOpen && !pausedByMenu)
        {
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            pausedByMenu = true;
        }
    }

    public void Resume()
    {
        if (keySettingsUI != null && keySettingsUI.IsOpen)
            keySettingsUI.Close();

        if (escapeMenuPanel != null)
            escapeMenuPanel.SetActive(false);

        RestoreTimeScale();
    }

    public void OpenKeySettings()
    {
        if (keySettingsUI != null)
            keySettingsUI.Open();
    }

    public void ReturnToLobby()
    {
        RestoreTimeScale();
        SaveManager.BeginSceneTransition();
        SceneManager.LoadScene(lobbySceneName);
    }

    private void OnDisable()
    {
        KeyBindingSettings.Changed -= RefreshToggleBinding;
        toggleMenuAction?.Disable();
        RestoreTimeScale();
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        // A key captured for rebinding must not also operate this menu.
        if (keySettingsUI != null &&
            (keySettingsUI.IsListening || keySettingsUI.ConsumedInputThisFrame))
            return;

        if (IsOpen)
            Resume();
        else
            Open();
    }

    private void RefreshToggleBinding()
    {
        if (toggleMenuAction == null)
            return;

        bool wasEnabled = toggleMenuAction.enabled;
        toggleMenuAction.Disable();
        toggleMenuAction.ApplyBindingOverride(0, KeyBindingSettings.GetPath(GameBinding.EscapeMenu));

        if (wasEnabled)
            toggleMenuAction.Enable();
    }

    private void OnDestroy()
    {
        RestoreTimeScale();

        if (toggleMenuAction != null)
        {
            toggleMenuAction.performed -= OnToggleMenu;
            toggleMenuAction.Dispose();
        }

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);

        if (keySettingsButton != null)
            keySettingsButton.onClick.RemoveListener(OpenKeySettings);

        if (lobbyButton != null)
            lobbyButton.onClick.RemoveListener(ReturnToLobby);
    }

    private void RestoreTimeScale()
    {
        if (!pausedByMenu)
            return;

        Time.timeScale = timeScaleBeforePause;
        pausedByMenu = false;
    }
}
