using UnityEngine;

public class PlaySoundAnimationEvent : MonoBehaviour
{
    public void PlaySound(AudioClip playSound)
    {
        AudioSource.PlayClipAtPoint(playSound, transform.position);
    }
}
