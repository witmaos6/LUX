using UnityEngine;

public class MoveJumpScare : MonoBehaviour
{
    [Header("Timed")]
    [SerializeField] float timedDuration;
    private bool timedRunning = false;
    private float timedElapsed = 0f;

    [SerializeField] Vector3 targetPositionOffSet;
    Vector3 startPosition;
    Vector3 targetPosition;

    [SerializeField] AudioClip audioClip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
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
        startPosition = transform.position;
        targetPosition = new Vector3(transform.position.x + targetPositionOffSet.x, transform.position.y + targetPositionOffSet.y, transform.position.z + targetPositionOffSet.z);
        if (audioClip && audioSource)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
    void OnTimedTick(float progress, float deltaTime)
    {
        float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
        float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress);
        float z = Mathf.Lerp(startPosition.z, targetPosition.z, progress);

        transform.position = new Vector3(x, y, z);
    }
    void OnTimedEnd()
    {
        Destroy(gameObject);
    }
}
