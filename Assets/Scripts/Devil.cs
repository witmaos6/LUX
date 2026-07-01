using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Devil : MonoBehaviour
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
    private int currentWaypointIndex = 0;

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
    public float distanceToPlayer;
    public bool isCollidingWithPlayer;
    public bool isPlayerHidden;

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Door / Zone Transition")]
    public float doorArriveDistance = 0.5f;

    [Header("Sound")]
    public AudioClip loopSound;

    private AudioSource audioSource;

    public System.Action OnPlayerExecuted;

    private Vector3 lastDetectedPosition;
    private Vector3 lastHiddenPosition;
    private GameObject zoneChangeDoor;
    private Vector3 investigateOrigin;

    private float lastDetectionTime = -999f;
    private bool hasExecuted = false;
    private bool isInDoorTransition = false;

    private Vector3 destination;
    private bool hasDestination = false;
    private float currentSpeed = 0f;
    private const float stopDistance = 0.5f;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    private List<Vector3> investigatePoints = new List<Vector3>();
    private int investigatePointIndex = -1;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        DevilDispatcher.Instance.AddDevil(this);

        ChangeState(DevilState.Patrol);

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = loopSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }

    private void Update()
    {
        if (hasExecuted) return;

        if (playerTransform != null)
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        UpdateSuspicionDecay();

        DevilState newState = EvaluateBehavior();
        if (newState != currentState)
            ChangeState(newState);

        RunStateMovement();

        if (hasDestination)
            MoveTowardsDestination();
    }

    public void AddSuspicion(Vector3 sourcePosition, float range, float strength)
    {
        if (hasExecuted) return;
        if (range <= 0f) return;

        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > range) return;

        float weight = 1f - Mathf.Clamp01(distance / range);
        suspicion = Mathf.Clamp(suspicion + strength * weight, 0f, maxSuspicion);

        lastDetectedPosition = sourcePosition;
        lastDetectionTime = Time.time;
    }

    private void UpdateSuspicionDecay()
    {
        if (suspicion <= 0f) return;

        if (Time.time - lastDetectionTime >= suspicionDecayDelay)
        {
            suspicion = Mathf.Max(0f, suspicion - suspicionDecayRate * Time.deltaTime);
        }
    }

    public void SetPlayerHidden(bool hidden, Vector3 lastPositionBeforeHide)
    {
        isPlayerHidden = hidden;
        if (hidden)
        {
            lastHiddenPosition = lastPositionBeforeHide;
        }
    }

    public void NotifyZoneChange(GameObject doorObject)
    {
        zoneChangeDoor = doorObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();
            PlayerController.PlayerState playerState = controller.GetState();
            if (playerState == PlayerController.PlayerState.Normal || (playerState == PlayerController.PlayerState.Hide && suspicion >= maxSuspicion))
            {
                Debug.Log("Player Die");

                isCollidingWithPlayer = true;

                controller.Dead();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isCollidingWithPlayer = false;
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

        if (isCollidingWithPlayer)
        {
            if (isPlayerHidden)
            {
                if (suspicion >= executeSuspicionThreshold)
                    return DevilState.Execute;

                investigateOrigin = lastHiddenPosition;
                return DevilState.Investigate;
            }
            else
            {
                return DevilState.Execute;
            }
        }
        else
        {
            return DevilState.Chase;
        }
    } 

    private void ChangeState(DevilState newState)
    {
        currentState = newState;
        isWaiting = false;
        waitTimer = 0f;

        switch (newState)
        {
            case DevilState.Patrol:
                currentSpeed = patrolSpeed;
                zoneChangeDoor = null;

                investigatePoints.Clear();
                investigatePointIndex = -1;

                if (patrolWaypoints != null && patrolWaypoints.Length > 0)
                {
                    destination = patrolWaypoints[currentWaypointIndex].position;
                    hasDestination = true;
                }
                else
                {
                    hasDestination = false;
                }
                break;

            case DevilState.Investigate:
                currentSpeed = investigateSpeed;
                zoneChangeDoor = null;

                destination = investigateOrigin;
                hasDestination = true;

                GenerateInvestigatePoints(investigateOrigin);
                investigatePointIndex = -1;
                break;

            case DevilState.Chase:
                currentSpeed = chaseSpeed;
                UpdateChaseDestination();
                break;

            case DevilState.Execute:
                if (!hasExecuted)
                {
                    hasExecuted = true;
                    hasDestination = false;
                    currentSpeed = 0f;
                    OnPlayerExecuted?.Invoke();
                }
                break;
        }
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
            case DevilState.Execute:
                // �̵� ����
                break;
        }
    }

    // ------------------ Patrol ------------------

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

        if (HasReachedDestination())
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
                return;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer >= targetWaitTime)
            {
                isWaiting = false;
                MoveToNextWaypoint();
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
        destination = patrolWaypoints[currentWaypointIndex].position;
        hasDestination = true;
    }

    private void GenerateInvestigatePoints(Vector3 center)
    {
        investigatePoints.Clear();

        int count = Random.Range(investigateMinPoints, investigateMaxPoints + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * investigateWanderDistance;
            investigatePoints.Add(new Vector3(center.x + offset.x, center.y + offset.y, transform.position.z));
        }
    }

    private void UpdateInvestigateMovement()
    {
        if (!hasDestination) return;

        if (HasReachedDestination())
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
                return;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer >= targetWaitTime)
            {
                isWaiting = false;
                AdvanceInvestigatePoint();
            }
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

    // ------------------ Chase ------------------

    private void UpdateChaseDestination()
    {
        if (zoneChangeDoor != null)
        {
            destination = zoneChangeDoor.transform.position;
            hasDestination = true;
        }
        else if (playerTransform != null)
        {
            destination = playerTransform.position;
            hasDestination = true;
        }
    }

    private void UpdateChaseMovement()
    {
        if (zoneChangeDoor != null)
        {
            if (isInDoorTransition)
                return;

            destination = zoneChangeDoor.transform.position;
            hasDestination = true;

            if (Vector3.Distance(transform.position, destination) <= doorArriveDistance)
            {
                Door door = zoneChangeDoor.GetComponent<Door>();
                if (door != null)
                {
                    StartCoroutine(DoorTransitionRoutine(zoneChangeDoor, door));
                }
                else
                {
                    HandleDoorPassThrough(zoneChangeDoor, Vector3.zero, false);
                    UpdateChaseDestination();
                }
            }
        }
        else if (playerTransform != null)
        {
            destination = playerTransform.position;
            hasDestination = true;
        }
    }

    private IEnumerator DoorTransitionRoutine(GameObject doorObject, Door door)
    {
        isInDoorTransition = true;
        hasDestination = false;

        yield return new WaitForSeconds(door.fadeInDuration);

        isInDoorTransition = false;

        // ��� �� ó���Ǿ��ų�, �ǽɵ��� ������ �� �̻� �� Door�� ���� �ʿ䰡 ������ ��� �ߴ�
        if (hasExecuted || zoneChangeDoor != doorObject)
            yield break;

        Vector3 targetPosition = door.moveTarget != null ? door.moveTarget.transform.position : door.movePosition;
        HandleDoorPassThrough(doorObject, targetPosition, true);
        UpdateChaseDestination();
    }

    private void HandleDoorPassThrough(GameObject doorObject, Vector3 movePosition, bool teleport)
    {
        if (teleport)
        {
            transform.position = new Vector3(movePosition.x, movePosition.y, transform.position.z);
        }

        zoneChangeDoor = null;
    }

    private void MoveTowardsDestination()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, currentSpeed * Time.deltaTime);
    }

    private bool HasReachedDestination()
    {
        if (!hasDestination) return false;
        return Vector3.Distance(transform.position, destination) <= stopDistance;
    }
}