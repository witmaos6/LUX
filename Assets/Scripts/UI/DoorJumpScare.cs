using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorJumpScare : MonoBehaviour
{
    [SerializeField] float startAngle = 0f;
    [SerializeField] float targetAngle = -70.0f;
    [SerializeField] float startDelay = 0.3f;
    [SerializeField] private AudioClip audioClip;

    private AudioSource audioSource;
    private Image childImage;

    [Header("Timed")] // timedDuration 시간 동안 Update함수에 맞춰서 실행되는 헤더
    [SerializeField] float timedDuration = 0.5f;
    private bool timedRunning = false;
    private float timedElapsed = 0f;

    private void Awake()
    {
        childImage = GetComponentInChildren<Image>();
        if (childImage != null)
        {
            childImage.enabled = false;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        audioSource.loop = false;
    }
    void Start()
    {
        StartCoroutine(JumpScare());
    }

    IEnumerator JumpScare()
    {
        yield return new WaitForSeconds(startDelay);

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

    void OnTimedBegin()
    {
        childImage.enabled = true;
        startAngle = transform.eulerAngles.z;
    }

    void TickTimed(float deltaTime)
    {
        if (!timedRunning)
            return;

        timedElapsed += deltaTime;

        float progress = timedDuration > 0f ? Mathf.Clamp01(timedElapsed / timedDuration) : 1f;
        OnTimedTick(progress, deltaTime);

        if(timedElapsed >= timedDuration)
        {
            OnTimedEnd();
        }
    }

    void OnTimedTick(float progress, float deltaTime)
    {
        float z = Mathf.LerpAngle(startAngle, targetAngle, progress);
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    void OnTimedEnd()
    {
        timedRunning = false;
        if (audioSource && audioClip)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if(timedRunning)
        {
            TickTimed(Time.deltaTime);
        }
    }
}
