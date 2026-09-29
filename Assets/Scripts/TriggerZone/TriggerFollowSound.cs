using System.Collections;
using UnityEngine;

public class TriggerFollowSound : TriggerCollision
{
    [SerializeField] private float backOffset = 1f;
    [SerializeField] private float soundDelay = 0.1f;

    private PlaySoundAnimationEvent playSoundEvent;
    private Transform soundTransform;
    // private AudioSource audioSource;

    private void Start()
    {
        //audioSource = GetComponent<AudioSource>();

        //audioSource.playOnAwake = false;
        //audioSource.loop = false;
        //audioSource.spatialBlend = 1f;
    }

    public override void EnterTrigger(GameObject collisionObject)
    {
        playSoundEvent = collisionObject.GetComponent<PlaySoundAnimationEvent>();
        if(playSoundEvent != null)
        {
            playSoundEvent.playSoundDelegate += SoundReady;

            soundTransform = collisionObject.transform;
        }
    }

    void SoundReady(AudioClip clip, float pitch, float volumeScale)
    {
        StartCoroutine(PlaySound(clip, pitch, volumeScale));
    }

    IEnumerator PlaySound(AudioClip clip, float pitch, float volumeScale)
    {
        yield return new WaitForSeconds(soundDelay);

        if (soundTransform)
        {
            Vector3 originPosition = soundTransform.position;
            Vector3 backPosition = -soundTransform.right * backOffset;

            Vector3 soundPosition = backPosition + originPosition;

            AudioSource.PlayClipAtPoint(clip, soundPosition);
        }
    }

    public override void ExitTrigger(GameObject collisionObject)
    {
        if (playSoundEvent != null)
        {
            playSoundEvent.playSoundDelegate -= SoundReady;
        }
        Destroy(gameObject); // 추후에 변경 가능
    }

    private void OnDisable()
    {
        if (playSoundEvent != null)
        {
            playSoundEvent.playSoundDelegate -= SoundReady;
        }
    }
}
