using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class ToolModeManager : MonoBehaviour, IModeManager
{
    private bool modeOn = false;
    private bool lightOn = false;

    [SerializeField] private ToolModeLight[] toolModeLights;
    [SerializeField] private GameObject devilObject;
    private GameObject devilInstance;
    private LightPatrolDevilAI devilAI;

    [Header("Cycle (mode = Cycle")]
    [SerializeField] private float flickerDurationMin = 0.5f;
    [SerializeField] private float flickerDurationMax = 1f;
    [SerializeField] private float onDurationMin = 0.5f;
    [SerializeField] private float onDurationMax = 1.5f;
    [SerializeField] private float offDurationMin = 0.5f;
    [SerializeField] private float offDurationMax = 1.5f;

    

    private void Awake()
    {
        toolModeLights = GetComponentsInChildren<ToolModeLight>();
    }

    public void ModeOn()
    {
        if (modeOn == false)
        {
            modeOn = true;

            if (devilObject != null)
            {
                if(devilInstance == null)
                {
                    devilInstance = Instantiate(devilObject, transform, true);
                    devilInstance.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                    if (devilInstance)
                    {
                        devilAI = devilInstance.GetComponent<LightPatrolDevilAI>();

                        if(devilAI)
                        {
                            List<Transform> lights = new List<Transform>();
                            foreach(ToolModeLight toolModeLight in toolModeLights)
                            {
                                lights.Add(toolModeLight.gameObject.transform);
                            }
                            devilAI.SetLightWaypoints(lights);
                        }
                    }
                }
            }

            LightFlickerOn();
        }
    }

    private void LightFlickerOn()
    {
        if (toolModeLights == null || toolModeLights.Length == 0)
            return;

        float flickerTime = Random.Range(flickerDurationMin, flickerDurationMax);
        foreach (ToolModeLight light in toolModeLights)
        {
            light.LightFlickerOn(flickerTime);
        }

        StartCoroutine(LightOnTimer(flickerTime));
        lightOn = true;
    }

    IEnumerator LightOnTimer(float flickerTime)
    {
        yield return new WaitForSeconds(flickerTime);

        if (devilAI)
            devilAI.SetPatrolLights();

        foreach(ToolModeLight light in toolModeLights)
        {
            light.LightOn();
        }

        StartCoroutine(LightOn());
    }

    IEnumerator LightOn()
    {
        float lightOnTime = Random.Range(onDurationMin, onDurationMax);
        lightOn = true;
        yield return new WaitForSeconds(lightOnTime);

        LightFlickerOff();
    }

    private void LightFlickerOff()
    {
        float flickerTime = Random.Range(flickerDurationMin, flickerDurationMax);
        foreach (ToolModeLight light in toolModeLights)
        {
            light.LightFlickerOn(flickerTime);
        }

        StartCoroutine(LightOffTimer(flickerTime));
    }

    IEnumerator LightOffTimer(float flickerTime)
    {
        yield return new WaitForSeconds(flickerTime);

        if (devilAI)
            devilAI.SetInvestigate();

        foreach (ToolModeLight light in toolModeLights)
        {
            light.LightOff();
        }
        StartCoroutine(LightOff());
    }

    IEnumerator LightOff()
    {
        float lightOffTime = Random.Range(offDurationMin, offDurationMax);
        lightOn = false;
        yield return new WaitForSeconds(lightOffTime);

        if(modeOn == true)
        {
            LightFlickerOn();
        }
    }

    public void ModeOff()
    {
        if(modeOn == true)
        {
            modeOn = false;

            if(lightOn)
            {
                foreach(ToolModeLight light in toolModeLights)
                {
                    light.LightOff();
                }
            }
            if(devilInstance)
            {
                Destroy(devilInstance);
            }
        }
    }
}
