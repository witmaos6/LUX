using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class LightPatrolDevilAI : MonoBehaviour, ISuspicionReceiver
{
    private enum State
    {
        PatrolLights,
        Investigate,
        Chase
    }

    [Header("Targets")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Transform> lightWaypoints;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private float arriveDistance = 0.2f;
    [SerializeField] private float waitAtLightTime = 1f;

    [Header("Suspicion")]
    [SerializeField] private float maxSuspicion = 100f;
    [SerializeField] private float chaseSuspicionThreshold = 50f;
    [SerializeField] private float suspicionDecayDelay = 1f;
    [SerializeField] private float suspicionDecayRate = 5f;

    [Header("Sound")]
    public AudioClip loopSound;

    private AudioSource audioSource;

    [Header("Runtime Debug")]
    [SerializeField] private State currentState;
    [SerializeField] private float suspicion;

    private Rigidbody2D rb;
    private int currentLightIndex;
    private float waitTimer;
    private float lastSuspicionTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        DevilDispatcher.Instance?.AddSuspicionReceiver(this);
    }

    public void SetLightWaypoints(List<Transform> lights)
    {
        lightWaypoints = lights;
    }

    public void AddSuspicion(Vector3 sourcePosition, float range, float strength)
    {
        if (range <= 0f)
            return;

        range *= 3f;
        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > range)
            return;

        float weight = 1f - Mathf.Clamp01(distance / range);
        suspicion = Mathf.Clamp(suspicion + strength * weight, 0f, maxSuspicion);

        if(currentState == State.Investigate && suspicion >= chaseSuspicionThreshold)
        {
            currentState = State.Chase;
        }

        lastSuspicionTime = Time.time;
    }

    private void OnDisable()
    {
        DevilDispatcher.Instance?.RemoveSuspicionReceiver(this);
    }
    void Start()
    {
        if(playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if(player != null)
            {
                playerTransform = player.transform;
            }
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = loopSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        audioSource.Play();

        currentState = State.Investigate;
    }

    void Update()
    {
        UpdateSuspicionDecay();
    }

    private void UpdateSuspicionDecay()
    {
        if (suspicion <= 0f)
            return;

        if (Time.time - lastSuspicionTime < suspicionDecayDelay)
            return;

        suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
    }

    public void SetPatrolLights()
    {
        if(currentState != State.Chase)
            currentState = State.PatrolLights;
    }

    public void SetInvestigate()
    {
        if(currentState != State.Chase)
            currentState = State.Investigate;
        
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.PatrolLights:
                PatrolLights();
                break;

            case State.Investigate:
                Stop();
                break;

            case State.Chase:
                ChasePlayer();
                break;
        }
    }

    private void PatrolLights()
    {
        if (lightWaypoints == null || lightWaypoints.Count == 0)
            return;

        Transform targetLight = lightWaypoints[currentLightIndex];
        if(targetLight == null)
        {
            MoveToNextLight();
            return;
        }

        if(HasArrived(targetLight.position))
        {
            waitTimer += Time.fixedDeltaTime;

            if(waitTimer >= waitAtLightTime)
            {
                waitTimer = 0f;
                MoveToNextLight();
            }

            Stop();
            return;
        }

        MoveTo(targetLight.position, patrolSpeed);
    }

    private void ChasePlayer()
    {
        if (playerTransform == null)
        {
            Stop();
            return;
        }
        MoveTo(playerTransform.position, chaseSpeed);
    }

    private void MoveToNextLight()
    {
        if (lightWaypoints == null || lightWaypoints.Count == 0)
            return;

        currentLightIndex = (currentLightIndex + 1) % lightWaypoints.Count;
    }

    private void MoveTo(Vector3 destination, float speed)
    {
        Vector2 current = rb.position;
        Vector2 target = destination;
        Vector2 next = Vector2.MoveTowards(current, target, speed * Time.fixedDeltaTime);

        rb.MovePosition(next);
    }

    private bool HasArrived(Vector3 destination)
    {
        return Vector2.Distance(rb.position, destination) <= arriveDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.Dead();
            }
        }        
    }

    private void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
