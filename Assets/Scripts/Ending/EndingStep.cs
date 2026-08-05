using System;
using UnityEngine;

public abstract class EndingStep : ScriptableObject
{
    public abstract void Play(EndingPlaybackContext context, Action onCompleted);
}
