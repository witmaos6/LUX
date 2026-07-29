using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DropItem;

public sealed class InventoryUI : MonoBehaviour
{
    private GameObject panel;
    private RectTransform itemContainer;
    private Text emptyText;
    private PlayerController player;
    private InventoryItemDatabase itemDatabase;
    private readonly List<GameObject> itemRows = new();

    public bool IsOpen => panel != null && panel.activeSelf;

    public void Initialize(
        PlayerController owner,
        InventoryItemDatabase database,
        Transform canvasTransform)
    {
        player = owner;
        itemDatabase = database;

        if (panel != null)
            return;

        if (canvasTransform == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            canvasTransform = canvas != null ? canvas.transform : null;
        }

        if (canvasTransform == null)
        {
            Debug.LogWarning("A Canvas could not be found for the inventory UI.", this);
            return;
        }

        CreatePanel(canvasTransform);
        panel.SetActive(false);
    }

    public void Toggle()
    {
        if (panel == null)
            return;

        bool shouldOpen = !panel.activeSelf;
        if (shouldOpen)
            Refresh();

        panel.SetActive(shouldOpen);
    }

    public void Refresh()
    {
        if (itemContainer == null || player == null)
            return;

        ClearItemRows();

        IReadOnlyList<ItemCode> items = player.Inventory;
        int visibleItemCount = 0;
        foreach (ItemCode item in items)
        {
            if (item == ItemCode.None)
                continue;

            CreateItemRow(item, visibleItemCount);
            visibleItemCount++;
        }

        emptyText.gameObject.SetActive(visibleItemCount == 0);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void CreatePanel(Transform canvasTransform)
    {
        panel = new GameObject("Inventory Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasTransform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 360f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.04f, 0.05f, 0.94f);

        CreateText(
            "Title",
            panel.transform,
            "INVENTORY",
            30,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 115f),
            new Vector2(360f, 60f));

        GameObject containerObject = new GameObject("Item List", typeof(RectTransform));
        containerObject.transform.SetParent(panel.transform, false);
        itemContainer = containerObject.GetComponent<RectTransform>();
        itemContainer.anchorMin = new Vector2(0.5f, 0.5f);
        itemContainer.anchorMax = new Vector2(0.5f, 0.5f);
        itemContainer.pivot = new Vector2(0.5f, 0.5f);
        itemContainer.anchoredPosition = new Vector2(0f, -15f);
        itemContainer.sizeDelta = new Vector2(330f, 210f);

        emptyText = CreateText(
            "Empty",
            itemContainer,
            "No items",
            23,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            Vector2.zero,
            itemContainer.sizeDelta);

        CreateText(
            "Close Hint",
            panel.transform,
            "Press I to close",
            17,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -145f),
            new Vector2(360f, 35f));
    }

    private void CreateItemRow(ItemCode item, int index)
    {
        GameObject row = new GameObject("Item - " + item, typeof(RectTransform));
        row.transform.SetParent(itemContainer, false);
        itemRows.Add(row);

        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, -index * 54f);
        rowRect.sizeDelta = new Vector2(0f, 50f);

        GameObject iconObject = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        iconObject.transform.SetParent(row.transform, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(48f, 48f);

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = itemDatabase != null ? itemDatabase.GetIcon(item) : null;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.enabled = iconImage.sprite != null;

        Text nameText = CreateText(
            "Name",
            panel.transform,
            itemDatabase != null ? itemDatabase.GetDisplayName(item) : item.ToString(),
            23,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            Vector2.zero,
            Vector2.zero);
        nameText.transform.SetParent(row.transform, false);

        RectTransform nameRect = nameText.rectTransform;
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.offsetMin = new Vector2(62f, 0f);
        nameRect.offsetMax = Vector2.zero;
    }

    private void ClearItemRows()
    {
        foreach (GameObject row in itemRows)
        {
            if (row == null)
                continue;

            row.SetActive(false);
            Destroy(row);
        }

        itemRows.Clear();
    }

    private static Text CreateText(
        string objectName,
        Transform parent,
        string content,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private void OnDestroy()
    {
        if (panel != null)
            Destroy(panel);
    }
}
