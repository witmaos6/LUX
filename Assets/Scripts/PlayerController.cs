using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static DropItem;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        Hide,
    }

    private PlayerState playerState = PlayerState.Normal;
    public float normalSpeed = 5f;
    private float moveSpeed = 5.0f;
    public float sprintSpeed = 8f;
    public float inputDeadZone = 0.01f;

    [SerializeField]
    private bool dirRight = true;

    public bool DirRight => dirRight;

    private bool blockInteraction = false;
    private bool dontMove = false;
    private InputSystem_Actions controls;
    private Rigidbody2D rb;

    private List<InteractionObject> arrowKeyInteractionObjects = new List<InteractionObject>();
    private List<InteractionObject> interactionObjects = new List<InteractionObject>();

    public GameObject flashlightPrefab;
    private GameObject flashlightInstance;

    private List<ItemCode> inventory = new List<ItemCode>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        controls = new InputSystem_Actions();

        moveSpeed = normalSpeed;
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Interaction.canceled += Interaction;
        controls.Player.ArrowInteraction.started += ArrowInteraction;
        controls.Player.Flashlight.started += OnFlashlight;
        controls.Player.Sprint.started += PressSprint;
        controls.Player.Sprint.canceled += ReleasedSprint;
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Player.Interaction.canceled -= Interaction;
        controls.Player.ArrowInteraction.started -= ArrowInteraction;
        controls.Player.Flashlight.started -= OnFlashlight;
        controls.Player.Sprint.started -= PressSprint;
        controls.Player.Sprint.canceled -= ReleasedSprint;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (dontMove == true)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            return;
        }

        Vector2 inputVec = controls.Player.Move.ReadValue<Vector2>();
        float hx = inputVec.x;


        if (Mathf.Abs(hx) <= inputDeadZone)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        rb.linearVelocity = new Vector2(hx * moveSpeed, rb.linearVelocity.y);

        if (hx > 0f)
        {
            dirRight = true;
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (hx < 0f)
        {
            dirRight = false;
            gameObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interaction"))
        {
            InteractionObject interact = collision.GetComponent<InteractionObject>();
            if (interact != null)
            {
                if (interact.interactionType == InteractionObject.InteractionType.ArrowKey && !arrowKeyInteractionObjects.Contains(interact))
                {
                    arrowKeyInteractionObjects.Add(interact);
                }
                else if (interact.interactionType == InteractionObject.InteractionType.InteractionKey && !interactionObjects.Contains(interact))
                {
                    interactionObjects.Add(interact);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interaction"))
        {
            InteractionObject interact = collision.GetComponent<InteractionObject>();
            if (interact != null)
            {
                if (interact.interactionType == InteractionObject.InteractionType.ArrowKey && arrowKeyInteractionObjects.Contains(interact))
                {
                    arrowKeyInteractionObjects.Remove(interact);
                }
                else if (interact.interactionType == InteractionObject.InteractionType.InteractionKey && interactionObjects.Contains(interact))
                {
                    interactionObjects.Remove(interact);
                }
            }
        }
    }

    void Interaction(InputAction.CallbackContext context)
    {
        while (interactionObjects.Count > 0)
        {
            InteractionObject interact = interactionObjects[0];
            if (interact == null)
            {
                interactionObjects.RemoveAt(0);
                continue;
            }

            dontMove = true;
            interact.interactionComplete += AllowInteraction;
            interact.Interaction(gameObject);
            break;
        }
    }

    void ArrowInteraction(InputAction.CallbackContext context)
    {
        if (blockInteraction == true)
            return;

        while (arrowKeyInteractionObjects.Count > 0)
        {
            InteractionObject interact = arrowKeyInteractionObjects[0];
            if (interact == null)
            {
                arrowKeyInteractionObjects.RemoveAt(0);
                continue;
            }

            dontMove = true;
            interact.interactionComplete += AllowInteraction;
            interact.Interaction(gameObject);
            break;
        }
    }

    public void BlockInteraction()
    {
        blockInteraction = true;
    }

    void AllowInteraction()
    {
        blockInteraction = false;
        dontMove = false;
    }

    void OnFlashlight(InputAction.CallbackContext context)
    {
        if (!inventory.Contains(ItemCode.Flashlight))
            return;

        if (flashlightInstance != null)
        {
            bool postState = !flashlightInstance.activeSelf;
            flashlightInstance.SetActive(postState);
        }
        else
        {
            flashlightInstance = Instantiate(flashlightPrefab);
            flashlightInstance.SetActive(true);
            flashlightInstance.transform.SetParent(transform, false);

            DevilDispatcher.Instance.TriggerLight(flashlightInstance);
        }
    }

    void PressSprint(InputAction.CallbackContext context)
    {
        moveSpeed = sprintSpeed;
    }

    void ReleasedSprint(InputAction.CallbackContext context)
    {
        moveSpeed = normalSpeed;
    }

    public void AddItem(ItemCode itemCode)
    {
        if (!inventory.Contains(itemCode))
        {
            inventory.Add(itemCode);
        }
    }

    public bool ExistItem(ItemCode itemCode)
    {
        return inventory.Contains(itemCode);
    }

    public void SetState(PlayerState inState)
    {
        playerState = inState;
    }

    public PlayerState GetState()
    {
        return playerState;
    }
}
