using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class ToolModeLight : MonoBehaviour
{
    [Header("Intensity Range")]
    public float intensity = 1f;

    [Header("Flicker")]
    public float flickerCycle = 0.1f;

    private Light2D light2D;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        light2D.intensity = 0f;
    }

    public void LightFlickerOn(float flickerTime)
    {
        flickerCoroutine = StartCoroutine(LightFlicker(flickerTime));
    }

    IEnumerator LightFlicker(float flickerTime)
    {
        while(flickerTime > 0f)
        {
            yield return new WaitForSeconds(flickerCycle);
            if(light2D.intensity == 0f)
            {
                light2D.intensity = 1f;
            }
            else
            {
                light2D.intensity = 0f;
            }
            flickerTime -= flickerCycle;
        }
    }

    public void LightOn()
    {
        if(flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        light2D.intensity = 1f;
    }

    public void LightOff()
    {
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        light2D.intensity = 0f;
    }
}
