using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Devil : MonoBehaviour, IDevilInterface
{
    public enum DevilState
    {
        Patrol,
        Investigate,
        Chase,
        Execute
    }

    [Header("AI State (Read Only)")]
    public DevilState currentState;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2f;
    public float investigateSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Patrol Settings (Waypoints)")]
    public Transform[] patrolWaypoints;

    [Header("Target Arrival Settings")]
    public float targetWaitTime = 1f;

    [Header("Investigate Wander Settings")]
    public int investigateMinPoints = 1;
    public int investigateMaxPoints = 3;
    public float investigateWanderDistance = 5f;

    [Header("Suspicion Settings")]
    public float maxSuspicion = 100f;
    public float chaseSuspicionThreshold = 50f;
    public float executeSuspicionThreshold = 100f;
    public float suspicionDecayDelay = 1f;
    public float suspicionDecayRate = 5f;

    [Header("Devil Info (Runtime, Debug)")]
    public float suspicion;
    public bool isCollidingWithPlayer;
    public bool isPlayerHidden;

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Door / Zone Transition")]
    public float doorArriveDistance = 0.5f;

    [Header("Jump Scare")]
    [SerializeField] GameObject jumpScareObject;

    public Action OnPlayerExecuted;

    private const float StopDistance = 0.5f;

    private readonly List<Vector3> investigatePoints = new List<Vector3>();

    private AnimationComponent animationComponent;
    private DevilSound devilSound;

    private DevilState previousState;
    private GameObject zoneChangeDoor;

    private Vector3 destination;
    private Vector3 investigateOrigin;
    private Vector3 lastDetectedPosition;
    private Vector3 lastHiddenPosition;

    private float currentSpeed;
    private float lastDetectionTime = -999f;
    private float waitTimer;

    private int currentWaypointIndex;
    private int investigatePointIndex = -1;

    private bool hasDestination;
    private bool hasExecuted;
    private bool isInDoorTransition;
    private bool isWaiting;

    private void Awake()
    {
        animationComponent = GetComponent<AnimationComponent>();
        devilSound = GetComponent<DevilSound>();
    }

    private void Start()
    {
        FindPlayerIfNeeded();

        DevilDispatcher.Instance.AddDevil(this);

        ChangeState(DevilState.Patrol);
    }

    private void Update()
    {
        if (hasExecuted)
            return;

        UpdateSuspicionDecay();
        UpdateState();
        RunStateMovement();

        if (hasDestination)
            MoveTowardsDestination();
    }

    public void AddSuspicion(Vector3 sourcePosition, float range, float strength)
    {
        if (hasExecuted || !gameObject.activeSelf || range <= 0f)
            return;

        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > range)
            return;

        float distanceWeight = 1f - Mathf.Clamp01(distance / range);

        if (suspicion <= 0f)
            devilSound.PlayDetectSound();

        suspicion = Mathf.Clamp(suspicion + strength * distanceWeight, 0f, maxSuspicion);

        lastDetectedPosition = sourcePosition;
        lastDetectionTime = Time.time;
    }

    public void SetPlayerHidden(bool hidden, Vector3 lastPositionBeforeHide)
    {
        isPlayerHidden = hidden;

        if (hidden)
            lastHiddenPosition = lastPositionBeforeHide;
    }

    public void NotifyZoneChange(GameObject doorObject)
    {
        zoneChangeDoor = doorObject;
    }

    public bool IsChase()
    {
        return currentState == DevilState.Chase || currentState == DevilState.Investigate;
    }

    public Vector3 InvestigatePoint()
    {
        return lastDetectedPosition;
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            playerTransform = playerObject.transform;
    }

    private void UpdateSuspicionDecay()
    {
        if (suspicion <= 0f)
            return;

        if (Time.time - lastDetectionTime < suspicionDecayDelay)
            return;

        suspicion = Mathf.Max(
            0f,
            suspicion - suspicionDecayRate * Time.deltaTime);
    }

    private void UpdateState()
    {
        DevilState nextState = EvaluateBehavior();

        if (nextState != currentState)
            ChangeState(nextState);
    }

    private DevilState EvaluateBehavior()
    {
        if (hasExecuted)
            return DevilState.Execute;

        if (suspicion <= 0f)
            return DevilState.Patrol;

        if (suspicion < chaseSuspicionThreshold)
        {
            investigateOrigin = lastDetectedPosition;
            return DevilState.Investigate;
        }

        if (isPlayerHidden)
            return EvaluateHiddenPlayerBehavior();

        return isCollidingWithPlayer ? DevilState.Execute : DevilState.Chase;
    }

    private DevilState EvaluateHiddenPlayerBehavior()
    {
        if (suspicion >= executeSuspicionThreshold)
        {
            return isCollidingWithPlayer ? DevilState.Execute : DevilState.Chase;
        }

        investigateOrigin = lastHiddenPosition;
        return DevilState.Investigate;
    }

    private void ChangeState(DevilState newState)
    {
        previousState = currentState;
        currentState = newState;

        isWaiting = false;
        waitTimer = 0f;

        switch (currentState)
        {
            case DevilState.Patrol:
                EnterPatrolState();
                break;

            case DevilState.Investigate:
                if(previousState == DevilState.Chase)
                {
                    OnInvestigateJumpScare();
                }
                
                EnterInvestigateState();
                break;

            case DevilState.Chase:
                EnterChaseState();
                break;

            case DevilState.Execute:
                EnterExecuteState();
                break;
        }
    }

    private void EnterPatrolState()
    {
        currentSpeed = patrolSpeed;
        zoneChangeDoor = null;

        investigatePoints.Clear();
        investigatePointIndex = -1;

        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            hasDestination = false;
            return;
        }

        destination = patrolWaypoints[currentWaypointIndex].position;
        hasDestination = true;
    }

    void OnInvestigateJumpScare()
    {
        if(playerTransform != null)
        {
            Transform canvasTransform = GameObject.Find("Canvas").transform;
            if(canvasTransform != null)
            {
                GameObject jumpScareInstance = Instantiate(jumpScareObject, canvasTransform);
                Destroy(jumpScareInstance, 3.0f);
            }
        }
    }

    private void EnterInvestigateState()
    {
        devilSound.PlayInvestigateSound();

        currentSpeed = investigateSpeed;
        zoneChangeDoor = null;

        destination = investigateOrigin;
        hasDestination = true;

        GenerateInvestigatePoints(investigateOrigin);
        investigatePointIndex = -1;
    }

    private void EnterChaseState()
    {
        currentSpeed = chaseSpeed;
        UpdateChaseDestination();
    }

    private void EnterExecuteState()
    {
        if (hasExecuted)
            return;

        hasExecuted = true;
        hasDestination = false;
        currentSpeed = 0f;

        OnPlayerExecuted?.Invoke();
    }

    private void RunStateMovement()
    {
        switch (currentState)
        {
            case DevilState.Patrol:
                UpdatePatrolMovement();
                break;

            case DevilState.Investigate:
                UpdateInvestigateMovement();
                break;

            case DevilState.Chase:
                UpdateChaseMovement();
                break;
        }
    }

    private void UpdatePatrolMovement()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            return;

        if (!hasDestination)
        {
            destination = patrolWaypoints[currentWaypointIndex].position;
            hasDestination = true;
            return;
        }

        if (!HasReachedDestination())
            return;

        if (HasFinishedWaiting())
            MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;

        destination = patrolWaypoints[currentWaypointIndex].position;
        hasDestination = true;
    }

    private void UpdateInvestigateMovement()
    {
        if (!hasDestination || !HasReachedDestination())
            return;

        if (HasFinishedWaiting())
            AdvanceInvestigatePoint();
    }

    private bool HasFinishedWaiting()
    {
        if (!isWaiting)
        {
            isWaiting = true;
            waitTimer = 0f;
            return false;
        }

        waitTimer += Time.deltaTime;

        if (waitTimer < targetWaitTime)
            return false;

        isWaiting = false;
        return true;
    }

    private void GenerateInvestigatePoints(Vector3 center)
    {
        investigatePoints.Clear();

        int pointCount = UnityEngine.Random.Range(investigateMinPoints, investigateMaxPoints + 1);

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * investigateWanderDistance;

            investigatePoints.Add(new Vector3(center.x + offset.x, center.y + offset.y, transform.position.z));
        }
    }

    private void AdvanceInvestigatePoint()
    {
        investigatePointIndex++;

        if (investigatePointIndex >= investigatePoints.Count)
        {
            GenerateInvestigatePoints(investigateOrigin);
            investigatePointIndex = 0;
        }

        destination = investigatePoints[investigatePointIndex];
        hasDestination = true;
    }

    private void UpdateChaseMovement()
    {
        if (zoneChangeDoor == null)
        {
            UpdateChaseDestination();
            return;
        }

        UpdateDoorChaseMovement();
    }

    private void UpdateDoorChaseMovement()
    {
        if (isInDoorTransition)
            return;

        destination = zoneChangeDoor.transform.position;
        hasDestination = true;

        if (Vector3.Distance(transform.position, destination) > doorArriveDistance)
        {
            return;
        }

        Door door = zoneChangeDoor.GetComponent<Door>();
        if (door != null)
        {
            StartCoroutine(DoorTransitionRoutine(zoneChangeDoor, door));
            return;
        }

        ClearDoorTarget();
        UpdateChaseDestination();
    }

    private void UpdateChaseDestination()
    {
        if (zoneChangeDoor != null)
        {
            destination = zoneChangeDoor.transform.position;
            hasDestination = true;
            return;
        }

        if (playerTransform != null)
        {
            destination = playerTransform.position;
            hasDestination = true;
        }
    }

    private IEnumerator DoorTransitionRoutine(
        GameObject doorObject,
        Door door)
    {
        isInDoorTransition = true;
        hasDestination = false;

        yield return new WaitForSeconds(door.fadeInDuration);

        isInDoorTransition = false;

        if (hasExecuted || zoneChangeDoor != doorObject)
            yield break;

        Vector3 targetPosition = door.moveTarget != null ? door.moveTarget.transform.position : door.movePosition;

        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

        animationComponent?.ResyncPosition();

        ClearDoorTarget();
        UpdateChaseDestination();
    }

    private void ClearDoorTarget()
    {
        zoneChangeDoor = null;
    }

    private void MoveTowardsDestination()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            currentSpeed * Time.deltaTime);
    }

    private bool HasReachedDestination()
    {
        return hasDestination
            && Vector3.Distance(transform.position, destination)
                <= StopDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isCollidingWithPlayer = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        isCollidingWithPlayer = true;

        PlayerController controller =
            other.GetComponent<PlayerController>();

        if (controller == null)
            return;

        PlayerController.PlayerState playerState =
            controller.GetState();

        bool isNormalPlayer = playerState == PlayerController.PlayerState.Normal;

        bool canExecuteHiddenPlayer = playerState == PlayerController.PlayerState.Hide && suspicion >= executeSuspicionThreshold;

        if (isNormalPlayer || canExecuteHiddenPlayer)
            controller.Dead();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isCollidingWithPlayer = false;
    }
}