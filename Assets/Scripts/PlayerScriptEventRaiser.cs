using UnityEngine;

public class PlayerScriptEventRaiser : MonoBehaviour
{
    [SerializeField] private GameEvent showScriptEvent;

    public void Raise()
    {
        if (showScriptEvent == null)
        {
            Debug.LogError("The player script is not configured.", this);
            return;
        }

        GameEventManager.Raise(showScriptEvent);
    }
}
