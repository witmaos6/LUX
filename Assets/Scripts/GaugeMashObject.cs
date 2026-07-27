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

    public float maxGauge = 100f;
    public float gaugePerPress = 10f;

    public GameEvent gaugeCompleteEvent;

    private GameObject gaugeUIInstance;
    private Image gaugeFillImage;
    private IGaugeVisualStrategy gaugeVisualStrategy;
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
        if (gaugeUIInstance != null)
            Destroy(gaugeUIInstance);

        gaugeVisualStrategy = null;
        gaugeFillImage = null;

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
        if (gaugeUIInstance != null)
            Destroy(gaugeUIInstance);

        if (failedUIInstance != null)
            Destroy(failedUIInstance);

        gaugeVisualStrategy = null;
        gaugeFillImage = null;

        base.CancelInteraction();
    }
}
