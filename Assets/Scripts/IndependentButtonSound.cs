using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class IndependentButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound; // 재생할 효과음 에셋
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        Button button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}