using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerScriptUI : MonoBehaviour
{
    [Serializable]
    private struct ScriptEntry
    {
        public GameEvent showEvent;
        public PlayerScriptSequence sequence;
    }

    [Header("Scripts")]
    [SerializeField] private ScriptEntry[] scriptEntries = Array.Empty<ScriptEntry>();

    [Header("UI")]
    [SerializeField] private GameObject scriptUI;
    [SerializeField] private TMP_Text scriptText;

    [Header("Completion")]
    [SerializeField] private UnityEvent onScriptCompleted;

    private InputAction nextLineAction;
    private readonly List<(GameEvent gameEvent, Action handler)> subscriptions = new();
    private PlayerScriptSequence activeSequence;
    private int currentLineIndex;
    private bool isShowing;

    private void Awake()
    {
        nextLineAction = new InputAction(
            name: "Next Script",
            type: InputActionType.Button,
            binding: "<Keyboard>/space");

        nextLineAction.performed += OnNextLine;

        if (scriptUI != null)
            scriptUI.SetActive(false);
    }

    private void OnEnable()
    {
        foreach (ScriptEntry entry in scriptEntries)
        {
            if (entry.showEvent == null)
                continue;

            PlayerScriptSequence sequence = entry.sequence;
            Action handler = () => Show(sequence);

            GameEventManager.Subscribe(entry.showEvent, handler);
            subscriptions.Add((entry.showEvent, handler));
        }
    }

    private void OnDisable()
    {
        foreach ((GameEvent gameEvent, Action handler) in subscriptions)
            GameEventManager.Unsubscribe(gameEvent, handler);

        subscriptions.Clear();
        Hide();
    }

    private void OnDestroy()
    {
        nextLineAction.performed -= OnNextLine;
        nextLineAction.Dispose();
    }

    public void Show(PlayerScriptSequence sequence)
    {
        if (sequence == null || sequence.LineCount == 0)
        {
            Debug.LogWarning("표시할 플레이어 스크립트가 없습니다.", this);
            return;
        }

        if (scriptUI == null || scriptText == null)
        {
            Debug.LogError("플레이어 스크립트 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

        activeSequence = sequence;
        currentLineIndex = 0;
        isShowing = true;
        scriptUI.SetActive(true);
        scriptText.text = activeSequence.GetLine(currentLineIndex);
        nextLineAction.Enable();
    }

    public void Hide()
    {
        isShowing = false;
        activeSequence = null;
        nextLineAction.Disable();

        if (scriptUI != null)
            scriptUI.SetActive(false);
    }

    private void OnNextLine(InputAction.CallbackContext context)
    {
        if (!isShowing)
            return;

        currentLineIndex++;

        if (activeSequence == null || currentLineIndex >= activeSequence.LineCount)
        {
            Complete();
            return;
        }

        scriptText.text = activeSequence.GetLine(currentLineIndex);
    }

    private void Complete()
    {
        Hide();
        onScriptCompleted?.Invoke();
    }
}
