using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptEndingStep", menuName = "LUX2D/Ending/Steps/Script")]
public sealed class ScriptEndingStep : EndingStep
{
    [SerializeField] private PlayerScriptSequence sequence;

    public override void Play(EndingPlaybackContext context, Action onCompleted)
    {
        if (context?.ScriptUI == null)
        {
            Debug.LogError("The ending script UI is not configured.", this);
            onCompleted?.Invoke();
            return;
        }

        if (!context.ScriptUI.TryPlay(sequence, onCompleted))
        {
            Debug.LogWarning($"Ending script step '{name}' could not be played.", this);
            onCompleted?.Invoke();
        }
    }
}
