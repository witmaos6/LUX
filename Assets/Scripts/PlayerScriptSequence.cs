using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(
    fileName = "PlayerScriptSequence",
    menuName = "Player Script/Sequence")]
public class PlayerScriptSequence : ScriptableObject
{
    [Serializable]
    public struct Line
    {
        public LocalizedString messageRef;
    }

    [SerializeField] private Line[] lines;

    public int LineCount => lines?.Length ?? 0;

    public string GetLine(int index)
    {
        if (index < 0 || index >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        Line line = lines[index];
        string result = line.messageRef.GetLocalizedString();
        return result;
    }
}
