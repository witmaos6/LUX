using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CameraVisibilityNotifier : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private bool findMainCameraAutomatically = true;
    [SerializeField] private bool notifyOnlyOnce;

    [Header("Game Events")]
    [SerializeField] private GameEvent enteredCameraEvent;
    [SerializeField] private GameEvent exitedCameraEvent;

    [Header("Inspector Events")]
    [SerializeField] private UnityEvent onEnteredCamera;
    [SerializeField] private UnityEvent onExitedCamera;

    private readonly Plane[] frustumPlanes = new Plane[6];
    private bool wasVisible;
    private bool hasNotified;
    private bool initialized;

    public bool IsVisible { get; private set; }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        ResolveCamera();
    }

    private void OnEnable()
    {
        initialized = false;
        wasVisible = false;
        IsVisible = false;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            ResolveCamera();

        bool isVisible = CheckVisibility();
        IsVisible = isVisible;

        if (!initialized)
        {
            initialized = true;
            wasVisible = isVisible;

            if (isVisible)
                NotifyEntered();

            return;
        }

        if (isVisible == wasVisible)
            return;

        wasVisible = isVisible;

        if (isVisible)
            NotifyEntered();
        else
            NotifyExited();
    }

    private void ResolveCamera()
    {
        if (targetCamera == null && findMainCameraAutomatically)
            targetCamera = Camera.main;
    }

    private bool CheckVisibility()
    {
        if (targetCamera == null ||
            targetRenderer == null ||
            !targetCamera.isActiveAndEnabled ||
            !targetRenderer.enabled ||
            !targetRenderer.gameObject.activeInHierarchy)
        {
            return false;
        }

        int objectLayerMask = 1 << targetRenderer.gameObject.layer;
        if ((targetCamera.cullingMask & objectLayerMask) == 0)
            return false;

        GeometryUtility.CalculateFrustumPlanes(targetCamera, frustumPlanes);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, targetRenderer.bounds);
    }

    private void NotifyEntered()
    {
        if (notifyOnlyOnce && hasNotified)
            return;

        hasNotified = true;

        if (enteredCameraEvent != null)
            GameEventManager.Raise(enteredCameraEvent);

        onEnteredCamera?.Invoke();
    }

    private void NotifyExited()
    {
        if (notifyOnlyOnce && hasNotified)
            return;

        if (exitedCameraEvent != null)
            GameEventManager.Raise(exitedCameraEvent);

        onExitedCamera?.Invoke();
    }
}
