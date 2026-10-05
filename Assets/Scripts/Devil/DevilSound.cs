using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DevilSound : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip basicSound;
    public AudioClip detectSound;
    public AudioClip investigateSound;

    private AudioSource audioSource;
    private bool playingEffectSound = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
    }

    private void Start()
    {
        audioSource.clip = basicSound;
        audioSource.Play();
    }

    public void PlayDetectSound()
    {
        if(playingEffectSound == false)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(detectSound);

            StartCoroutine(SetPlayEffectSound(detectSound.length));
        }
    }

    IEnumerator SetPlayEffectSound(float soundLength)
    {
        playingEffectSound = true;
        yield return new WaitForSeconds(soundLength);

        playingEffectSound = false;
        audioSource.Play();
    }

    public void PlayInvestigateSound()
    {
        if(playingEffectSound == false)
        {
            StartCoroutine(DelayInvestigateSound());
        }
    }

    IEnumerator DelayInvestigateSound()
    {
        yield return new WaitForSeconds(1f);

        audioSource.Stop();
        audioSource.PlayOneShot(investigateSound);

        StartCoroutine(SetPlayEffectSound(investigateSound.length));
    }
}
