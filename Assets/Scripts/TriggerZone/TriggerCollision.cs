using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class TriggerCollision : MonoBehaviour
{
    private void Awake()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        if(boxCollider2D != null)
        {
            boxCollider2D.isTrigger = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            EnterTrigger(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            ExitTrigger(collision.gameObject);
        }
    }

    public virtual void EnterTrigger(GameObject collisionObject) { }

    public virtual void ExitTrigger(GameObject collisionObject) { }
}
