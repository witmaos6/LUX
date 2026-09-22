using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClueInventoryPanel : MonoBehaviour
{
    [SerializeField] List<ClueInventoryIcon> itemIcon = new List<ClueInventoryIcon>();
    [SerializeField] ClueInventoryItemDatabase clueInventoryItemDatabase;

    [Header("FocusOutline")]
    [SerializeField] private GameObject outline;
    [SerializeField] private float yOffset = 0f;

    private int xCoord = 0;
    private int yCoord = 0;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
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
                itemIcon[index].IsExist(clueInventoryItemDatabase.GetDisplayName(clueItemCode));
            }
        }
        FocusIcon(itemIcon[0].gameObject.transform);
    }

    void FocusIcon(Transform focusedIcon)
    {
        if(outline != null)
        {
            Vector3 focusedPosition = focusedIcon.position;
            outline.transform.position = new Vector3(focusedPosition.x, focusedPosition.y + yOffset, focusedPosition.z);
        }
    }

    private void Update()
    {
        // To do: 입력 방식 수정 필요
        if (Keyboard.current.upArrowKey.isPressed)
        {
            xCoord = Mathf.Clamp(xCoord - 1, 0, 1);
        }
        else if (Keyboard.current.downArrowKey.isPressed)
        {
            xCoord = Mathf.Clamp(xCoord + 1, 0, 1);
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            yCoord = Mathf.Clamp(yCoord + 1, 0, 3);
        }
        else if (Keyboard.current.leftArrowKey.isPressed)
        {
            yCoord = Mathf.Clamp(yCoord - 1, 0, 3);
        }

        int index = xCoord * 4 + yCoord;
        FocusIcon(itemIcon[index].gameObject.transform);
    }
}
