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
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] SpawnPointSetting spawnPoint;
    [SerializeField] bool setPrarent = false;

    private GameObject spawnInstance;
    private SpriteRenderer sr;
    private Transform spawnTransform;

    public override void EnterTrigger(GameObject collisionObject)
    {
        SetSpawnTransform(collisionObject);

        if(spawnObject != null)
        {
            spawnInstance = Instantiate(spawnObject, spawnTransform, setPrarent);
            sr = spawnInstance.GetComponent<SpriteRenderer>();

            StartCoroutine(ShowTime());
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

    IEnumerator ShowTime()
    {
        yield return new WaitForSeconds(showDuration);

        StartCoroutine(FadeSpriteRenderer(sr, 1f, 0f, fadeOutDuration));
    }

    private IEnumerator FadeSpriteRenderer(SpriteRenderer sr, float from, float to, float duration)
    {
        Color color = sr.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(from, to, t);
            sr.color = color;
            yield return null;
        }

        color.a = to;
        sr.color = color;
    }

    public override void ExitTrigger(GameObject collisionObject)
    {
        if(spawnInstance != null)
            Destroy(spawnInstance);

        Destroy(gameObject);
    }
}
