using UnityEngine;

public class CameraJumpScare : MonoBehaviour
{
    [SerializeField] float targetScaleOffset = 5f;
    [SerializeField] float restorationScaleOffset = 3f;
    [SerializeField] float jumpScareProgress = 0.7f;
    [SerializeField] float targetPositionOffset = 10f;

    float originScale = 1f;
    float targetScale;
    float restorationScale;

    float originPosition = 0;
    float targetPosition = 0f;

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
        originScale = transform.localScale.x;
        targetScale = originScale * targetScaleOffset;
        restorationScale = originScale * restorationScaleOffset;

        originPosition = transform.position.x;
        targetPosition = transform.position.x + targetPositionOffset;
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

        float currentX = Mathf.Lerp(originPosition, targetPosition, progress);
        transform.position = new Vector3(currentX, transform.position.y);
    }
    void OnTimedEnd()
    {
        Destroy(gameObject);
    }
}
