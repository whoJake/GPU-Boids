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
    [Range(0, 5)]
    public int drawRadius;

    [Header("Boid Settings")]
    [Min(1)]
    public int numberOfBoids = 1;
    [Min(0)]
    public float minSpeed = 1f;
    [Min(0)]
    public float maxSpeed = 2f;
    [Range(0, 2)]
    public float boidSpeedDial = 1f;
    [Min(1)]
    public float detectionDistance = 1f;
    [Min(1)]
    public float avoidDistance = 1f;
    [Range(0, 360)]
    public float detectionAngle;
    [Range(0, 1)]
    public float alignmentWeight;
    [Range(0, 1)]
    public float cohesionWeight;
    [Range(0, 1)]
    public float seperationWeight;
    [Min(0)]
    public float maxSteerForce;

    [Header("Processing Settings")]
    [Range(1f, 10f)]
    public float fadeSpeed = 1f;
    #endregion

    #region Internal Variables
    private Boid[] boidHandler;
    #endregion

    #region Compute Shader Variables
    private ComputeShader boidCompute;
    private int[] computeDim;

    private ComputeShader windBuilder;
    private RenderTexture windTexture;
    #endregion

    #region Compute Helpers
    private ComputeShader clearTextureCompute;
    #endregion

    struct Boid {
        public uint group;

        public Vector2 position;
        public Vector2 velocity;

        public Vector2 flockHeading;
        public Vector2 flockCentre;
        public Vector2 seperationHeading;

        public int numFlockmates;

        public static int Size {
            get {
                return (sizeof(int))
                    + (sizeof(float) * 2 * 2)
                    + (sizeof(float) * 2 * 3)
                    + (sizeof(int));
            }
        }
    }

    void OnEnable() {
        #region Load Resources
        boidCompute = Resources.Load<ComputeShader>("WindBoidsCompute");
        windBuilder = Resources.Load<ComputeShader>("WindBuilder");
        clearTextureCompute = Resources.Load<ComputeShader>("ClearTexture");
        #endregion
    }

    void Start() {
        windTexture = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 0);
        windTexture.filterMode = FilterMode.Point;
        windTexture.enableRandomWrite = true;
        windTexture.Create();

        outputImage.texture = windTexture;

        //Perform initial setup
        SetUpBoids();
        computeDim = new int[2] { outputTextureSize.x, outputTextureSize.y };
    }

    //Set position, random direction, random colour
    void SetUpBoids() {
        boidHandler = new Boid[numberOfBoids];

        for (int i = 0; i < numberOfBoids; i++) {
            boidHandler[i] = new Boid {
                group = 0,
                position = new Vector2(Random.Range(0f, outputTextureSize.x), Random.Range(0f, outputTextureSize.y)),
                velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * ((maxSpeed - minSpeed) / 2)
            };
        }
    }

    void FixedUpdate() {

        RenderTexture lastFrame = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 0);
        lastFrame.Create();
        Graphics.CopyTexture(windTexture, lastFrame);

        //Clear agent map
        ClearTexture(windTexture);


        //Setup compute variables
        int numOfBatches = Mathf.CeilToInt(numberOfBoids / 8);


        var boidBuffer = new ComputeBuffer(numberOfBoids, Boid.Size);
        boidBuffer.SetData(boidHandler);

        //Set compute variables
        boidCompute.SetBuffer(0, "_BoidData", boidBuffer);
        boidCompute.SetInts("_TextureDimensions", computeDim);
        boidCompute.SetInt("_BoidCount", numberOfBoids);
        boidCompute.SetFloat("_DetectionDistance", detectionDistance);
        boidCompute.SetFloat("_AvoidDistance", avoidDistance);
        boidCompute.SetFloat("_DetectionAngle", detectionAngle * Mathf.Deg2Rad);

        boidCompute.SetFloat("_Speed", boidSpeedDial);
        boidCompute.SetFloat("_MinSpeed", minSpeed);
        boidCompute.SetFloat("_MaxSpeed", maxSpeed);
        boidCompute.SetFloat("_MaxSteerForce", maxSteerForce);

        boidCompute.SetFloat("_AlignmentWeight", alignmentWeight);
        boidCompute.SetFloat("_CohesionWeight", cohesionWeight);
        boidCompute.SetFloat("_SeperationWeight", seperationWeight);

        boidCompute.Dispatch(0, numOfBatches, 1, 1);
        boidBuffer.GetData(boidHandler);
        boidBuffer.Release();

        var outputBoids = new ComputeBuffer(numberOfBoids, Boid.Size);
        outputBoids.SetData(boidHandler);

        //Draw and build wind
        windBuilder.SetTexture(0, "_WindOutput", windTexture);
        windBuilder.SetTexture(0, "_LastFrame", lastFrame);
        windBuilder.SetBuffer(0, "_BoidInput", outputBoids);
        windBuilder.SetFloat("_MaxMagnitude", maxSpeed);
        windBuilder.SetFloat("_MinMagnitude", minSpeed);
        windBuilder.SetInts("_TextureDimensions", computeDim);
        windBuilder.SetFloat("_DrawRadius", (float)drawRadius);


        windBuilder.Dispatch(0, numOfBatches, 1, 1);
        outputBoids.Release();

        lastFrame.Release();
    }


    void ClearTexture(RenderTexture rt) {
        int numBatchX = Mathf.CeilToInt(rt.width / 8);
        int numBatchY = Mathf.CeilToInt(rt.height / 8);

        clearTextureCompute.SetTexture(0, "_Texture", rt);
        clearTextureCompute.SetFloat("width", rt.width);
        clearTextureCompute.SetFloat("height", rt.height);
        clearTextureCompute.SetFloat("_FadeSpeed", fadeSpeed);

        clearTextureCompute.Dispatch(0, numBatchX, numBatchY, 1);
    }

}