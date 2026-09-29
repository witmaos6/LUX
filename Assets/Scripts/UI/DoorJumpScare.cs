using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorJumpScare : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 3.0f;
    [SerializeField] float rotationAngle = -70.0f;
    [SerializeField] float startDelay = 0.3f;

    private bool onScare = false;
    private Image childImage;
    void Start()
    {
        StartCoroutine(JumpScare());

        childImage = GetComponentInChildren<Image>();
        if(childImage != null)
        {
            childImage.enabled = false;
        }
    }

    IEnumerator JumpScare()
    {
        yield return new WaitForSeconds(startDelay);
        onScare = true;
        childImage.enabled = true;
    }

    void Update()
    {
        if(onScare)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotationAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
