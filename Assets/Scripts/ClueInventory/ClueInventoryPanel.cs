using System.Collections.Generic;
using UnityEngine;

public class ClueInventoryPanel : MonoBehaviour
{
    [SerializeField] List<ClueInventoryIcon> itemIcon = new List<ClueInventoryIcon>();
    [SerializeField] ClueInventoryItemDatabase clueInventoryItemDatabase;
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
    }
}
