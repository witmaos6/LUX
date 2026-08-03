using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static DropItem;

public class PasswordUnlockObject : InteractionObject
{
    public string correctAnswer;
    public GameObject passwordKeypad;
    public Transform canvasTransform;

    [Header("Hint")]
    [Tooltip("The hint is shown when the player owns at least one of these items.")]
    [SerializeField] private List<ItemCode> hintItemCodes = new();

    private GameObject uiInstance;

    public GameEvent passwordSuccessEvent;

    public float dispatchSoundStrength = 70f;
    public float dispatchSoundRange = 30f;
    public AudioClip audioClip;

    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if (canvasTransform == null)
        {
            canvasTransform = GameObject.Find("Canvas").transform;
        }
    }
    public override void ActivateInteraction(GameObject tryObject)
    {
        if(passwordKeypad != null)
        {
            uiInstance = Instantiate(passwordKeypad, canvasTransform);

            PasswordUIController uiController = uiInstance.GetComponent<PasswordUIController>();
            if (uiController != null)
            {
                PlayerController playerController = tryObject.GetComponent<PlayerController>();
                uiController.Init(this, HasHintItem(playerController), correctAnswer);
            }
        }
    }

    private bool HasHintItem(PlayerController playerController)
    {
        if (playerController == null || hintItemCodes.Count == 0)
            return false;

        foreach (ItemCode itemCode in hintItemCodes)
        {
            if (itemCode != ItemCode.None && !playerController.ExistItem(itemCode))
                return false;
        }

        return true;
    }

    public override void InputPressed()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
        }
        EndInteraction();
    }

    public bool CheckPassword(string inputPassword)
    {
        if (inputPassword == correctAnswer)
        {
            OnUnlockSuccess();
            return true;
        }
        else
        {
            OnUnlockFailed();
            return false;
        }
    }

    private void OnUnlockSuccess()
    {
        if(passwordSuccessEvent != null)
        {
            GameEventManager.Raise(passwordSuccessEvent);
        }

        if(audioClip != null)
        {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }

        DevilDispatcher.Instance.NotifySuspicionSource(transform.position, dispatchSoundRange, dispatchSoundStrength);

        EndInteraction();
    }

    private void OnUnlockFailed()
    {

    }

    public override void CancelInteraction()
    {
        if (uiInstance != null)
            Destroy(uiInstance);

        base.CancelInteraction();
    }
}
