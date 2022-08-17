using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Wind : MonoBehaviour
{
    [Header("Texture Settings")]
    public Vector2Int textureSize;

    [Header("Simulator Settings")]
    public WindSettings baseSettings;
    public WindSettings secondarySettings;

    public bool autoUpdateBaseSettings = false;

    [Header("Internal Settings (DO NOT TOUCH)")]
    private float windSpeed;
    private float frequency;
    private float amplitude;
    private Vector2 period;
    private float turbulencePower;
    private float turbulenceSize;
    private bool isBlending;

    [Header("Debug")]
    public RenderTexture windTexture;
    public RawImage canvas;

    public ComputeShader windGeneratorShader;

    [Header("Blend Settings")]
    [Min(0f)]
    public float blendTime;
    [Min(1)]
    public int steps;

    void OnEnable() {
        windTexture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        windTexture.enableRandomWrite = true;
        windTexture.Create();
        canvas.texture = windTexture;
        SetSettings(baseSettings);
    }

    void Update()
    {
        if (autoUpdateBaseSettings && !isBlending) SetSettings(baseSettings);
        GenerateWindTexture();
    }

    public void SetSettings(WindSettings i) {
        windSpeed = i.windSpeed;
        frequency = i.frequency;
        amplitude = i.amplitude;
        period = i.period;
        turbulencePower = i.turbulencePower;
        turbulenceSize = i.turbulenceSize;
    }

    public void StartBlend() {
        //Check if not already blending
        if (!isBlending) {
            StartCoroutine(BlendInto(secondarySettings, blendTime, steps));
        }
    }

    IEnumerator BlendInto(WindSettings b, float blendTime, float steps) {
        isBlending = true;
        
        for (int i = 1; i <= steps; i++) {
            windSpeed = Mathf.Lerp(windSpeed, b.windSpeed, i / steps);
            frequency = Mathf.Lerp(frequency, b.frequency, i / steps);
            amplitude = Mathf.Lerp(amplitude, b.amplitude, i / steps);
            period.x = Mathf.Lerp(period.x, b.period.x, i / steps);
            period.y = Mathf.Lerp(period.y, b.period.y, i / steps);
            turbulencePower = Mathf.Lerp(turbulencePower, b.turbulencePower, i / steps);
            turbulenceSize = Mathf.Lerp(turbulenceSize, b.turbulenceSize, i / steps);

            yield return new WaitForSeconds(blendTime / steps);
        }

        //Swap the two settings
        SwapSettings();
        isBlending = false;
        yield break;
    }

    public void SwapSettings() {
        WindSettings transf = baseSettings;
        baseSettings = secondarySettings;
        secondarySettings = transf;
    }

    void GenerateWindTexture() {
        windGeneratorShader.SetTexture(0, "_WindMap", windTexture);
        windGeneratorShader.SetFloat("_Time", Time.time * windSpeed);
        windGeneratorShader.SetFloat("_Frequency", frequency);
        windGeneratorShader.SetFloat("_Amplitude", amplitude);
        windGeneratorShader.SetFloat("_PeriodX", period.x);
        windGeneratorShader.SetFloat("_PeriodY", period.y);
        windGeneratorShader.SetFloat("_TurbulencePower", turbulencePower);
        windGeneratorShader.SetFloat("_TurbulenceSize", turbulenceSize);

        windGeneratorShader.Dispatch(0, Mathf.CeilToInt(textureSize.x / 8f), Mathf.CeilToInt(textureSize.y / 8f), 1);
    }
}

//Editor
[CustomEditor(typeof(Wind))]
public class WindEditor : Editor {
    public override void OnInspectorGUI() {
        Wind wind = (Wind)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Blend")) {
            wind.StartBlend();
        }
        if (GUILayout.Button("Swap")) {
            wind.SwapSettings();
        }
    }
}
