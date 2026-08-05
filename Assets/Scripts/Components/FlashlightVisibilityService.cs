using UnityEngine;
using UnityEngine.Rendering.Universal;

public readonly struct FlashlightDetectionResult
{
    public FlashlightDetectionResult(bool isIlluminated, Vector3 lightPosition, float distance, float detectionRange)
    {
        IsIlluminated = isIlluminated;
        LightPosition = lightPosition;
        Distance = distance;
        DetectionRange = detectionRange;
    }

    public bool IsIlluminated { get; }
    public Vector3 LightPosition { get; }
    public float Distance { get; }
    public float DetectionRange { get; }
}

public static class FlashlightVisibilityService
{
    private static Light2D flashlight;

    public static bool HasActiveFlashlight =>
        flashlight != null &&
        flashlight.isActiveAndEnabled &&
        flashlight.gameObject.activeInHierarchy &&
        flashlight.intensity > 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        flashlight = null;
    }

    public static void Register(GameObject flashlightObject)
    {
        flashlight = flashlightObject != null ? flashlightObject.GetComponent<Light2D>() : null;

        if (flashlightObject != null && flashlight == null)
            Debug.LogWarning($"{flashlightObject.name}: The registered flashlight has no Light2D.", flashlightObject);
    }

    public static FlashlightDetectionResult Evaluate(Vector3 targetPosition, float rangeMultiplier = 1f)
    {
        if (!HasActiveFlashlight || rangeMultiplier <= 0f)
            return default;

        Vector3 lightPosition = flashlight.transform.position;
        Vector2 offset = targetPosition - lightPosition;
        float distance = offset.magnitude;
        float detectionRange = flashlight.pointLightOuterRadius * rangeMultiplier;

        if (distance > detectionRange)
            return new FlashlightDetectionResult(false, lightPosition, distance, detectionRange);

        bool isInsideAngle = true;
        if (distance > Mathf.Epsilon && flashlight.pointLightOuterAngle < 360f)
        {
            Vector2 forward = flashlight.transform.up;
            float angle = Vector2.Angle(forward, offset);
            isInsideAngle = angle <= flashlight.pointLightOuterAngle * 0.5f;
        }

        return new FlashlightDetectionResult(isInsideAngle, lightPosition, distance, detectionRange);
    }
}
