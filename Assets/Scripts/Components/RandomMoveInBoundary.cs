using System.Collections;
using TMPro;
using UnityEngine;

public class RandomMoveInBoundary : MonoBehaviour
{
    [SerializeField] private bool setParent = true; // 추후에 부모 - 자식 관계가 아닐 경우에 사용
    [SerializeField] private float boundary = 1.0f; // 현재는 원을 기준으로 랜덤이동만 지원
    [SerializeField] private float speed = 1.0f;
    private Vector3 movePosition = Vector3.zero;
    private float threshold = 0.001f;

    private void LateUpdate()
    {
        Vector3 diff = transform.localPosition - movePosition;
        if (diff.magnitude < threshold)
        {
            SetNextMovePosition();
        }
        else
        {
            if(setParent)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, movePosition, speed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, movePosition, speed * Time.deltaTime);
            }
        }
    }

    void SetNextMovePosition()
    {
        Vector2 circlePoint = Random.insideUnitCircle * boundary;

        movePosition = new Vector3(circlePoint.x, circlePoint.y, 0);
    }
}
