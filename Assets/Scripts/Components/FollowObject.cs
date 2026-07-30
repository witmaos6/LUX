using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public GameObject objectToFollow;
    public Vector3 positionOffset = new Vector3(0f, 0f, -10f);
    public bool followRotation = false;
    public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    void LateUpdate()
    {
        if (objectToFollow != null)
        {
            transform.position = objectToFollow.transform.position + positionOffset;
        }
        if(followRotation)
        {
            transform.rotation = objectToFollow.transform.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}
