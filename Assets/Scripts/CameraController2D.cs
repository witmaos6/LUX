using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Deadzone")]
    [SerializeField] private Vector2 deadzoneSize = new Vector2(3f, 2f);

    [Header("Lookahead")]
    [SerializeField] private float lookaheadDistance = 2f;
    [SerializeField] private float lookaheadTime = 0.4f;

    [Header("Map Bounds")]
    [SerializeField] private Transform wallpaper;
    [SerializeField] private Vector2 boundsPadding = Vector2.zero;

    [Header("Pixel Snap")]
    [SerializeField] private bool usePixelSnap = true;
    [SerializeField] private float pixelsPerUnit = 256f;

    private Vector2 _mapMin;
    private Vector2 _mapMax;

    private Camera _cam;
    private float _camZ;
    private Vector2 _velocity;
    private float _lookaheadOffset;
    private float _lookaheadVel;

    private PlayerController _playerController;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _camZ = transform.position.z;

        if (target != null)
            _playerController = target.GetComponent<PlayerController>();

        UpdateBoundsFromWallpaper();
    }

    private void UpdateBoundsFromWallpaper()
    {
        if (wallpaper == null)
        {
            Debug.LogWarning("[CameraController2D] wallpaper is null. Bounds not updated.");
            return;
        }

        Vector2 pos = wallpaper.position;
        Vector2 scale = wallpaper.lossyScale;
        _mapMin = pos - scale * 0.5f - boundsPadding;
        _mapMax = pos + scale * 0.5f + boundsPadding;
    }

    public void SetWallpaper(Transform newWallpaper)
    {
        wallpaper = newWallpaper;
        UpdateBoundsFromWallpaper();
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        _mapMin = min;
        _mapMax = max;
    }

    // FixedUpdate: 플레이어가 Rigidbody2D로 움직이므로 물리 타이밍에 맞춤
    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 cam = transform.position;
        Vector2 tgt = target.position;

        // 1. Deadzone
        float hw = deadzoneSize.x * 0.5f;
        float hh = deadzoneSize.y * 0.5f;

        Vector2 desired = cam;

        float dx = tgt.x - cam.x;
        float dy = tgt.y - cam.y;

        if (dx > hw) desired.x = tgt.x - hw;
        if (dx < -hw) desired.x = tgt.x + hw;
        if (dy > hh) desired.y = tgt.y - hh;
        if (dy < -hh) desired.y = tgt.y + hh;

        // 2. Lookahead — velocity 대신 dirRight 기준
        if (_playerController != null)
        {
            float facingSign = _playerController.DirRight ? 1f : -1f;
            float lookaheadTarget = facingSign * lookaheadDistance;

            _lookaheadOffset = Mathf.SmoothDamp(
                _lookaheadOffset, lookaheadTarget, ref _lookaheadVel, lookaheadTime);
        }

        desired.x += _lookaheadOffset;

        // 3. Bound Clamping
        float halfCamH = _cam.orthographicSize;
        float halfCamW = halfCamH * _cam.aspect;

        float minX = _mapMin.x + halfCamW;
        float maxX = _mapMax.x - halfCamW;
        float minY = _mapMin.y + halfCamH;
        float maxY = _mapMax.y - halfCamH;

        desired.x = minX < maxX ? Mathf.Clamp(desired.x, minX, maxX) : (_mapMin.x + _mapMax.x) * 0.5f;
        desired.y = minY < maxY ? Mathf.Clamp(desired.y, minY, maxY) : (_mapMin.y + _mapMax.y) * 0.5f;

        // 4. SmoothDamp
        Vector2 smoothed = Vector2.SmoothDamp(cam, desired, ref _velocity, smoothTime);

        if (usePixelSnap)
        {
            smoothed.x = Mathf.Round(smoothed.x * pixelsPerUnit) / pixelsPerUnit;
            smoothed.y = Mathf.Round(smoothed.y * pixelsPerUnit) / pixelsPerUnit;
        }

        transform.position = new Vector3(smoothed.x, smoothed.y, _camZ);
    }

    private void OnDrawGizmos()
    {
        // Deadzone — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneSize.x, deadzoneSize.y, 0f));

        // Map bounds — red
        if (wallpaper != null)
        {
            Vector2 pos = wallpaper.position;
            Vector2 scale = wallpaper.lossyScale;
            Vector2 min = pos - scale * 0.5f - boundsPadding;
            Vector2 max = pos + scale * 0.5f + boundsPadding;

            Gizmos.color = Color.red;
            Vector3 c = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
            Vector3 s = new Vector3(max.x - min.x, max.y - min.y, 0f);
            Gizmos.DrawWireCube(c, s);
        }
    }
}