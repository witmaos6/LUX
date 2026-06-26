using Unity.VisualScripting;
using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    private Vector3 prevPosition;
    private Vector3 currentPosition;

    public bool dirRight = true;
    public bool idle = true;
    public float speed = 0f;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        prevPosition = transform.position;
    }

    private void LateUpdate()
    {
        currentPosition = transform.position;
        if(currentPosition != prevPosition)
        {
            idle = false;
            if(currentPosition.x < prevPosition.x)
            {
                dirRight = false;
            }
            else
            {
                dirRight = true;
            }

            speed = Mathf.Abs((currentPosition - prevPosition).sqrMagnitude);
        }
        else
        {
            if(!idle)
            {
                if(animator != null)
                {
                    animator.SetTrigger("Stop");
                }
            }
            idle = true;
            speed = 0f;
        }

        if (animator != null)
        {
            animator.SetBool("DirRight", dirRight);
            animator.SetFloat("Speed", speed);
            animator.SetBool("Idle", idle);
        }

        prevPosition = currentPosition;
    }
}
