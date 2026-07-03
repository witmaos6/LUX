using UnityEngine;

public class SaveableId : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;

    public void SetId(string newId)
    {
        id = newId;

        if (SaveManager.IsDestroyed(id))
            Destroy(gameObject);
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(id)) return;

        if (SaveManager.IsDestroyed(id))
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (string.IsNullOrEmpty(id)) return;
        if (SaveManager.IsQuitting) return;

        SaveManager.MarkDestroyed(id);
    }

#if UNITY_EDITOR
    [ContextMenu("Generate New Id")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
