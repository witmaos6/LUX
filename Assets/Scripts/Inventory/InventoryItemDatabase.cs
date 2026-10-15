using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
        public Sprite icon;
        public LocalizedString displayNameRef;
        public LocalizedString descriptionTextRef;
    }

    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private List<ItemDefinition> items = new();

    public TMP_FontAsset UIFont => uiFont;

    public LocalizedString GetDisplayNameRef(ItemCode code)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == code);
        if(item != null)
        {
            return item.displayNameRef;
        }
        return null;
    }

    public LocalizedString GetDescriptionRef(ItemCode code)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == code);
        if (item != null)
        {
            return item.descriptionTextRef;
        }
        return null;
    }

    public Sprite GetIcon(ItemCode itemCode)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == itemCode);
        return item != null ? item.icon : null;
    }
}
