using UnityEngine;

public class TriggerCameraJumpScare : MonoBehaviour // To do: Components 파일로 이동 및 CameraJumpScare로 이름 변경
{
    [SerializeField] float targetScaleOffset = 5f;
    [SerializeField] float restorationScaleOffset = 3f;
    [SerializeField] float jumpScareProgress = 0.7f;

    float originScale = 1f;
    float targetScale;
    float restorationScale;

    [Header("Timed")]
    [SerializeField] float timedDuration = 1f;
    private bool timedRunning = false;
    private float timedElapsed = 0f;

    private void Start()
    {
        BeginTimed();
    }

    void BeginTimed()
    {
        if (timedRunning)
            return;

        timedRunning = true;
        timedElapsed = 0f;

        OnTimedBegin();
    }

    void Update()
    {
        if (timedRunning)
        {
            TickTimed(Time.deltaTime);
        }
    }

    void TickTimed(float deltaTime)
    {
        if (!timedRunning)
            return;

        timedElapsed += deltaTime;
        
        float progress = timedDuration > 0f ? Mathf.Clamp01(timedElapsed / timedDuration) : 1f;
        OnTimedTick(progress, deltaTime);

        if (timedElapsed >= timedDuration)
        {
            EndTimed();
        }
    }

    void EndTimed()
    {
        timedRunning = false;
        OnTimedEnd();
    }

    void OnTimedBegin()
    {
        originScale = transform.position.x;
        targetScale = originScale * targetScaleOffset;
        restorationScale = originScale * restorationScaleOffset;
    }
    void OnTimedTick(float progress, float deltaTime)
    {
        if(progress <= jumpScareProgress)
        {
            float currentScale = Mathf.Lerp(originScale, targetScale, progress);

            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
        else
        {
            float currentScale = Mathf.Lerp(targetScale, restorationScale, progress);

            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
    }
    void OnTimedEnd()
    {
        Destroy(gameObject);
    }
}
