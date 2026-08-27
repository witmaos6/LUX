using UnityEngine;

public interface ISuspicionReceiver
{
    void AddSuspicion(Vector3 sourcePosition, float range, float strength);
}
