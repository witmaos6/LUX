using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClueInventoryPanel : MonoBehaviour
{
    [SerializeField] List<ClueInventoryIcon> itemIcon = new List<ClueInventoryIcon>();
    [SerializeField] ClueInventoryItemDatabase clueInventoryItemDatabase;

    private List<bool> existClueItems = new List<bool>();

    [Header("FocusOutline")]
    [SerializeField] private GameObject outline;
    [SerializeField] private int numberOfX = 2;
    [SerializeField] private int numberOfY = 4;

    private int xCoord = 0;
    private int yCoord = 0;
    private int currentIndex = 0;

    private InputSystem_Actions controls;

    private bool isOpen = false;
    private GameObject uiInstance;

    private int numberOfClue = 0;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        numberOfClue = System.Enum.GetValues(typeof(ClueItemCode)).Length - 1;

        for (int i = 0; i < numberOfClue; i++)
        {
            existClueItems.Add(false);
        }
    }

    private void OnEnable()
    {
        controls.ClueInventory.Navigate.performed += Navigate;
        controls.ClueInventory.OpenUI.started += ToggleUI;
        controls.ClueInventory.Enable();
    }

    private void OnDisable()
    {
        controls.ClueInventory.Navigate.performed -= Navigate;
        controls.ClueInventory.OpenUI.started -= ToggleUI;
        controls.ClueInventory.Disable();
    }

    public void Initialize(List<ClueItemCode> clueItemList)
    {
        if(clueInventoryItemDatabase == null)
        {
            Debug.LogWarning("DB is null");
            return;
        }
        foreach (ClueItemCode clueItemCode in clueItemList)
        {
            int index = (int)clueItemCode - 1;

            if(index < itemIcon.Count)
            {
                itemIcon[index].SetExist(clueInventoryItemDatabase.GetDisplayName(clueItemCode));
                existClueItems[index] = true;
            }
        }
        FocusIcon(itemIcon[0].gameObject.transform);
    }

    void FocusIcon(Transform focusedIcon)
    {
        if(outline != null)
        {
            Vector3 focusedPosition = focusedIcon.position;
            outline.transform.position = new Vector3(focusedPosition.x, focusedPosition.y, focusedPosition.z);
        }
    }

    void Navigate(InputAction.CallbackContext context)
    {
        if (isOpen)
            return;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.y > 0)
        {
            yCoord--;
        }
        else if (direction.y < 0)
        {
            yCoord++; 
        }
        else if (direction.x > 0)
        {
            xCoord++;
        }
        else if (direction.x < 0)
        {
            xCoord--;
        }

        xCoord = Mathf.Clamp(xCoord, 0, numberOfX - 1);
        yCoord = Mathf.Clamp(yCoord, 0, numberOfY - 1);

        currentIndex = Mathf.Clamp(yCoord * numberOfX + xCoord, 0, numberOfClue - 1);
        FocusIcon(itemIcon[currentIndex].gameObject.transform);
    }

    void ToggleUI(InputAction.CallbackContext context)
    {
        if (isOpen)
        {
            CloseUI();
        }
        else
        {
            OpenUI();
        }
    }

    void OpenUI()
    {
        if (existClueItems[currentIndex])
        {
            GameObject uiPrefab = clueInventoryItemDatabase.GetUI((ClueItemCode)currentIndex + 1);

            uiInstance = Instantiate(uiPrefab, transform);
            isOpen = true;
        }
        else
        {
            // To do: 사운드 재생 등 효과 추가
        }
    }

    void CloseUI()
    {
        if (uiInstance)
        {
            Destroy(uiInstance);
            isOpen = false;
        }
    }
}
