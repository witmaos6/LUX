using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static DropItem;

public enum GaugeVisualMode
{
    Fill,
    SpriteSwap
}

[System.Serializable]
public sealed class GaugeSpriteSoundStep
{
    [Tooltip("이 단계에서 표시할 스프라이트")]
    public Sprite sprite;

    [Tooltip("이 단계로 변경될 때 재생할 효과음. 비어 있으면 재생하지 않습니다.")]
    public AudioClip sound;
}

internal interface IGaugeVisualStrategy
{
    void SetProgress(float normalizedProgress);
}

internal sealed class FillGaugeVisualStrategy : IGaugeVisualStrategy
{
    private readonly Image image;

    public FillGaugeVisualStrategy(Image image)
    {
        this.image = image;
    }

    public void SetProgress(float normalizedProgress)
    {
        if (image != null)
            image.fillAmount = Mathf.Clamp01(normalizedProgress);
    }
}

internal sealed class SpriteSwapGaugeVisualStrategy : IGaugeVisualStrategy
{
    private readonly Image image;
    private readonly GaugeSpriteSoundStep[] progressSteps;
    private readonly Sprite[] legacyProgressSprites;
    private readonly AudioSource audioSource;
    private readonly float soundVolume;
    private int currentSpriteIndex = -1;

    public SpriteSwapGaugeVisualStrategy(
        Image image,
        GaugeSpriteSoundStep[] progressSteps,
        Sprite[] legacyProgressSprites,
        AudioSource audioSource,
        float soundVolume)
    {
        this.image = image;
        this.progressSteps = progressSteps;
        this.legacyProgressSprites = legacyProgressSprites;
        this.audioSource = audioSource;
        this.soundVolume = soundVolume;
    }

    public void SetProgress(float normalizedProgress)
    {
        int stepCount = GetStepCount();
        if (image == null || stepCount == 0)
            return;

        // A prefab configured for Filled images could otherwise hide swapped sprites.
        image.fillAmount = 1f;
        int spriteIndex = Mathf.RoundToInt(
            Mathf.Clamp01(normalizedProgress) * (stepCount - 1));

        bool shouldPlaySound = currentSpriteIndex >= 0 && currentSpriteIndex != spriteIndex;
        AudioClip stepSound = null;

        if (progressSteps != null && progressSteps.Length > 0)
        {
            GaugeSpriteSoundStep step = progressSteps[spriteIndex];
            image.sprite = step != null ? step.sprite : null;
            stepSound = step != null ? step.sound : null;
        }
        else
        {
            image.sprite = legacyProgressSprites[spriteIndex];
        }

        currentSpriteIndex = spriteIndex;

        if (shouldPlaySound && audioSource != null && stepSound != null)
            audioSource.PlayOneShot(stepSound, soundVolume);
    }

    private int GetStepCount()
    {
        if (progressSteps != null && progressSteps.Length > 0)
            return progressSteps.Length;

        return legacyProgressSprites != null ? legacyProgressSprites.Length : 0;
    }
}

public class GaugeMashObject : InteractionObject
{
    public GameObject gaugeUIPrefab;
    public Transform canvasTransform;

    [Header("Gauge Visual")]
    [Tooltip("게이지 진행도를 표시하는 방식")]
    public GaugeVisualMode visualMode = GaugeVisualMode.Fill;

    [Tooltip("gaugeUIPrefab 안에서 게이지 Fill Image를 담고 있는 자식 오브젝트 이름")]
    public string fillImageName = "Fill";

    [Tooltip("SpriteSwap 모드에서 0%부터 100% 순서로 사용할 스프라이트와 효과음 세트")]
    public GaugeSpriteSoundStep[] progressSteps;

    [HideInInspector]
    public Sprite[] progressSprites;

    [Tooltip("스프라이트가 변경될 때 효과음을 재생할 AudioSource. 비어 있으면 현재 오브젝트에서 찾습니다.")]
    public AudioSource spriteSwapAudioSource;

    [Range(0f, 1f)]
    public float spriteSwapSoundVolume = 1f;

    [Header("Switch Visual")]
    [Tooltip("gaugeUIPrefab 안에서 스위치 Image를 가진 자식 오브젝트 이름")]
    public string switchImageName = "Switch";

    [Tooltip("스위치를 누르지 않은 상태의 스프라이트")]
    public Sprite switchReleasedSprite;

    [Tooltip("InputPressed 호출 시 표시할 스프라이트")]
    public Sprite switchPressedSprite;

    [Min(0f)]
    [Tooltip("눌린 스프라이트를 유지할 시간(초)")]
    public float switchPressedDuration = 0.1f;

    public float maxGauge = 100f;
    public float gaugePerPress = 10f;

    public GameEvent gaugeCompleteEvent;

    private GameObject gaugeUIInstance;
    private Image gaugeFillImage;
    private IGaugeVisualStrategy gaugeVisualStrategy;
    private Image switchImage;
    private Coroutine switchReleaseRoutine;
    private float currentGauge = 0f;

