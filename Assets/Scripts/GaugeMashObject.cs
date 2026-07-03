using UnityEngine;
using UnityEngine.UI;

public class GaugeMashObject : InteractionObject
{
    public GameObject gaugeUIPrefab;
    public Transform canvasTransform;

    [Tooltip("gaugeUIPrefab 안에서 게이지 Fill Image를 담고 있는 자식 오브젝트 이름")]
    public string fillImageName = "Fill";

    public float maxGauge = 100f;
    public float gaugePerPress = 10f;

    public GameEvent gaugeCompleteEvent;

    private GameObject gaugeUIInstance;
    private Image gaugeFillImage;
    private float currentGauge = 0f;

    private void Awake()
    {
        interactionType = InteractionType.InteractionKey;

        if (canvasTransform == null)
            canvasTransform = GameObject.Find("Canvas").transform;
    }

    public override void ActivateInteraction(GameObject tryObject)
    {
        currentGauge = 0f;

        if (gaugeUIPrefab != null)
        {
            gaugeUIInstance = Instantiate(gaugeUIPrefab, canvasTransform);

            // 루트 오브젝트의 Image는 배경용이므로, 게이지용 Image는 이름으로 지정된 자식에서 찾는다.
            Transform fillTransform = gaugeUIInstance.transform.Find(fillImageName);
            gaugeFillImage = fillTransform != null ? fillTransform.GetComponent<Image>() : null;
        }

        UpdateGaugeUI();
    }

    public override void InputPressed()
    {
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

        if (gaugeCompleteEvent != null)
            GameEventManager.Raise(gaugeCompleteEvent);

        EndInteraction();
    }

    private void UpdateGaugeUI()
    {
        if (gaugeFillImage != null)
            gaugeFillImage.fillAmount = currentGauge / maxGauge;
    }
}
