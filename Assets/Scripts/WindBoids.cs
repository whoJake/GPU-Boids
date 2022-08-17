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

        public static int Size {
            get {
                return sizeof(float) * 2 * 2;
            }
        }
    }

    void OnEnable() {
        if(!outputTexture.IsCreated() || outputTexture == default || outputTexture == null) {
            outputTexture = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 24);
        }
        boidHandler = new Boid[numberOfBoids];

        #region Load Resources
        boidCompute = Resources.Load<ComputeShader>("WindBoidsCompute");
        #endregion

        outputImage.texture = outputTexture;

        computeDim = new int[2] { outputTextureSize.x, outputTextureSize.y };

        //Perform initial setup
        SetUpBoids();
    }

    void SetUpBoids() {
        for(int i = 0; i < numberOfBoids; i++) {
            boidHandler[i] = new Boid {
                position = new Vector2(100, 100),
                direction = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f))
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
        boidCompute.SetFloat("_Speed", 1f);
        boidCompute.SetInts("_TextureDimensions", computeDim);

        //Start compute
        boidCompute.Dispatch(0, numOfBatches, 1, 1);

        //Release buffers
        boidBuffer.Dispose();
    }

}
