using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    private readonly List<ISuspicionReceiver> suspicionReceivers = new List<ISuspicionReceiver>();

    [Header("Flashlight Detection")]
    public float flashlightDetectStrength = 10f;
    [FormerlySerializedAs("flashlightRangeWeitght")]
    public float flashlightRangeWeight = 1.5f;

    [Header("Suspicion Source Debug")]
    public bool showSuspicionRangeDebug = true;
    public float debugDisplayDuration = 0.5f;
    public Color soundRangeColor = new Color(1f, 0.6f, 0.1f);
    public Color lightRangeColor = new Color(1f, 1f, 0.4f);
    private const int debugCircleSegments = 24;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (FlashlightVisibilityService.HasActiveFlashlight && devils.Count > 0)
        {
            DetectDevilsWithFlashlight();
        }
    }

    public void AddDevil(Devil inDevil)
    {
        if (inDevil == null)
            return;

        if(!devils.Contains(inDevil))
        {
            devils.Add(inDevil);
        }
    }

    public void AddSuspicionReceiver(ISuspicionReceiver receiver)
    {
        if (receiver == null)
            return;

        if (!suspicionReceivers.Contains(receiver))
        {
            suspicionReceivers.Add(receiver);
        }
    }

    public void RemoveSuspicionReceiver(ISuspicionReceiver receiver)
    {
        if (receiver == null)
            return;

        suspicionReceivers.Remove(receiver);
    }

    public void NotifySuspicionSource(Vector3 sourcePosition, float range, float strength, SuspicionSourceType type = SuspicionSourceType.Sound)
    {
        if (showSuspicionRangeDebug)
            ShowSuspicionRangeDebug(sourcePosition, range, type);

        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.AddSuspicion(sourcePosition, range, strength);
        }

        for (int i = suspicionReceivers.Count - 1; i >= 0; i--)
        {
            ISuspicionReceiver receiver = suspicionReceivers[i];
            if (receiver == null)
            {
                suspicionReceivers.RemoveAt(i);
                continue;
            }

            receiver.AddSuspicion(sourcePosition, range, strength);
        }
    }


    public void ShowSuspicionRangeDebug(Vector3 position, float range, SuspicionSourceType type)
    {
        Color color = (type == SuspicionSourceType.Sound) ? soundRangeColor : lightRangeColor;
        DrawDebugCircle(position, range, color, debugDisplayDuration);
    }

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

    public void NotifyZoneChange(GameObject doorObject)
    {
        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.NotifyZoneChange(doorObject);
        }
    }

    public void NotifyPlayerHidden(bool hidden, Vector3 lastPositionBeforeHide)
    {
        foreach (Devil devil in devils)
        {
            if (devil != null)
                devil.SetPlayerHidden(hidden, lastPositionBeforeHide);
        }
    }

    private void DetectDevilsWithFlashlight()
    {
        FlashlightDetectionResult lightInfo =
            FlashlightVisibilityService.Evaluate(Vector3.zero, flashlightRangeWeight);

        if (showSuspicionRangeDebug)
            ShowSuspicionRangeDebug(
                lightInfo.LightPosition,
                lightInfo.DetectionRange,
                SuspicionSourceType.Light);

        foreach (Devil devil in devils)
        {
            if (devil == null) continue;

            FlashlightDetectionResult result =
                FlashlightVisibilityService.Evaluate(
                    devil.transform.position,
                    flashlightRangeWeight);

            if (result.IsIlluminated)
            {
                devil.AddSuspicion(
                    result.LightPosition,
                    result.DetectionRange,
                    flashlightDetectStrength);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!FlashlightVisibilityService.HasActiveFlashlight)
            return;

        FlashlightDetectionResult lightInfo =
            FlashlightVisibilityService.Evaluate(Vector3.zero, flashlightRangeWeight);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lightInfo.LightPosition, lightInfo.DetectionRange);
    }
}
