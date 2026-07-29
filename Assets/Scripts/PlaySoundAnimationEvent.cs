using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Animation Event에서 짧은 효과음을 재생하기 위한 범용 컴포넌트입니다.
/// Unreal Engine의 AnimNotify처럼 클립 직접 전달, 목록의 랜덤/인덱스 재생을 지원합니다.
/// </summary>
[DisallowMultipleComponent]
public class PlaySoundAnimationEvent : MonoBehaviour
{
    public enum PlaybackPosition
    {
        FollowTransform,
        AtEventPosition
    }

    [Header("Sound")]
    [Tooltip("비워두면 이 컴포넌트 전용 AudioSource를 런타임에 생성합니다.")]
    [SerializeField] private AudioSource outputAudioSource;

    [Tooltip("PlayRandomSound 또는 PlaySoundByIndex에서 사용할 클립 목록입니다.")]
    [SerializeField] private AudioClip[] soundClips;

    [SerializeField] private AudioMixerGroup outputMixerGroup;
    [SerializeField] private PlaybackPosition playbackPosition = PlaybackPosition.FollowTransform;
    [SerializeField, Range(0f, 1f)] private float spatialBlend;

    [Header("Variation")]
    [SerializeField, Min(0f)] private float volume = 1f;
    [SerializeField] private Vector2 randomVolumeMultiplier = new Vector2(0.95f, 1f);
    [SerializeField] private Vector2 randomPitch = new Vector2(0.97f, 1.03f);

    [Header("Event Filtering")]
    [Tooltip("상태 전환이나 짧은 블렌드 중 같은 이벤트가 연속 호출되는 것을 막습니다. 0이면 사용하지 않습니다.")]
    [SerializeField, Min(0f)] private float minimumInterval;

    [Tooltip("켜면 Rigidbody2D가 실제로 움직일 때만 소리를 냅니다.")]
    [SerializeField] private bool requireMovement;

    [SerializeField, Min(0f)] private float minimumSpeed = 0.05f;

    private Rigidbody2D cachedRigidbody;
    private float lastPlayedTime = float.NegativeInfinity;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();

        if (outputAudioSource == null && playbackPosition == PlaybackPosition.FollowTransform)
        {
            GameObject sourceObject = new GameObject("Animation Event Audio");
            sourceObject.transform.SetParent(transform, false);
            outputAudioSource = sourceObject.AddComponent<AudioSource>();
            ConfigureSource(outputAudioSource);
        }
    }

    /// <summary>
    /// Animation Event의 Object 파라미터로 전달한 AudioClip을 재생합니다.
    /// 기존 애니메이션 이벤트와 호환됩니다.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        TryPlay(clip);
    }

    /// <summary>
    /// soundClips 중 하나를 무작위로 재생합니다. 파라미터 없는 Animation Event용입니다.
    /// </summary>
    public void PlayRandomSound()
    {
        if (soundClips == null || soundClips.Length == 0)
            return;

        TryPlay(soundClips[Random.Range(0, soundClips.Length)]);
    }

    /// <summary>
    /// Animation Event의 Int 파라미터에 해당하는 클립을 재생합니다.
    /// </summary>
    public void PlaySoundByIndex(int index)
    {
        if (soundClips == null || index < 0 || index >= soundClips.Length)
            return;

        TryPlay(soundClips[index]);
    }

    /// <summary>
    /// FollowTransform 모드에서 재생 중인 모든 효과음을 정지합니다.
    /// </summary>
    public void StopSound()
    {
        if (outputAudioSource != null)
            outputAudioSource.Stop();
    }

    private void TryPlay(AudioClip clip)
    {
        if (clip == null || !CanPlay())
            return;

        float pitch = Random.Range(
            Mathf.Min(randomPitch.x, randomPitch.y),
            Mathf.Max(randomPitch.x, randomPitch.y));

        float volumeScale = volume * Random.Range(
            Mathf.Min(randomVolumeMultiplier.x, randomVolumeMultiplier.y),
            Mathf.Max(randomVolumeMultiplier.x, randomVolumeMultiplier.y));

        lastPlayedTime = Time.time;

        if (playbackPosition == PlaybackPosition.AtEventPosition)
            PlayAtEventPosition(clip, pitch, volumeScale);
        else
            PlayFollowingTransform(clip, pitch, volumeScale);
    }

    private bool CanPlay()
    {
        if (minimumInterval > 0f && Time.time - lastPlayedTime < minimumInterval)
            return false;

        if (!requireMovement)
            return true;

        return cachedRigidbody != null &&
               cachedRigidbody.linearVelocity.sqrMagnitude >= minimumSpeed * minimumSpeed;
    }

    private void PlayFollowingTransform(AudioClip clip, float pitch, float volumeScale)
    {
        if (outputAudioSource == null)
            return;

        ConfigureSource(outputAudioSource);
        outputAudioSource.pitch = pitch;
        outputAudioSource.PlayOneShot(clip, volumeScale);
    }

    private void PlayAtEventPosition(AudioClip clip, float pitch, float volumeScale)
    {
        GameObject temporaryObject = new GameObject("One Shot Animation Sound");
        temporaryObject.transform.position = transform.position;

        AudioSource source = temporaryObject.AddComponent<AudioSource>();
        ConfigureSource(source);
        source.pitch = pitch;
        source.clip = clip;
        source.volume = volumeScale;
        source.Play();

        Destroy(temporaryObject, clip.length / Mathf.Max(Mathf.Abs(pitch), 0.01f) + 0.1f);
    }

    private void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.outputAudioMixerGroup = outputMixerGroup;
    }

    private void OnValidate()
    {
        randomVolumeMultiplier.x = Mathf.Max(0f, randomVolumeMultiplier.x);
        randomVolumeMultiplier.y = Mathf.Max(0f, randomVolumeMultiplier.y);
        randomPitch.x = Mathf.Clamp(randomPitch.x, -3f, 3f);
        randomPitch.y = Mathf.Clamp(randomPitch.y, -3f, 3f);

        if (outputAudioSource != null)
            ConfigureSource(outputAudioSource);
    }
}
