using UnityEngine;

public class DevilEyeJumpScare : TriggerCollision
{
    [SerializeField] private GameObject jumpScareObject;
    [SerializeField] private AudioClip jumpScareSound;

    private GameObject jumpScareInstance;
    
    public override void EnterTrigger(GameObject collisionObject)
    {
        if(jumpScareObject != null)
        {
            AudioSource.PlayClipAtPoint(jumpScareSound, collisionObject.transform.position);
        }

        if(jumpScareSound != null)
        {
            jumpScareInstance = Instantiate(jumpScareObject, collisionObject.transform);
        }
    }
}
