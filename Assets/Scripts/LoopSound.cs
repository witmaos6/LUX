using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopSound : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Sound")]
    public AudioClip loopSound;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = loopSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        audioSource.Play();
    }
}
