using System.Collections.Generic;
using UnityEngine;

public class RandomPatrolDevil : MonoBehaviour, ISuspicionReceiver, IDevilInterface
{
    private enum State
    {
        Patrol,
        Chase
    }
    private State currentState = State.Patrol;

    [Header("Targets")]
    [SerializeField] private Transform playerTransform;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private float arriveDistance = 0.2f;

    private DevilSound devilSound;

    private Rigidbody2D rb;

    private Vector3 patrolPoint = Vector3.zero;

    private ServerModeManager modeManager;

    private void OnEnable()
    {
        DevilDispatcher.Instance?.AddSuspicionReceiver(this);
    }

    private void OnDisable()
    {
        DevilDispatcher.Instance?.RemoveSuspicionReceiver(this);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        SetState(State.Patrol);
    }

    public void SetModeManager(ServerModeManager inModeManager)
    {
        modeManager = inModeManager;
    }

    public void SetPatrolPoint(Vector3 inPatrolPoint)
    {
        patrolPoint = inPatrolPoint;
    }

    public void AddSuspicion(Vector3 sourcePosition, float range, float strength)
    {
        if (range <= 0f)
            return;

        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > range)
            return;

        SetState(State.Chase);
    }

    void Update()
    {
        if (currentState == State.Patrol && FlashlightVisibilityService.HasActiveFlashlight)
        {
            SetState(State.Chase);
        }
    }

    private void FixedUpdate()
    {
        if(currentState == State.Patrol && patrolPoint != Vector3.zero)
        {
            Patrol();
        }
        else if(currentState == State.Chase && playerTransform != null)
        {
            Chase();
        }
    }

    void Patrol()
    {
        if(HasArrived(patrolPoint))
        {
            patrolPoint = modeManager.PatrolPoint();
        }
        else
        {
            MoveTo(patrolPoint, patrolSpeed);
        }
    }

    void Chase()
    {
        if(HasArrived(playerTransform.position))
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            MoveTo(playerTransform.position, chaseSpeed);
        }
    }

    private bool HasArrived(Vector3 destination)
    {
        return Vector2.Distance(rb.position, destination) <= arriveDistance;
    }

    private void MoveTo(Vector3 destination, float speed)
    {
        Vector2 current = rb.position;
        Vector2 target = destination;
        Vector2 next = Vector2.MoveTowards(current, target, speed * Time.fixedDeltaTime);

        rb.MovePosition(next);
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

    public bool IsChase()
    {
        return currentState == State.Chase;
    }

    public Vector3 InvestigatePoint()
    {
        return playerTransform.position;
    }

    void SetState(State inState)
    {
        currentState = inState;
        if(currentState == State.Chase)
        {
            devilSound.PlayDetectSound();
        }
    }
}
