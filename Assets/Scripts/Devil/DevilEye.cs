using UnityEngine;

public class DevilEye : MonoBehaviour
{
    [SerializeField] private bool setParent = true;
    [SerializeField] private float boundary = 1.0f;
    [SerializeField] private float speed = 1.0f;
    private Vector3 movePosition = Vector3.zero;
    private float threshold = 0.001f;
    private IDevilInterface devilInterface;

    private void Start()
    {
        devilInterface = transform.parent.gameObject.GetComponent<IDevilInterface>();
    }

    private void LateUpdate()
    {
        if(devilInterface != null)
        {
            bool currentState = devilInterface.IsChase();

            if (currentState)
            {
                ChaseMove();
            }
            else
            {
                RandomMove();
            }
        }
    }

    void ChaseMove()
    {
        Vector3 movePoint = devilInterface.InvestigatePoint();

        Vector3 normal = (movePoint - transform.position).normalized;
        Vector3 movePosition = normal * boundary;

        if(setParent)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, movePosition, speed * Time.deltaTime);
        }
    }

    void RandomMove()
    {
        Vector3 diff = transform.localPosition - movePosition;
        if (diff.magnitude < threshold)
        {
            SetNextMovePosition();
        }
        else
        {
            if (setParent)
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
