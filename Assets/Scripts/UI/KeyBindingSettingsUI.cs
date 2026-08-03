using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class KeyBindingSettingsUI : MonoBehaviour
{
    [Serializable]
    private class BindingRow
    {
        public GameBinding binding = GameBinding.Interaction;
        public Button rebindButton = null;
        public TMP_Text keyText = null;
    }

    [Header("Canvas UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text hintText;

    [Header("Binding Rows")]
    [SerializeField] private List<BindingRow> bindingRows = new();

    [Header("Behaviour")]
    [SerializeField] private bool hidePanelOnStart = true;

    private IDisposable listenOperation;
    private int inputConsumedFrame = -1;

    public static bool IsCapturingInput { get; private set; }
    public static bool ConsumedGameplayInputThisFrame { get; private set; }
    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;
    public bool IsListening => listenOperation != null;
    public bool ConsumedInputThisFrame => inputConsumedFrame == Time.frameCount;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetBindings);

        foreach (BindingRow row in bindingRows)
        {
            if (row.rebindButton == null)
                continue;

            GameBinding binding = row.binding;
            row.rebindButton.onClick.AddListener(() => BeginListening(binding));
        }

        if (hidePanelOnStart && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        KeyBindingSettings.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        KeyBindingSettings.Changed -= Refresh;
        StopListening();
    }

    public void Open()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        SetHint("변경할 항목을 선택하세요.");
        Refresh();
    }

    public void Close()
    {
        StopListening();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ResetBindings()
    {
        StopListening();
        KeyBindingSettings.ResetAll();
        SetHint("기본 키 설정으로 복원했습니다.");
    }

    public void Refresh()
    {
        foreach (BindingRow row in bindingRows)
        {
            if (row.keyText != null)
                row.keyText.text = KeyBindingSettings.GetDisplayName(row.binding);
        }
    }

    private void BeginListening(GameBinding binding)
    {
        StopListening();
        IsCapturingInput = true;
        ConsumedGameplayInputThisFrame = false;
        SetHint("새 키를 누르세요. (ESC: 취소)");

        listenOperation = InputSystem.onAnyButtonPress.CallOnce(control =>
        {
            listenOperation = null;
            inputConsumedFrame = Time.frameCount;
            IsCapturingInput = false;
            ConsumedGameplayInputThisFrame = true;

            if (control.device is not Keyboard)
            {
                SetHint("키보드 키만 지정할 수 있습니다.");
                return;
            }

            if (control == Keyboard.current.escapeKey)
            {
                SetHint("키 변경을 취소했습니다.");
                return;
            }

            KeyBindingSettings.Set(binding, control.path);
            SetHint("키가 저장되었습니다.");
        });
    }

    private void SetHint(string message)
    {
        if (hintText != null)
            hintText.text = message;
    }

    private void StopListening()
    {
        listenOperation?.Dispose();
        listenOperation = null;
        IsCapturingInput = false;
    }

    private void LateUpdate()
    {
        ConsumedGameplayInputThisFrame = false;
    }

    private void OnDestroy()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(Open);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetBindings);
    }
}
