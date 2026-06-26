using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject { }

[CreateAssetMenu(menuName = "Events/Spawn Event")]
public class SpawnEvent : GameEvent { }

public struct SpawnEventData
{
    public GameObject spawnPrefab;
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
}
