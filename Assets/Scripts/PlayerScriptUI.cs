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
    [SerializeField] private PlayerController playerController;

    [Header("Completion")]
    [SerializeField] private UnityEvent onScriptCompleted;

    private InputAction nextLineAction;
    private readonly List<(GameEvent gameEvent, Action handler)> subscriptions = new();
    private PlayerScriptSequence activeSequence;
    private Action activeCompletion;
    private int currentLineIndex;
    private bool isShowing;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

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
        TryPlay(sequence, null);
    }

    public bool TryPlay(PlayerScriptSequence sequence, Action onCompleted)
    {
        if (isShowing)
        {
            Debug.LogWarning("A player script is already being displayed.", this);
            return false;
        }

        if (sequence == null || sequence.LineCount == 0)
        {
            Debug.LogWarning("The player script sequence is empty.", this);
            return false;
        }

        if (scriptUI == null || scriptText == null)
        {
            Debug.LogError("The player script UI references are not configured.", this);
            return false;
        }

        activeSequence = sequence;
        activeCompletion = onCompleted;
        currentLineIndex = 0;
        isShowing = true;

        playerController?.EnterScriptMode();
        scriptUI.SetActive(true);
        scriptText.text = activeSequence.GetLine(currentLineIndex);
        nextLineAction.Enable();
        return true;
    }

    public void Hide()
    {
        bool wasShowing = isShowing;
        isShowing = false;
        activeSequence = null;
        activeCompletion = null;
        nextLineAction.Disable();

        if (scriptUI != null)
            scriptUI.SetActive(false);

        if (wasShowing)
            playerController?.ExitScriptMode();
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
        Action completion = activeCompletion;
        Hide();
        onScriptCompleted?.Invoke();
        completion?.Invoke();
    }
}
