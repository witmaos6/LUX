using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>�ǽɵ��� �����ϴ� �ҽ��� ���� (����� ǥ�ÿ� ����)</summary>
public enum SuspicionSourceType
{
    Sound,
    Light
}

public class DevilDispatcher : MonoBehaviour
{
    public static DevilDispatcher Instance;

    [Tooltip("씬에 배치된 모든 Devil. 비워두면 자동으로 탐색해서 채운다.")]
    public List<Devil> devils = new List<Devil>();

    [Header("Flashlight Detection")]
    public float flashlightDetectStrength = 10f;
    public float flashlightRangeWeitght = 1.5f;
    public LayerMask obstacleLayer;

    [Header("Suspicion Source Debug")]
    public bool showSuspicionRangeDebug = true;
    public float debugDisplayDuration = 0.5f;
    public Color soundRangeColor = new Color(1f, 0.6f, 0.1f); // ��Ȳ��: �Ҹ�
    public Color lightRangeColor = new Color(1f, 1f, 0.4f);   // ���� �����: ��
    private const int debugCircleSegments = 24;

    private Light2D flashlight;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (flashlight != null && flashlight.gameObject.activeSelf && devils.Count > 0)
        {
            DetectDevilsWithFlashlight();
        }
    }

    public void AddDevil(Devil inDevil)
    {
        if(!devils.Contains(inDevil))
        {
            devils.Add(inDevil);
        }
    }

    // ============================================================
    // ��/�Ҹ��� ���� ������Ʈ -> Dispatcher -> Devil
    // (gameObject, distance, strength)
    // ============================================================

    public void NotifySuspicionSource(Vector3 sourcePosition, float range, float strength, SuspicionSourceType type = SuspicionSourceType.Sound)
    {
        if (showSuspicionRangeDebug)
            ShowSuspicionRangeDebug(sourcePosition, range, type);

        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.AddSuspicion(sourcePosition, range, strength);
        }
    }

    /// <summary>
    /// �ǽɵ� �߰� ���ο� �����ϰ�, ��/�Ҹ��� �߻��� ������ ���� ������ ǥ���Ѵ�.
    /// ���� ���� �ִ� ���� �� ������, �Ҹ��� �߻��� ������ ȣ���ϸ� �ȴ�.
    /// </summary>
    public void ShowSuspicionRangeDebug(Vector3 position, float range, SuspicionSourceType type)
    {
        Color color = (type == SuspicionSourceType.Sound) ? soundRangeColor : lightRangeColor;
        DrawDebugCircle(position, range, color, debugDisplayDuration);
    }

    /// <summary>�־��� ��ġ�� �߽����� range �ݰ��� ���� ����׷� �׸��� (Scene �信�� Ȯ�� ����).</summary>
    private void DrawDebugCircle(Vector3 center, float radius, Color color, float duration)
    {
        if (radius <= 0f) return;

        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= debugCircleSegments; i++)
        {
            float angle = (i / (float)debugCircleSegments) * Mathf.PI * 2f;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Debug.DrawLine(prevPoint, nextPoint, color, duration);
            prevPoint = nextPoint;
        }
    }

    // ============================================================
    // PlayerController -> Dispatcher -> Devil (Door ������Ʈ)
    // ============================================================

    public void NotifyZoneChange(GameObject doorObject)
    {
        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.NotifyZoneChange(doorObject);
        }
    }

    // ============================================================
    // ���� �ý��� -> Dispatcher -> Devil
    // ============================================================

    public void NotifyPlayerHidden(bool hidden, Vector3 lastPositionBeforeHide)
    {
        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.SetPlayerHidden(hidden, lastPositionBeforeHide);
        }
    }

    // ============================================================
    // ������ ���� �߰� -> �ǽɵ� �ý������� ����
    // ============================================================

    public void TriggerLight(GameObject inFlashlight)
    {
        if (inFlashlight == null) return;
        flashlight = inFlashlight.GetComponent<Light2D>();
    }

    private void DetectDevilsWithFlashlight()
    {
        Vector3 flashlightPosition = flashlight.transform.position;
        float detectDistance = flashlight.pointLightOuterRadius * flashlightRangeWeitght;

        if (showSuspicionRangeDebug)
            ShowSuspicionRangeDebug(flashlightPosition, detectDistance, SuspicionSourceType.Light);

        Vector3 forwardDir = flashlight.transform.up;
        float halfSpotAngle = flashlight.pointLightOuterAngle / 2f;

        foreach (Devil devil in devils)
        {
            if (devil == null) continue;

            Vector3 devilPosition = devil.transform.position;
            float distToDevil = Vector3.Distance(flashlightPosition, devilPosition);

            if (distToDevil > detectDistance) continue;

            Vector3 dirToDevil = (devilPosition - flashlightPosition).normalized;

            float angleToDevil = Vector3.Angle(forwardDir, dirToDevil);
            if (angleToDevil > halfSpotAngle) continue;

            RaycastHit2D hitObstacle = Physics2D.Raycast(flashlightPosition, dirToDevil, distToDevil, obstacleLayer);
            if (hitObstacle.collider != null) continue;

            devil.AddSuspicion(flashlightPosition, detectDistance, flashlightDetectStrength);
        }

    }

    // ============================================================
    // ����� ǥ��
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (flashlight == null)
        {
            var foundLight = FindFirstObjectByType<Light2D>();
            if (foundLight != null) flashlight = foundLight;
            else return;
        }

        Gizmos.color = Color.yellow;
        float radius = flashlight.pointLightOuterRadius * 1.5f;
        float halfAngle = flashlight.pointLightOuterAngle / 2f;

        Vector3 forward = flashlight.transform.up;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward;

        Vector3 flashlightPosition = flashlight.transform.position;

        Gizmos.DrawLine(flashlightPosition, flashlightPosition + leftBoundary * radius);
        Gizmos.DrawLine(flashlightPosition, flashlightPosition + rightBoundary * radius);

        int segments = 16;
        Vector3 prevPoint = flashlightPosition + leftBoundary * radius;
        for (int i = 1; i <= segments; i++)
        {
            float currAngle = halfAngle - (flashlight.pointLightOuterAngle / segments) * i;
            Vector3 currDir = Quaternion.Euler(0, 0, currAngle) * forward;
            Vector3 currPoint = flashlightPosition + currDir * radius;

            Gizmos.DrawLine(prevPoint, currPoint);
            prevPoint = currPoint;
        }
    }
}
