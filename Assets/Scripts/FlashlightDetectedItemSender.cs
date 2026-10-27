using UnityEngine;
using static DropItem;

public class FlashlightDetectedItemSender : MonoBehaviour
{
    [Header("Item")]
    [Tooltip("활성화하면 같은 오브젝트의 DropItem에서 아이템 코드를 읽습니다.")]
    [SerializeField] private ItemCode itemCode = ItemCode.None;

    [Header("Receiver")]
    [Tooltip("비어 있으면 씬에서 PlayerController를 자동으로 찾습니다.")]
    [SerializeField] private PlayerController player;

    [Header("Light Detection")]
    [Min(0.01f)]
    [SerializeField] private float lightRangeMultiplier = 1f;
    [Min(0f)]
    [SerializeField] private float requiredIlluminationTime = 0.4f;

    private bool hasSent = false;
    private float illuminatedTime = 0f;

    private void Start()
    {
        ResolvePlayer();
    }

    private void Update()
    {
        if (!hasSent)
        {
            FlashlightDetectionResult lightResult = FlashlightVisibilityService.Evaluate(transform.position, lightRangeMultiplier);

            if(lightResult.IsIlluminated)
            {
                illuminatedTime += Time.deltaTime;
                if (illuminatedTime >= requiredIlluminationTime)
                    SendItemCodeToPlayer();
            }
        }
    }

    private void SendItemCodeToPlayer()
    {
        if (itemCode == ItemCode.None)
        {
            Debug.LogWarning($"{name}: 전달할 아이템 코드가 지정되지 않았습니다.", this);
            return;
        }

        if (player == null)
            ResolvePlayer();

        if (player == null)
        {
            Debug.LogWarning($"{name}: 아이템 코드를 전달할 PlayerController를 찾을 수 없습니다.", this);
            return;
        }

        player.AddItem(itemCode);
        hasSent = true;
    }

    private void ResolvePlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }

}
