using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClueInventory : MonoBehaviour
{
    private List<ClueItemCode> clueItemList = new List<ClueItemCode>();

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanelUI;
    [SerializeField] private float DestroyTime = 5f;

    private ClueInventoryPanel inventoryPanel;
    private Coroutine inventoryCloseCoroutine;

    public void AddItem(ClueItemCode item)
    {
        if (!clueItemList.Contains(item))
        {
            clueItemList.Add(item);
            SaveManager.SetClueInventory(clueItemList);
        }
    }

    public void RestoreInventory(IReadOnlyList<ClueItemCode> items)
    {
        clueItemList.Clear();
        clueItemList.AddRange(items);
    }

    public void Toggle(Transform canvasTransform)
    {
        if (inventoryPanel != null && inventoryPanel.gameObject.activeSelf == true)
        {
            PlayerController playerController = gameObject.GetComponent<PlayerController>();
            if (playerController != null && playerController.GetState() == PlayerController.PlayerState.OpenUI)
            {
                playerController.SetState(PlayerController.PlayerState.Normal);
                ClosePanel();
            }
        }
        else
        {
            PlayerController playerController = gameObject.GetComponent<PlayerController>();
            if (playerController != null && playerController.GetState() == PlayerController.PlayerState.Normal)
            {
                playerController.SetState(PlayerController.PlayerState.OpenUI);
                OpenPanel(canvasTransform);
            }
        }
    }

    private void OpenPanel(Transform canvasTransform)
    {
        if (inventoryPanel != null)
        {
            if (inventoryCloseCoroutine != null)
            {
                StopCoroutine(inventoryCloseCoroutine);
            }
            inventoryPanel.gameObject.SetActive(true);
        }
        else
        {
            GameObject inventoryPanelInstance = Instantiate(inventoryPanelUI, canvasTransform, false);
            if (inventoryPanelInstance != null)
            {
                inventoryPanel = inventoryPanelInstance.GetComponent<ClueInventoryPanel>();

                if (inventoryPanel != null)
                {
                    inventoryPanel.Initialize(clueItemList);
                }
            }
        }
        
    }

    private void ClosePanel()
    {
        inventoryPanel.gameObject.SetActive(false);
        inventoryCloseCoroutine = StartCoroutine(DestoryPanel());

        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetState(PlayerController.PlayerState.Normal);
        }
    }

    IEnumerator DestoryPanel()
    {
        yield return new WaitForSeconds(DestroyTime);

        if(inventoryPanel != null)
        {
            Destroy(inventoryPanel.gameObject);
        }
    }

    private void OnDisable()
    {
        if (inventoryCloseCoroutine != null)
        {
            StopCoroutine(inventoryCloseCoroutine);
            if (inventoryPanel != null)
            {
                Destroy(inventoryPanel.gameObject);
            }
        }
    }
}
