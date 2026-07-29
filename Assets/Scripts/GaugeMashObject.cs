using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static DropItem;

public enum GaugeVisualMode
{
    Fill,
    SpriteSwap
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
    private readonly Sprite[] progressSprites;

    public SpriteSwapGaugeVisualStrategy(Image image, Sprite[] progressSprites)
    {
        this.image = image;
        this.progressSprites = progressSprites;
    }

    public void SetProgress(float normalizedProgress)
    {
        if (image == null || progressSprites == null || progressSprites.Length == 0)
            return;

        // A prefab configured for Filled images could otherwise hide swapped sprites.
        image.fillAmount = 1f;
        int spriteIndex = Mathf.RoundToInt(
            Mathf.Clamp01(normalizedProgress) * (progressSprites.Length - 1));
        image.sprite = progressSprites[spriteIndex];
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

    [Tooltip("SpriteSwap 모드에서 0%부터 100% 순서로 사용할 스프라이트")]
    public Sprite[] progressSprites;

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
                return new SpriteSwapGaugeVisualStrategy(targetImage, progressSprites);
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
