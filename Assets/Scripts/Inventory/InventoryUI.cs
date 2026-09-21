using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static DropItem;

/// <summary>Creates and updates the inventory panel from UI prefabs.</summary>
public sealed class InventoryUI : MonoBehaviour
{
    private const float KeyboardScrollSpeed = 420f;

    [Header("Prefabs")]
    [SerializeField] private InventoryPanelView panelPrefab;

    private InventoryPanelView panel;
    private PlayerController player;
    private InventoryItemDatabase itemDatabase;
    private readonly List<InventoryItemRowView> itemRows = new();

    public bool IsOpen => panel != null && panel.gameObject.activeSelf;

    public void Initialize(PlayerController owner, InventoryItemDatabase database, Transform canvasTransform)
    {
        player = owner;
        itemDatabase = database;
        if (panel != null) return;

        if (panelPrefab == null)
        {
            Debug.LogWarning("Inventory Panel Prefab is not assigned on InventoryUI.", this);
            return;
        }

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

        panel = Instantiate(panelPrefab, canvasTransform, false);
        panel.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (panel == null) return;

        bool shouldOpen = !panel.gameObject.activeSelf;
        if (shouldOpen) Refresh();
        panel.gameObject.SetActive(shouldOpen);
    }

    public void Refresh()
    {
        if (panel == null || panel.ItemContainer == null || player == null) return;

        ClearItemRows();
        int visibleItemCount = 0;
        foreach (ItemCode item in player.Inventory)
        {
            if (item == ItemCode.None) continue;
            CreateItemRow(item, visibleItemCount++);
        }

        if (panel.EmptyText != null)
            panel.EmptyText.gameObject.SetActive(visibleItemCount == 0);

        float contentHeight = Mathf.Max(panel.MinimumContentHeight, visibleItemCount * panel.RowHeight);
        panel.ItemContainer.sizeDelta = new Vector2(0f, contentHeight);

        if (panel.ScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            panel.ScrollRect.verticalNormalizedPosition = 1f;
        }

        if (panel.Scrollbar != null)
            panel.Scrollbar.gameObject.SetActive(contentHeight > panel.MinimumContentHeight);
    }

    public void Close()
    {
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen || panel.ScrollRect == null || Keyboard.current == null) return;

        float direction = 0f;
        if (Keyboard.current.upArrowKey.isPressed) direction += 1f;
        if (Keyboard.current.downArrowKey.isPressed) direction -= 1f;
        if (!Mathf.Approximately(direction, 0f))
            ScrollByPixels(direction * KeyboardScrollSpeed * Time.unscaledDeltaTime);
    }

    private void ScrollByPixels(float pixelDelta)
    {
        float scrollableHeight = panel.ScrollRect.content.rect.height - panel.ScrollRect.viewport.rect.height;
        if (scrollableHeight <= 0f) return;

        panel.ScrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            panel.ScrollRect.verticalNormalizedPosition + pixelDelta / scrollableHeight);
    }

    private void CreateItemRow(ItemCode item, int index)
    {
        if (panel.ItemRowPrefab == null)
        {
            Debug.LogWarning("Item Row Prefab is not assigned on the inventory panel prefab.", panel);
            return;
        }

        InventoryItemRowView row = Instantiate(panel.ItemRowPrefab, panel.ItemContainer, false);
        RectTransform rowRect = row.transform as RectTransform;
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, -index * panel.RowHeight);
        rowRect.sizeDelta = new Vector2(0f, panel.RowHeight - panel.RowSpacing);

        row.SetItem(item, itemDatabase);
        itemRows.Add(row);
    }

    private void ClearItemRows()
    {
        foreach (InventoryItemRowView row in itemRows)
            if (row != null) Destroy(row.gameObject);
        itemRows.Clear();
    }

    private void OnDestroy()
    {
        if (panel != null) Destroy(panel.gameObject);
    }
}
