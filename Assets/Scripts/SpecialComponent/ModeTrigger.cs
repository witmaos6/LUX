using UnityEngine;

public class ModeTrigger : MonoBehaviour
{
    private IModeManager modeManager;
    [SerializeField] private bool onTrigger = true;

    private void Start()
    {
        GameObject parentObject = transform.parent.gameObject;
        if(parentObject != null)
        {
            modeManager = parentObject.GetComponent<IModeManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(modeManager != null)
            {
                if(onTrigger)
                {
                    modeManager.ModeOn();
                }
                else
                {
                    modeManager.ModeOff();
                }
            }
        }
    }
}
