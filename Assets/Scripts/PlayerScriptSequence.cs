using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerScriptSequence",
    menuName = "Player Script/Sequence")]
public class PlayerScriptSequence : ScriptableObject
{
    [Serializable]
    public struct Line
    {
        [TextArea(2, 5)]
        public string text;
    }

    [SerializeField] private Line[] lines;

    public int LineCount => lines?.Length ?? 0;

    public string GetLine(int index)
    {
        if (index < 0 || index >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        return lines[index].text;
    }
}
