using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindBoids : MonoBehaviour
{

    #region Editor Visible Variables
    [Header("Output Settings")]
    public Vector2Int outputTextureSize;
    public RawImage outputImage;

    [Header("Boid Settings")]
    [Min(1)]
    public int numberOfBoids;
    public float speed = 1f;
    #endregion

    #region Internal Variables
    private Boid[] boidHandler;
    #endregion

    #region Compute Shader Variables
    private RenderTexture outputTexture;
    private ComputeShader boidCompute;
    private int[] computeDim;
    #endregion

    struct Boid {
        public Vector2 position;
        public Vector2 direction;

        public Vector4 color;

        public static int Size {
            get {
                return (sizeof(float) * 2 * 2) + (sizeof(float) * 4);
            }
        }
    }

    void OnEnable() {
        #region Load Resources
        boidCompute = Resources.Load<ComputeShader>("WindBoidsCompute");
        #endregion
    }

    void Start() {
        outputTexture = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 0);
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        outputImage.texture = outputTexture;

        //Perform initial setup
        SetUpBoids();
        computeDim = new int[2] { outputTextureSize.x, outputTextureSize.y };
    }

    //Set position, random direction, random colour
    void SetUpBoids() {
        boidHandler = new Boid[numberOfBoids];
        for (int i = 0; i < numberOfBoids; i++) {
            boidHandler[i] = new Boid {
                position = new Vector2(50, 50),
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
                color = Random.ColorHSV()
            };
        }
    }

    void Update() {
        //Setup compute variables
        var boidBuffer = new ComputeBuffer(numberOfBoids, Boid.Size);
        boidBuffer.SetData(boidHandler);
        int numOfBatches = Mathf.CeilToInt(numberOfBoids / 8);

        //Set compute variables
        boidCompute.SetTexture(0, "_WindTexture", outputTexture);
        boidCompute.SetBuffer(0, "_BoidData", boidBuffer);
        boidCompute.SetFloat("_Speed", speed);
        boidCompute.SetInts("_TextureDimensions", computeDim);

        //Dispatch
        boidCompute.Dispatch(0, numOfBatches, 1, 1);

        //Read data and release buffer
        boidBuffer.GetData(boidHandler);
        boidBuffer.Release();
    }

}
