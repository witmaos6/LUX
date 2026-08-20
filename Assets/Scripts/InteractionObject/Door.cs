using System;
using UnityEngine;

public class Door : InteractionObject
{
    [Header("Camera")]
    public Transform nextWallpaper;
    public GameEvent wallpaperRequestEvent;
    public GameObject moveTarget;
    public GameEvent moveTargetRequestEvent;
    public CameraFader cameraFader;
    private CameraController2D cameraController;
    public Vector3 movePosition;

    [Header("Fade Durations")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    [Header("Checkpoint")]
    [Tooltip("When enabled, moving through this door updates the saved respawn location and wallpaper.")]
    [SerializeField] private bool saveCheckpoint = true;

    private GameObject m_tryObject;

    private void Awake()
    {
        interactionType = InteractionType.ArrowKey;

        if (cameraFader == null)
        {
            cameraFader = GameObject.Find("Main Camera").GetComponent<CameraFader>();
            cameraController = GameObject.Find("Main Camera").GetComponent<CameraController2D>();
        }
    }

    private void Start()
    {
        if (nextWallpaper == null && wallpaperRequestEvent != null)
            GameEventManager.Raise<Action<Transform>>(wallpaperRequestEvent, t => nextWallpaper = t);

        if (moveTargetRequestEvent != null)
            GameEventManager.Raise<Action<Transform>>(moveTargetRequestEvent, t => moveTarget = t.gameObject);
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        m_tryObject = tryObject;
        if(cameraFader != null)
        {
            cameraFader.StartFade(fadeInDuration, fadeOutDuration);
            cameraFader.cameraFadeInComplete += MoveToPosition;
            cameraFader.cameraFadeOutComplete += EndInteraction;
        }

        PlayerController playerController = m_tryObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.BlockInteraction();
        }
    }

    void MoveToPosition()
    {
        if(m_tryObject != null)
        {
            if(moveTarget != null)
            {
                m_tryObject.transform.position = moveTarget.transform.position;
            }
            else
            {
                m_tryObject.transform.position = movePosition;
            }

            AnimationComponent tryObjectAnimation = m_tryObject.GetComponent<AnimationComponent>();
            if (tryObjectAnimation != null)
            {
                tryObjectAnimation.ResyncPosition();
            }

            if(cameraController != null && nextWallpaper != null)
            {
                cameraController.SetWallpaper(nextWallpaper);
            }

            if (saveCheckpoint)
            {
                SaveManager.SetRespawnPoint(
                    m_tryObject.transform.position,
                    nextWallpaper != null ? nextWallpaper.name : null);
            }

            if (DevilDispatcher.Instance != null)
            {
                DevilDispatcher.Instance.NotifyZoneChange(gameObject);
            }
        }
    }

    public override void CancelInteraction()
    {
        if (cameraFader != null)
        {
            cameraFader.cameraFadeInComplete -= MoveToPosition;
            cameraFader.cameraFadeOutComplete -= EndInteraction;
        }

        base.CancelInteraction();
    }
}
