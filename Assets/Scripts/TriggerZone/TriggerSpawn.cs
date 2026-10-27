using System.Collections;
using UnityEngine;

public class TriggerSpawn : TriggerCollision
{
    enum SpawnPointSetting
    {
        CollisionObjectSpawn,
        TriggerPointSpawn,
        // 필요하면 스폰 지점을 수동으로 설정하는 것 추가
    }
    [SerializeField] private GameObject spawnObject;
    
    [SerializeField] SpawnPointSetting spawnPoint;
    [SerializeField] bool setPrarent = false;
    
    private Transform spawnTransform;

    public override void EnterTrigger(GameObject collisionObject)
    {
        SetSpawnTransform(collisionObject);

        if(spawnObject != null)
        {
            Instantiate(spawnObject, spawnTransform, setPrarent);
        }
    }

    void SetSpawnTransform(GameObject collisionObject)
    {
        if(spawnPoint == SpawnPointSetting.CollisionObjectSpawn)
        {
            spawnTransform = collisionObject.transform;
        }
        else if(spawnPoint == SpawnPointSetting.TriggerPointSpawn)
        {
            spawnTransform = transform;
        }
    }

    public override void ExitTrigger(GameObject collisionObject)
    {
        Destroy(gameObject);
    }
}
