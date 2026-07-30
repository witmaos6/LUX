using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerScriptUI : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private GameEvent showScriptEvent;

    [Header("Script")]
    [SerializeField] private PlayerScriptSequence scriptSequence;

    [Header("UI")]
    [SerializeField] private GameObject scriptUI;
    [SerializeField] private TMP_Text scriptText;

    [Header("Completion")]
    [SerializeField] private UnityEvent onScriptCompleted;

    private InputAction nextLineAction;
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
        if (showScriptEvent != null)
            GameEventManager.Subscribe(showScriptEvent, Show);
    }

    private void OnDisable()
    {
        if (showScriptEvent != null)
            GameEventManager.Unsubscribe(showScriptEvent, Show);

        Hide();
    }

    private void OnDestroy()
    {
        nextLineAction.performed -= OnNextLine;
        nextLineAction.Dispose();
    }

    public void Show()
    {
        if (scriptSequence == null || scriptSequence.LineCount == 0)
        {
            Debug.LogWarning("표시할 플레이어 스크립트가 없습니다.", this);
            return;
        }

        if (scriptUI == null || scriptText == null)
        {
            Debug.LogError("플레이어 스크립트 UI 참조가 설정되지 않았습니다.", this);
            return;
        }

        currentLineIndex = 0;
        isShowing = true;
        scriptUI.SetActive(true);
        scriptText.text = scriptSequence.GetLine(currentLineIndex);
        nextLineAction.Enable();
    }

    public void Hide()
    {
        isShowing = false;
        nextLineAction.Disable();

        if (scriptUI != null)
            scriptUI.SetActive(false);
    }

    private void OnNextLine(InputAction.CallbackContext context)
    {
        if (!isShowing)
            return;

        currentLineIndex++;

        if (currentLineIndex >= scriptSequence.LineCount)
        {
            Complete();
            return;
        }

        scriptText.text = scriptSequence.GetLine(currentLineIndex);
    }

    private void Complete()
    {
        Hide();
        onScriptCompleted?.Invoke();
    }
}
