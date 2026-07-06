using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public enum FlickerMode { Natural, WarningLight}

    [Header("Mode")]
    public FlickerMode mode = FlickerMode.Natural;

    [Header("Intensity Range")]
    public float minIntensity = 0.6f;
    public float maxIntensity = 1.2f;

    [Header("Flicker")]
    [Tooltip("Perlin 노이즈 샘플링 속도. 높을수록 빠르게 깜빡임")]
    public float flickerSpeed = 8f;

    [Header("Spike (순간 꺼짐)")]
    [Tooltip("활성화 시 무작위로 빛이 순간적으로 꺼졌다 켜짐")]
    public bool enableSpike = false;
    [Tooltip("초당 스파이크 발생 확률 (0~1)")]
    [Range(0f, 1f)]
    public float spikeChance = 0.03f;
    [Tooltip("스파이크 지속 시간(초)")]
    public float spikeDuration = 0.05f;

    [Header("Event")]
    public GameEvent startEvent;

    private Light2D light2D;
    public float maxNoiseOffset = 100f;
    public float minNoiseOffset = 0f;
    private float noiseOffset;
    private float spikeTimer = 0f;
    private bool isSpiking = false;
    private bool isActive = true;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        noiseOffset = Random.Range(minNoiseOffset, maxNoiseOffset);
        light2D.intensity = 0f;
    }

    private void OnEnable()
    {
        if (startEvent != null)
        {
            GameEventManager.Subscribe(startEvent, StartFlicker);
            isActive = SaveManager.HasEventFired(startEvent.name);
        }
    }

    private void OnDisable()
    {
        if (startEvent != null)
            GameEventManager.Unsubscribe(startEvent, StartFlicker);
    }

    private void StartFlicker()
    {
        if (startEvent != null)
            SaveManager.MarkEventFired(startEvent.name);

        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;

        if (isSpiking)
        {
            spikeTimer -= Time.deltaTime;
            if (spikeTimer <= 0f)
                isSpiking = false;

            light2D.intensity = 0f;
            return;
        }

        if (enableSpike && Random.value < spikeChance * Time.deltaTime)
        {
            isSpiking = true;
            spikeTimer = spikeDuration;
            return;
        }

        if(mode == FlickerMode.Natural)
        {
            float noise = Mathf.PerlinNoise(noiseOffset + Time.time * flickerSpeed, 0f);
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }
        else if(mode == FlickerMode.WarningLight)
        {
            float sinValue = Mathf.Sin(Time.time * flickerSpeed) > 0 ? 1f : 0f;
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, sinValue);
        }
    }
}
