using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

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

    public void SetText(TMP_Text text, int index)
    {
        if (index < 0 || index >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        Line line = lines[index];
        LocaleService.instance.SetLocaleText(text, line.messageRef);
    }
}
