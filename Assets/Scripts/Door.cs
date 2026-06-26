using UnityEngine;

public class Door : InteractionObject
{
    public GameObject moveTarget;
    public CameraFader cameraFader;
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
        }
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

            if (DevilDispatcher.Instance != null)
            {
                DevilDispatcher.Instance.NotifyZoneChange(gameObject);
            }
        }
    }
}
