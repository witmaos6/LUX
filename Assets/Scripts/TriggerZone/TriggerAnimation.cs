using UnityEngine;

public class TriggerAnimation : TriggerCollision
{
    [SerializeField] private Animator animatorObject;
    public override void EnterTrigger(GameObject collisionObject)
    {
        if(animatorObject != null)
        {
            animatorObject.SetBool("Trigger", true);
        }
    }
}
