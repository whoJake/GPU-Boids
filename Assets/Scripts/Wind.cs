using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Wind : MonoBehaviour
{
    [Header("Texture Settings")]
    public Vector2Int textureSize;

    [Header("Simulator Settings")]
    public float windSpeed;
    public float frequency;
    public float amplitude;
    public Vector2 period;
    public float turbulencePower;
    public float turbulenceSize;

    [Header("Debug")]
    public RenderTexture windTexture;
    public RawImage canvas;

    public ComputeShader windGeneratorShader;

    void OnEnable() {
        windTexture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        windTexture.enableRandomWrite = true;
        windTexture.Create();
        canvas.texture = windTexture;
    }

    void Update()
    {
        GenerateWindTexture();
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
