using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EndingSequenceRunner : MonoBehaviour
{
    [SerializeField] private PlayerScriptUI scriptUI;

    private readonly List<EndingStep> playableSteps = new();
    private EndingPlaybackContext playbackContext;
    private Action sequenceCompleted;
    private int currentIndex;
    private int runVersion;

    public bool IsPlaying { get; private set; }

    public bool Play(EndingSequence sequence, EndingContext endingContext, Action onCompleted)
    {
        if (IsPlaying)
        {
            Debug.LogWarning("The ending sequence runner is already playing.", this);
            return false;
        }

        if (sequence == null)
        {
            Debug.LogError("The ending sequence runner received no sequence.", this);
            return false;
        }

        if (endingContext == null)
        {
            Debug.LogError("The ending sequence runner received no ending context.", this);
            return false;
        }

        if (scriptUI == null)
        {
            Debug.LogError("The ending sequence runner has no PlayerScriptUI.", this);
            return false;
        }

        if (scriptUI.IsShowing)
        {
            Debug.LogWarning("The ending sequence cannot start while another player script is active.", this);
            return false;
        }

        playableSteps.Clear();
        foreach (EndingSequence.Entry entry in sequence.Entries)
        {
            if (entry != null && entry.Step != null && entry.IsSatisfiedBy(endingContext))
                playableSteps.Add(entry.Step);
        }

        IsPlaying = true;
        currentIndex = 0;
        sequenceCompleted = onCompleted;
        playbackContext = new EndingPlaybackContext(scriptUI);
        int activeRun = ++runVersion;
        PlayNext(activeRun);
        return true;
    }

    public void Cancel()
    {
        if (!IsPlaying)
            return;

        runVersion++;
        IsPlaying = false;
        playableSteps.Clear();
        playbackContext = null;
        sequenceCompleted = null;
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void PlayNext(int activeRun)
    {
        if (!IsPlaying || activeRun != runVersion)
            return;

        if (currentIndex >= playableSteps.Count)
        {
            Complete(activeRun);
            return;
        }

        EndingStep step = playableSteps[currentIndex++];
        bool stepCompleted = false;

        void Continue()
        {
            if (stepCompleted)
                return;

            stepCompleted = true;
            PlayNext(activeRun);
        }

        try
        {
            step.Play(playbackContext, Continue);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, step);
            Continue();
        }
    }

    private void Complete(int activeRun)
    {
        if (!IsPlaying || activeRun != runVersion)
            return;

        Action completion = sequenceCompleted;
        IsPlaying = false;
        playableSteps.Clear();
        playbackContext = null;
        sequenceCompleted = null;
        completion?.Invoke();
    }
}
