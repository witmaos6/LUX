using UnityEngine;

public class FlashlightDetected : MonoBehaviour
{
    [Header("Timed")]
    [SerializeField] float timedDuration;
    private bool timedRunning = false;
    private float timedElapsed = 0f;

    [SerializeField] GameObject moveObject;
    [SerializeField] float moveXOffset = 1f;
    [Min(0f)]
    [SerializeField] private float requiredIlluminationTime = 0.4f;

    float illuminationTime = 0f;

    float originX;
    float targetX;
    bool isActivate = false;

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
        if(!isActivate)
        {
            FlashlightDetectionResult lightResult = FlashlightVisibilityService.Evaluate(transform.position, 0.8f);
            if (lightResult.IsIlluminated)
            {
                illuminationTime += Time.deltaTime;
                if(illuminationTime > requiredIlluminationTime)
                {
                    BeginTimed();
                }
            }
        }
        
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
        originX = moveObject.transform.position.x;
        targetX = originX + moveXOffset;
    }
    void OnTimedTick(float progress, float deltaTime)
    {
        float currentX = Mathf.Lerp(originX, targetX, progress);
        
        if(moveObject)
        {
            moveObject.transform.position = new Vector3(currentX, moveObject.transform.position.y, moveObject.transform.position.z);
        }
    }
    void OnTimedEnd()
    {
        if(moveObject)
        {
            Destroy(moveObject);
        }
        Destroy(gameObject);
    }
}
