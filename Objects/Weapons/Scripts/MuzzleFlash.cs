﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Light lightFlash;
    private float defaultIntensity;
    private float shownAt = 0;
    private float showTime = 0.05f;
    private bool shownOnce = false; // whether its been shown for at least one frame

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        lightFlash = GetComponent<Light>();
        if (lightFlash)
             defaultIntensity = lightFlash.intensity;
    }

    public void Flash() {
        shownAt = Time.time;
        transform.localScale = new Vector3(Random.Range(0.7f, 1.1f), Random.Range(0.7f, 1.1f), Random.Range(0.7f, 1.1f));
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f) + ((int)Random.Range(0, 4) * 90f));
        meshRenderer.enabled = true;
        if (lightFlash) {
            lightFlash.intensity = defaultIntensity;
            lightFlash.enabled = true;
        }
        shownOnce = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (shownOnce) { 
            if (Time.time > shownAt + showTime) {
                meshRenderer.enabled = false;
                lightFlash.enabled = false;
            } else {
                float scale = 1f/showTime * Time.deltaTime;
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scale);
                lightFlash.intensity = Mathf.Lerp(lightFlash.intensity, 0, scale);
            }
        }
        shownOnce = true;
    }
}
