using UnityEngine;
using static DropItem;

[DisallowMultipleComponent]
[RequireComponent(typeof(CameraVisibilityNotifier))]
public sealed class CameraDetectedItemSender : MonoBehaviour
{
    [Header("Item")]
    [Tooltip("활성화하면 같은 오브젝트의 DropItem에서 아이템 코드를 읽습니다.")]
    [SerializeField] private bool useDropItemCode = true;
    [SerializeField] private DropItem itemSource;
    [SerializeField] private ItemCode itemCode = ItemCode.None;

    [Header("Receiver")]
    [Tooltip("비어 있으면 씬에서 PlayerController를 자동으로 찾습니다.")]
    [SerializeField] private PlayerController player;
    [SerializeField] private bool sendOnlyOnce = true;

    private CameraVisibilityNotifier visibilityNotifier;
    private bool hasSent;

    private void Awake()
    {
        visibilityNotifier = GetComponent<CameraVisibilityNotifier>();

        if (itemSource == null && useDropItemCode)
            itemSource = GetComponent<DropItem>();

        ResolvePlayer();
    }

    private void OnEnable()
    {
        if (visibilityNotifier == null)
            visibilityNotifier = GetComponent<CameraVisibilityNotifier>();

        visibilityNotifier.EnteredCamera += SendItemCodeToPlayer;
    }

    private void OnDisable()
    {
        if (visibilityNotifier != null)
            visibilityNotifier.EnteredCamera -= SendItemCodeToPlayer;
    }

    private void SendItemCodeToPlayer()
    {
        if (sendOnlyOnce && hasSent)
            return;

        ItemCode codeToSend = GetItemCode();
        if (codeToSend == ItemCode.None)
        {
            Debug.LogWarning(
                $"{name}: 카메라 감지 시 전달할 아이템 코드가 지정되지 않았습니다.",
                this);
            return;
        }

        if (player == null)
            ResolvePlayer();

        if (player == null)
        {
            Debug.LogWarning(
                $"{name}: 아이템 코드를 전달할 PlayerController를 찾을 수 없습니다.",
                this);
            return;
        }

        player.AddItem(codeToSend);
        hasSent = true;
    }

    private ItemCode GetItemCode()
    {
        if (useDropItemCode && itemSource != null)
            return itemSource.itemCode;

        return itemCode;
    }

    private void ResolvePlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }
}
