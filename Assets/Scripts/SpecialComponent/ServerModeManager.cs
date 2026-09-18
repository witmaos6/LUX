using UnityEngine;

public class ServerModeManager : MonoBehaviour, IModeManager
{
    private bool modeOn = false;

    [SerializeField] private GameObject devilObject;
    private GameObject devilInstance;
    private RandomPatrolDevil devilAI;

    [Header("DevilPatrolZone")]
    [SerializeField] private float xLength = 20f;
    [SerializeField] private float yLength = 20f;


    public void ModeOn()
    {
        if (modeOn == true)
            return;
        
        modeOn = true;

        if(devilObject)
        {
            devilInstance = Instantiate(devilObject, transform, true);
            if(devilInstance != null)
            {
                devilInstance.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

                devilAI = devilInstance.GetComponent<RandomPatrolDevil>();
                if(devilAI != null)
                {
                    devilAI.SetModeManager(this);
                    devilAI.SetPatrolPoint(PatrolPoint());
                }
            }
        }
    }

    public Vector3 PatrolPoint()
    {
        Vector3 origin = transform.localPosition;
        float randomX = Random.Range(-xLength / 2, xLength / 2);
        float randomY = Random.Range(-yLength / 2, yLength / 2);

        return new Vector3(origin.x + randomX, origin.y + randomY, 0f);
    }

    public void ModeOff()
    {
        if (modeOn == false)
            return;

        modeOn = false;

        if(devilInstance)
        {
            Destroy(devilInstance);
        }
    }

    private void OnDisable()
    {
        Destroy(devilInstance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(xLength, yLength, 0f));
    }
}
