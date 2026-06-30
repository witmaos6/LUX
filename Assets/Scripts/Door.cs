using System;
using UnityEngine;

public class Door : InteractionObject
{
    [Header("Camera")]
    public Transform nextWallpaper;
    public GameEvent wallpaperRequestEvent;
    public GameObject moveTarget;
    public CameraFader cameraFader;
    private CameraController2D cameraController;
    public Vector3 movePosition;

    [Header("Fade Durations")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

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

            if(cameraController != null && nextWallpaper != null)
            {
                cameraController.SetWallpaper(nextWallpaper);
            }

            if (DevilDispatcher.Instance != null)
            {
                DevilDispatcher.Instance.NotifyZoneChange(gameObject);
            }
        }
    }
}
