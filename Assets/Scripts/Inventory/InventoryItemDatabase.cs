using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DropItem;

[CreateAssetMenu(
    fileName = "InventoryItemDatabase",
    menuName = "LUX2D/Inventory Item Database")]
public sealed class InventoryItemDatabase : ScriptableObject
{
    [Serializable]
    public class ItemDefinition
    {
        public ItemCode itemCode;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public Sprite icon;
    }

    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private List<ItemDefinition> items = new();

    public TMP_FontAsset UIFont => uiFont;

    public string GetDisplayName(ItemCode itemCode)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == itemCode);
        return item != null && !string.IsNullOrWhiteSpace(item.displayName)
            ? item.displayName
            : itemCode.ToString();
    }

    public Sprite GetIcon(ItemCode itemCode)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == itemCode);
        return item != null ? item.icon : null;
    }

    public string GetDescription(ItemCode itemCode)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == itemCode);
        return item != null ? item.description : string.Empty;
    }
}
