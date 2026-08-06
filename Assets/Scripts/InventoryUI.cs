using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static DropItem;

public sealed class InventoryUI : MonoBehaviour
{
    private const float KeyboardScrollSpeed = 420f;

    private GameObject panel;
    private RectTransform itemContainer;
    private ScrollRect itemScrollRect;
    private Scrollbar itemScrollbar;
    private TMP_Text emptyText;
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

        float contentHeight = Mathf.Max(500f, visibleItemCount * 96f);
        itemContainer.sizeDelta = new Vector2(0f, contentHeight);

        if (itemScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            itemScrollRect.verticalNormalizedPosition = 1f;
        }

        if (itemScrollbar != null)
            itemScrollbar.gameObject.SetActive(contentHeight > 500f);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen || itemScrollRect == null || Keyboard.current == null)
            return;

        float direction = 0f;
        if (Keyboard.current.upArrowKey.isPressed)
            direction += 1f;
        if (Keyboard.current.downArrowKey.isPressed)
            direction -= 1f;

        if (!Mathf.Approximately(direction, 0f))
            ScrollByPixels(direction * KeyboardScrollSpeed * Time.unscaledDeltaTime);
    }

    private void ScrollByPixels(float pixelDelta)
    {
        float viewportHeight = itemScrollRect.viewport.rect.height;
        float contentHeight = itemScrollRect.content.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;
        if (scrollableHeight <= 0f)
            return;

        itemScrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            itemScrollRect.verticalNormalizedPosition + pixelDelta / scrollableHeight);
    }

    private void CreatePanel(Transform canvasTransform)
    {
        panel = new GameObject("Inventory Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasTransform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900f, 700f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.04f, 0.05f, 0.94f);

        CreateText(
            "Title",
            panel.transform,
            "INVENTORY",
            38,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Vector2(0f, 290f),
            new Vector2(820f, 70f));

        CreateItemScrollView(panel.transform);

        emptyText = CreateText(
            "Empty",
            itemContainer,
            "No items",
            28,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            Vector2.zero,
            itemContainer.sizeDelta);

        CreateText(
            "Close Hint",
            panel.transform,
            "Press I to close",
            21,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Vector2(0f, -315f),
            new Vector2(820f, 40f));
    }

    private void CreateItemScrollView(Transform parent)
    {
        GameObject scrollView = new GameObject("Item Scroll View", typeof(RectTransform), typeof(ScrollRect));
        scrollView.transform.SetParent(parent, false);

        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = Vector2.zero;
        scrollRect.sizeDelta = new Vector2(800f, 500f);

        GameObject viewportObject = new GameObject(
            "Viewport",
            typeof(RectTransform),
            typeof(Image),
            typeof(RectMask2D));
        viewportObject.transform.SetParent(scrollView.transform, false);

        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = new Vector2(-24f, 0f);

        // ScrollRect needs a raycast target inside its bounds to receive wheel events.
        Image viewportInputSurface = viewportObject.GetComponent<Image>();
        viewportInputSurface.color = Color.clear;
        viewportInputSurface.raycastTarget = true;

        GameObject containerObject = new GameObject("Item List", typeof(RectTransform));
        containerObject.transform.SetParent(viewportObject.transform, false);
        itemContainer = containerObject.GetComponent<RectTransform>();
        itemContainer.anchorMin = new Vector2(0f, 1f);
        itemContainer.anchorMax = new Vector2(1f, 1f);
        itemContainer.pivot = new Vector2(0.5f, 1f);
        itemContainer.anchoredPosition = Vector2.zero;
        itemContainer.sizeDelta = new Vector2(0f, 500f);

        itemScrollRect = scrollView.GetComponent<ScrollRect>();
        itemScrollRect.viewport = viewport;
        itemScrollRect.content = itemContainer;
        itemScrollRect.horizontal = false;
        itemScrollRect.vertical = true;
        itemScrollRect.scrollSensitivity = 32f;
        itemScrollRect.movementType = ScrollRect.MovementType.Clamped;

        itemScrollbar = CreateScrollbar(scrollView.transform);
        itemScrollRect.verticalScrollbar = itemScrollbar;
        itemScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
    }

    private Scrollbar CreateScrollbar(Transform parent)
    {
        GameObject scrollbarObject = new GameObject(
            "Scrollbar",
            typeof(RectTransform),
            typeof(Image),
            typeof(Scrollbar));
        scrollbarObject.transform.SetParent(parent, false);

        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 0.5f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(16f, 0f);

        Image background = scrollbarObject.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.15f);

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObject.transform.SetParent(scrollbarObject.transform, false);

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = new Vector2(2f, 2f);
        handleRect.offsetMax = new Vector2(-2f, -2f);

        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRect;
        return scrollbar;
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
        rowRect.anchoredPosition = new Vector2(0f, -index * 96f);
        rowRect.sizeDelta = new Vector2(0f, 88f);

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
        iconRect.sizeDelta = new Vector2(82f, 82f);

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = itemDatabase != null ? itemDatabase.GetIcon(item) : null;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.enabled = iconImage.sprite != null;

        TMP_Text nameText = CreateText(
            "Name",
            row.transform,
            itemDatabase != null ? itemDatabase.GetDisplayName(item) : item.ToString(),
            27,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Vector2.zero,
            Vector2.zero);

        RectTransform nameRect = nameText.rectTransform;
        nameRect.anchorMin = new Vector2(0f, 0.5f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.offsetMin = new Vector2(100f, 0f);
        nameRect.offsetMax = Vector2.zero;

        TMP_Text descriptionText = CreateText(
            "Description",
            row.transform,
            itemDatabase != null ? itemDatabase.GetDescription(item) : string.Empty,
            18,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Vector2.zero,
            Vector2.zero);
        descriptionText.color = new Color(0.78f, 0.78f, 0.78f, 1f);
        descriptionText.textWrappingMode = TextWrappingModes.Normal;
        // DOSGothic SDF does not contain U+2026 (ellipsis). Using Ellipsis here
        // makes TMP emit a warning and fall back to Truncate every time it renders.
        descriptionText.overflowMode = TextOverflowModes.Truncate;

        RectTransform descriptionRect = descriptionText.rectTransform;
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(1f, 0.55f);
        descriptionRect.pivot = new Vector2(0.5f, 0.5f);
        descriptionRect.offsetMin = new Vector2(100f, 0f);
        descriptionRect.offsetMax = Vector2.zero;
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

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string content,
        int fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        if (itemDatabase != null && itemDatabase.UIFont != null)
            text.font = itemDatabase.UIFont;
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private void OnDestroy()
    {
        if (panel != null)
            Destroy(panel);
    }
}
