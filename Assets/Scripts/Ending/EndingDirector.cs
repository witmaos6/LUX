using UnityEngine;
using UnityEngine.Events;

public sealed class EndingDirector : MonoBehaviour
{
    [SerializeField] private GameEvent endingEvent;
    [SerializeField] private EndingSequence endingSequence;
    [SerializeField] private EndingSequenceRunner sequenceRunner;
    [SerializeField] private UnityEvent onEndingCompleted;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (sequenceRunner == null)
            sequenceRunner = GetComponent<EndingSequenceRunner>();
    }

    private void OnEnable()
    {
        if (endingEvent != null)
            GameEventManager.Subscribe(endingEvent, BeginEnding);
    }

    private void OnDisable()
    {
        if (endingEvent != null)
            GameEventManager.Unsubscribe(endingEvent, BeginEnding);

        IsPlaying = false;
    }

    public void BeginEnding()
    {
        if (IsPlaying)
            return;

        if (endingSequence == null || sequenceRunner == null)
        {
            Debug.LogError("The ending director is not configured.", this);
            return;
        }

        EndingContext context = new EndingContext(EndingProgress.CreateSnapshot());
        IsPlaying = true;

        if (!sequenceRunner.Play(endingSequence, context, CompleteEnding))
        {
            IsPlaying = false;
            Debug.LogError("The ending sequence could not be started.", this);
        }
    }

    private void CompleteEnding()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;
        onEndingCompleted?.Invoke();
    }
}
