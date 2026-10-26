using UnityEngine;

public class RandomMoveInBoundaryUI : MonoBehaviour
{
    [SerializeField] private float boundary = 50f;
    [SerializeField] private float speed = 30f;
    [SerializeField] private bool useInitialPositionAsCenter = true;

    private RectTransform rectTransform;
    private Vector2 center;
    private Vector2 movePosition;
    private const float Threshold = 0.01f;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    private void OnEnable()
    {
        center = useInitialPositionAsCenter ? rectTransform.anchoredPosition : Vector2.zero;
        SetNextMovePosition();
    }

    private void LateUpdate()
    {
        Vector2 current = rectTransform.anchoredPosition;

        if ((current - movePosition).sqrMagnitude < Threshold * Threshold)
        {
            SetNextMovePosition();
            return;
        }

        rectTransform.anchoredPosition =
            Vector2.MoveTowards(current, movePosition, speed * Time.deltaTime);
    }

    private void SetNextMovePosition()
    {
        movePosition = center + Random.insideUnitCircle * boundary;
    }
}
