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
    }

    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private List<ItemDefinition> items = new();

    public TMP_FontAsset UIFont => uiFont;

    public string GetDisplayName(ItemCode itemCode)
    {
        return CreateLocalizedString(itemCode, "DisplayName");
    }

    public Sprite GetIcon(ItemCode itemCode)
    {
        ItemDefinition item = items.Find(definition => definition.itemCode == itemCode);
        return item != null ? item.icon : null;
    }

    public string GetDescription(ItemCode itemCode)
    {
        return CreateLocalizedString(itemCode, "Description");
    }

    string CreateLocalizedString(ItemCode itemCode, string fieldName)
    {
        string Key = $"Item.{itemCode}.{fieldName}";
        string tableName = LocaleService.instance.GetTableName();
        return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, Key);
    }
}
