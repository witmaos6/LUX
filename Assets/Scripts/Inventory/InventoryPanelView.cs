using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>References authored on the inventory panel prefab.</summary>
public sealed class InventoryPanelView : MonoBehaviour
{
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private InventoryItemRowView itemRowPrefab;
    [SerializeField, Min(1f)] private float rowHeight = 96f;
    [SerializeField, Min(0f)] private float rowSpacing = 8f;
    [SerializeField, Min(1f)] private float minimumContentHeight = 500f;

    public RectTransform ItemContainer => itemContainer;
    public ScrollRect ScrollRect => scrollRect;
    public Scrollbar Scrollbar => scrollbar;
    public TMP_Text EmptyText => emptyText;
    public InventoryItemRowView ItemRowPrefab => itemRowPrefab;
    public float RowHeight => rowHeight;
    public float RowSpacing => rowSpacing;
    public float MinimumContentHeight => minimumContentHeight;
}
