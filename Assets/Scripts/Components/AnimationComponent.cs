using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    [SerializeField]
    private float idleSpeedThreshold = 0.05f;

    private Vector3 prevPosition;
    private Animator animator;
    private Rigidbody2D rb;

    public bool dirRight = true;
    public bool idle = true;
    public float speed = 0f;
    public bool hide = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        prevPosition = transform.position;
    }

    // transform.position을 직접 대입해 순간이동시키는 쪽(Door, Devil 존 이동, 체크포인트 리스폰 등)에서
    // 이동 직후 호출해야 한다. 안 그러면 그 텔레포트가 한 프레임짜리 이동으로 오인되어
    // dirRight가 엉뚱하게 뒤집히거나 Stop 트리거가 잘못 발동한다.
    public void ResyncPosition()
    {
        prevPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector2 velocity = rb != null ? rb.linearVelocity : GetVelocityFromPositionDelta();

        speed = velocity.magnitude;

        bool wasIdle = idle;
        idle = speed <= idleSpeedThreshold;

        if (!idle)
        {
            dirRight = velocity.x >= 0f;
        }

        if (animator != null)
        {
            if (idle && !wasIdle)
            {
                animator.SetTrigger("Stop");
            }

            animator.SetBool("DirRight", dirRight);
            animator.SetFloat("Speed", speed);
            animator.SetBool("Idle", idle);
        }

        prevPosition = transform.position;
    }

    private Vector2 GetVelocityFromPositionDelta()
    {
        if (Time.deltaTime <= 0f)
            return Vector2.zero;

        return (transform.position - prevPosition) / Time.deltaTime;
    }

    public void SetHide(bool inHide)
    {
        hide = inHide;

        animator.SetBool("Hide", hide);
    }
}
