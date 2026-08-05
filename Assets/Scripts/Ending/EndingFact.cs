using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EndingFact", menuName = "LUX2D/Ending/Fact")]
public sealed class EndingFact : ScriptableObject
{
    [SerializeField, HideInInspector] private string id;
    [SerializeField, TextArea] private string description;

    public string Id => id;
    public string Description => description;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(id))
            return;

        id = Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
