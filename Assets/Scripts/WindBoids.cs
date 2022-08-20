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
    public bool clearTrail;

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
    [Range(0f, 1f)]
    public float fadeSpeed;
    #endregion

    #region Internal Variables
    private Boid[] initialBoids;
    #endregion

    #region Compute Shader Variables
    private RenderTexture outputBoidTexture;
    private ComputeShader boidCompute;
    private int[] computeDim;
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

        public Vector4 color;

        public static int Size {
            get {
                return (sizeof(int))
                    + (sizeof(float) * 2 * 2)
                    + (sizeof(float) * 2 * 3)
                    + (sizeof(int))
                    + (sizeof(float) * 4);
            }
        }
    }

    void OnEnable() {
        #region Load Resources
        boidCompute = Resources.Load<ComputeShader>("WindBoidsCompute");
        clearTextureCompute = Resources.Load<ComputeShader>("ClearTexture");
        #endregion
    }

    void Start() {
        outputBoidTexture = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 0);
        outputBoidTexture.filterMode = FilterMode.Point;
        outputBoidTexture.enableRandomWrite = true;
        outputBoidTexture.Create();

        outputImage.texture = outputBoidTexture;

        //Perform initial setup
        SetUpBoids();
        computeDim = new int[2] { outputTextureSize.x, outputTextureSize.y };
    }

    //Set position, random direction, random colour
    void SetUpBoids() {
        initialBoids = new Boid[numberOfBoids];

        for (int i = 0; i < numberOfBoids; i++) {
            initialBoids[i] = new Boid {
                group = 0,
                position = new Vector2(Random.Range(0f, outputTextureSize.x), Random.Range(0f, outputTextureSize.y)),
                velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * ((maxSpeed - minSpeed) / 2),
                color = Color.green
            };
        }

        var boidBuffer = new ComputeBuffer(numberOfBoids, Boid.Size);
        boidBuffer.SetData(initialBoids);

        //Set to global buffer so it stays on GPU
        Shader.SetGlobalBuffer("_BoidData", boidBuffer);

        //R
        initialBoids = null;
    }

    void FixedUpdate() {
        //Clear agent map
        if (clearTrail) ClearTexture(outputBoidTexture);

        //Setup compute variables
        int numOfBatches = Mathf.CeilToInt(numberOfBoids / 8);

        //Set compute variables
        boidCompute.SetTexture(0, "_BoidMap", outputBoidTexture);
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


        //Dispatch
        boidCompute.Dispatch(0, numOfBatches, 1, 1);

    }


    void ClearTexture(RenderTexture rt) {
        int numBatchX = Mathf.CeilToInt(rt.width / 8);
        int numBatchY = Mathf.CeilToInt(rt.height / 8);

        clearTextureCompute.SetTexture(0, "_Texture", rt);
        clearTextureCompute.SetFloat("width", rt.width);
        clearTextureCompute.SetFloat("height", rt.height);

        clearTextureCompute.Dispatch(0, numBatchX, numBatchY, 1);
    }

}
