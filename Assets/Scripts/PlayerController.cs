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
        Dead,
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
    private InputAction inventoryAction;
    private Rigidbody2D rb;

    private List<InteractionObject> arrowKeyInteractionObjects = new List<InteractionObject>();
    private List<InteractionObject> interactionObjects = new List<InteractionObject>();
    private readonly Dictionary<InteractionObject, int> interactionContactCounts = new();
    private InteractionObject currentInteraction;

    public GameObject flashlightPrefab;
    private GameObject flashlightInstance;

    [Header("Sound")]
    private AudioSource audioSource;
    public AudioClip deadSound;

    public float moveSoundRangeWeight = 2f;
    public float moveSoundStrength = 30f;

    public Transform canvasTransform;
    public GameObject resurrectionUI;

    [Header("Inventory")]
    [SerializeField] private InventoryItemDatabase inventoryItemDatabase;

    private List<ItemCode> inventory = new List<ItemCode>();
    private InventoryUI inventoryUI;

    public IReadOnlyList<ItemCode> Inventory => inventory;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        controls = new InputSystem_Actions();
        inventoryAction = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/i");

        moveSpeed = normalSpeed;

        audioSource = GetComponent<AudioSource>();

        inventoryUI = GetComponent<InventoryUI>();
        if (inventoryUI == null)
            inventoryUI = gameObject.AddComponent<InventoryUI>();
    }

    private void Start()
    {
        if (canvasTransform == null)
        {
            GameObject canvasObject = GameObject.Find("Canvas");
            if (canvasObject != null)
                canvasTransform = canvasObject.transform;
        }

        inventoryUI.Initialize(this, inventoryItemDatabase, canvasTransform);

        CameraFader cameraFader = GameObject.Find("Main Camera").GetComponent<CameraFader>();
        if(cameraFader != null)
        {
            cameraFader.StartFade(0f, 1f);
        }
    }

    public void ResurrectionEvent()
    {
        if (canvasTransform == null)
            canvasTransform = GameObject.Find("Canvas").transform;

        if(canvasTransform != null && resurrectionUI != null)
        {
            resurrectionUI = Instantiate(resurrectionUI, canvasTransform);
            Destroy(resurrectionUI, 3f);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Interaction.canceled += Interaction;
        controls.Player.ArrowInteraction.started += ArrowInteraction;
        controls.Player.Flashlight.started += OnFlashlight;
        controls.Player.Sprint.started += PressSprint;
        controls.Player.Sprint.canceled += ReleasedSprint;
        inventoryAction.performed += ToggleInventory;
        inventoryAction.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Player.Interaction.canceled -= Interaction;
        controls.Player.ArrowInteraction.started -= ArrowInteraction;
        controls.Player.Flashlight.started -= OnFlashlight;
        controls.Player.Sprint.started -= PressSprint;
        controls.Player.Sprint.canceled -= ReleasedSprint;
        inventoryAction.performed -= ToggleInventory;
        inventoryAction.Disable();

        ClearInteractionHighlights(arrowKeyInteractionObjects);
        ClearInteractionHighlights(interactionObjects);
        interactionContactCounts.Clear();
    }

    private void OnDestroy()
    {
        inventoryAction?.Dispose();
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

        if(playerState == PlayerState.Normal)
        {
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
    }

    public void NotifySound()
    {
        DevilDispatcher.Instance.NotifySuspicionSource(transform.position, moveSpeed * moveSoundRangeWeight, moveSoundStrength);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interaction"))
        {
            InteractionObject interact = collision.GetComponentInParent<InteractionObject>();
            if (interact != null)
            {
                if (interactionContactCounts.TryGetValue(interact, out int contactCount))
                {
                    interactionContactCounts[interact] = contactCount + 1;
                    return;
                }

                interactionContactCounts.Add(interact, 1);

                if (interact.interactionType == InteractionObject.InteractionType.ArrowKey && !arrowKeyInteractionObjects.Contains(interact))
                {
                    arrowKeyInteractionObjects.Add(interact);
                    interact.SetHighlighted(true);
                }
                else if (interact.interactionType == InteractionObject.InteractionType.InteractionKey && !interactionObjects.Contains(interact))
                {
                    interactionObjects.Add(interact);
                    interact.SetHighlighted(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interaction"))
        {
            InteractionObject interact = collision.GetComponentInParent<InteractionObject>();
            if (interact != null)
            {
                if (!interactionContactCounts.TryGetValue(interact, out int contactCount))
                {
                    return;
                }

                if (contactCount > 1)
                {
                    interactionContactCounts[interact] = contactCount - 1;
                    return;
                }

                interactionContactCounts.Remove(interact);

                if (interact.interactionType == InteractionObject.InteractionType.ArrowKey && arrowKeyInteractionObjects.Contains(interact))
                {
                    arrowKeyInteractionObjects.Remove(interact);
                    interact.SetHighlighted(false);
                }
                else if (interact.interactionType == InteractionObject.InteractionType.InteractionKey && interactionObjects.Contains(interact))
                {
                    interactionObjects.Remove(interact);
                    interact.SetHighlighted(false);
                }
            }
        }
    }

    private static void ClearInteractionHighlights(List<InteractionObject> interactions)
    {
        foreach (InteractionObject interact in interactions)
        {
            if (interact != null)
            {
                interact.SetHighlighted(false);
            }
        }

        interactions.Clear();
    }

    void Interaction(InputAction.CallbackContext context)
    {
        if (playerState == PlayerState.Normal)
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
                currentInteraction = interact;
                interact.interactionComplete += AllowInteraction;
                interact.Interaction(gameObject);
                break;
            }
        }
    }

    void ArrowInteraction(InputAction.CallbackContext context)
    {
        if (blockInteraction == true)
            return;

        if (playerState == PlayerState.Normal || playerState == PlayerState.Hide)
        {
            while (arrowKeyInteractionObjects.Count > 0)
            {
                InteractionObject interact = arrowKeyInteractionObjects[0];
                if (interact == null)
                {
                    arrowKeyInteractionObjects.RemoveAt(0);
                    continue;
                }

                dontMove = true;
                currentInteraction = interact;
                interact.interactionComplete += AllowInteraction;
                interact.Interaction(gameObject);
                break;
            }
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
        currentInteraction = null;
    }

    void OnFlashlight(InputAction.CallbackContext context)
    {
        if (!inventory.Contains(ItemCode.Flashlight))
            return;

        if(playerState == PlayerState.Normal || playerState == PlayerState.Hide)
        {
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
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        if (playerState != PlayerState.Dead)
            inventoryUI.Toggle();
    }

    void PressSprint(InputAction.CallbackContext context)
    {
        if(playerState == PlayerState.Normal || playerState == PlayerState.Hide)
        {
            moveSpeed = sprintSpeed;
        }
    }

    void ReleasedSprint(InputAction.CallbackContext context)
    {
        if (playerState == PlayerState.Normal || playerState == PlayerState.Hide)
        {
            moveSpeed = normalSpeed;
        }
    }

    public void AddItem(ItemCode itemCode)
    {
        if (!inventory.Contains(itemCode))
        {
            inventory.Add(itemCode);
            SaveManager.SetInventory(inventory);
            inventoryUI.Refresh();
        }
    }

    public bool ExistItem(ItemCode itemCode)
    {
        return inventory.Contains(itemCode);
    }

    public void RestoreInventory(IReadOnlyList<ItemCode> items)
    {
        inventory.Clear();
        inventory.AddRange(items);
        inventoryUI.Refresh();
    }

    public void SetState(PlayerState inState)
    {
        playerState = inState;
    }

    public PlayerState GetState()
    {
        return playerState;
    }

    public void Dead()
    {
        playerState = PlayerState.Dead;
        inventoryUI.Close();

        if (currentInteraction != null)
        {
            currentInteraction.CancelInteraction();
        }

        audioSource.clip = deadSound;
        audioSource.Play();

        if (DeathScreenController.Instance != null)
            DeathScreenController.Instance.Show();
    }
}