    public ItemCode itemCode = ItemCode.None;
    public GameEvent unlockEvent;
    public GameEvent failedEvent;
    public GameObject failedUIPrefab;
    private GameObject failedUIInstance;

    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if (spriteSwapAudioSource == null)
            spriteSwapAudioSource = GetComponent<AudioSource>();

        if (canvasTransform == null)
            canvasTransform = GameObject.Find("Canvas").transform;
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        PlayerController playerController = tryObject.GetComponent<PlayerController>();
        if(itemCode != ItemCode.None && !playerController.ExistItem(itemCode))
        {
            GameEventManager.Raise(failedEvent);
            failedUIInstance = Instantiate(failedUIPrefab, canvasTransform);
            return;
        }

        if (itemCode != ItemCode.None && playerController.ExistItem(itemCode) && unlockEvent != null)
        {
            GameEventManager.Raise(unlockEvent);
        }

        if(currentGauge >= maxGauge)
        {
            EndInteraction();
            return;
        }

        currentGauge = 0f;

        if (gaugeUIPrefab != null)
        {
            gaugeUIInstance = Instantiate(gaugeUIPrefab, canvasTransform);

            Transform fillTransform = gaugeUIInstance.transform.Find(fillImageName);
            gaugeFillImage = fillTransform != null ? fillTransform.GetComponent<Image>() : null;
            gaugeVisualStrategy = CreateVisualStrategy(gaugeFillImage);

            Transform switchTransform = gaugeUIInstance.transform.Find(switchImageName);
            switchImage = switchTransform != null ? switchTransform.GetComponent<Image>() : null;
            SetSwitchSprite(switchReleasedSprite);
        }

        UpdateGaugeUI();
    }

    public override void InputPressed()
    {
        if(failedUIInstance != null)
        {
            Destroy(failedUIInstance);
            EndInteraction();
            return;
        }

        ShowSwitchPressed();
        currentGauge += gaugePerPress;

        if (currentGauge >= maxGauge)
        {
            currentGauge = maxGauge;
            UpdateGaugeUI();
            CompleteGauge();
        }
        else
        {
            UpdateGaugeUI();
        }
    }

    private void CompleteGauge()
    {
        StopSwitchReleaseRoutine();

        if (gaugeUIInstance != null)
        {
            float destroyDelay = switchImage != null && switchPressedSprite != null
                ? switchPressedDuration
                : 0f;
            Destroy(gaugeUIInstance, destroyDelay);
        }

        gaugeVisualStrategy = null;
        gaugeFillImage = null;
        switchImage = null;

        if (gaugeCompleteEvent != null)
            GameEventManager.Raise(gaugeCompleteEvent);

        EndInteraction();
    }

    private void UpdateGaugeUI()
    {
        float normalizedProgress = maxGauge > 0f
            ? currentGauge / maxGauge
            : 1f;

        gaugeVisualStrategy?.SetProgress(normalizedProgress);
    }

    private void ShowSwitchPressed()
    {
        if (switchImage == null || switchPressedSprite == null)
            return;

        StopSwitchReleaseRoutine();
        SetSwitchSprite(switchPressedSprite);

        if (switchPressedDuration <= 0f)
        {
            SetSwitchSprite(switchReleasedSprite);
            return;
        }

        switchReleaseRoutine = StartCoroutine(ReleaseSwitchAfterDelay());
    }

    private IEnumerator ReleaseSwitchAfterDelay()
    {
        yield return new WaitForSecondsRealtime(switchPressedDuration);
        SetSwitchSprite(switchReleasedSprite);
        switchReleaseRoutine = null;
    }

    private void SetSwitchSprite(Sprite sprite)
    {
        if (switchImage != null && sprite != null)
            switchImage.sprite = sprite;
    }

    private void StopSwitchReleaseRoutine()
    {
        if (switchReleaseRoutine == null)
            return;

        StopCoroutine(switchReleaseRoutine);
        switchReleaseRoutine = null;
    }

    private IGaugeVisualStrategy CreateVisualStrategy(Image targetImage)
    {
        switch (visualMode)
        {
            case GaugeVisualMode.SpriteSwap:
                return new SpriteSwapGaugeVisualStrategy(
                    targetImage,
                    progressSteps,
                    progressSprites,
                    spriteSwapAudioSource,
                    spriteSwapSoundVolume);
            case GaugeVisualMode.Fill:
            default:
                return new FillGaugeVisualStrategy(targetImage);
        }
    }

    public override void CancelInteraction()
    {
        StopSwitchReleaseRoutine();

        if (gaugeUIInstance != null)
            Destroy(gaugeUIInstance);

        if (failedUIInstance != null)
            Destroy(failedUIInstance);

        gaugeVisualStrategy = null;
        gaugeFillImage = null;
        switchImage = null;

        base.CancelInteraction();
    }
}
