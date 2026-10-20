using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorJumpScare : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 3.0f;
    [SerializeField] float rotationAngle = -70.0f;
    [SerializeField] float startDelay = 0.3f;
    [SerializeField] private AudioClip audioClip;

    private AudioSource audioSource;
    private bool onScare = false;
    private Image childImage;

    private void Awake()
    {
        childImage = GetComponentInChildren<Image>();
        if (childImage != null)
        {
            childImage.enabled = false;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        audioSource.loop = false;
    }
    void Start()
    {
        StartCoroutine(JumpScare());
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

            if(Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                Complete();
            }
        }
    }

    void Complete()
    {
        onScare = false;

        if (audioSource && audioClip)
        {
            audioSource.Play();
        }
    }
}
