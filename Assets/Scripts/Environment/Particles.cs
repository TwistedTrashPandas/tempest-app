﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Particles : MonoBehaviour
{
    /// Compute Shader for particle updates
    public ComputeShader particlesCS;
    public ComputeShader sortCS;
    public Shader renderParticlesS;

    /// Vector field for tornado
    public VectorField vectorField;
    public GenNoiseTexture noiseTex;

    /// In and out Computer buffers for the shader
    private ComputeBuffer particleVelCB;
    private ComputeBuffer particlePosCB;
    private ComputeBuffer vectorFieldCBIn;

    /// kernel for computeshader
    private int kernelP;
    private int kernelS;
    private int kernelT;

    /// amount of particles
    private uint numberParticles;
    private float maxDist;
    private float[] maxVel;
    private float dampVel;
    /// arrays for particles
    private Vector3[] particlePos;
    private Vector3[] particleVel;
    private Texture2D partTex;
    private int[] particleIdx;

    const uint BLOCK_SIZE = 1024;
    const uint TRANSPOSE_BLOCK_SIZE = 16;

    System.Random rnd = new System.Random();

    void Start()
    {
        numberParticles = (uint)Mathf.Pow(2, 15f);
        maxVel = new float[3];
        rnd = new System.Random();
        particlePos = new Vector3[numberParticles];
        particleVel = new Vector3[numberParticles];
        particleIdx = new int[numberParticles];
        for (int i = 0; i < numberParticles; i++)
        {
            float x = Random.Range(0f, vectorField.GetDimensions()[0] * vectorField.GetCellSize());
            float z = Random.Range(0f, vectorField.GetDimensions()[2] * vectorField.GetCellSize());
            float y = Random.Range(0f, vectorField.GetDimensions()[1] * vectorField.GetCellSize());
            particlePos[i] = new Vector3(x, y, z);
            particleIdx[i] = i;
        }
        initBuffers();
        CreateMesh();
        partTex = noiseTex.GetNoise();
        // GetComponent<Renderer>().material.SetTexture("g_NoiseTex", partTex);
    }

    private void initBuffers()
    {
        //  particlesCB buffers
        particleVelCB = new ComputeBuffer((int)numberParticles, 12);
        particlePosCB = new ComputeBuffer((int)numberParticles, 12);
        vectorFieldCBIn = new ComputeBuffer(vectorField.GetAmountOfElements(), 12);
        //  get corresponding kernel index
        kernelP = particlesCS.FindKernel("UpdateParticles");
        kernelS = sortCS.FindKernel("BitonicSort");
        // kernelT = sortCS.FindKernel("Transpose");
        int[] dims = new int[4];
        //  assume static grid size
        Vector3Int temp = vectorField.GetDimensions();
        dims[0] = temp.x;
        dims[1] = temp.y;
        dims[2] = temp.z;
        dims[3] = Mathf.RoundToInt(vectorField.GetCellSize());
        maxDist = temp.x * vectorField.GetCellSize() * 3f;
        maxVel[0] = dims[3] * 10f;
        maxVel[1] = dims[3] * 2f;
        maxVel[2] = dims[3] * 10f;
        dampVel = 0.999f;
        float[] center = new float[3];
        center[0] = (temp.x - 1) * 0.5f * dims[3];
        center[1] = (temp.y - 1) * 0.5f * dims[3];
        center[2] = (temp.z - 1) * 0.5f * dims[3];

        particlesCS.SetInts("g_i3Dimensions", dims);
        particlesCS.SetFloats("g_vCenter", center);
        particlesCS.SetFloat("g_fDampVel", dampVel);
        particlesCS.SetFloats("g_fMaxVel", maxVel);
        particlesCS.SetFloat("g_fMaxDist", maxDist);

        Shader.SetGlobalFloat("g_fHeightInterp", dims[1] * dims[3] * 0.333f);
        Shader.SetGlobalFloat("g_fMaxHeight", dims[1] * dims[3]);

        //  assume static data for compute buffers
        vectorFieldCBIn.SetData(vectorField.GetVectorField());
        particlePosCB.SetData(particlePos);
        particleVelCB.SetData(particleVel);
        //  assume static vector field
        particlesCS.SetBuffer(kernelP, "vectorFieldIn", vectorFieldCBIn);
        particlesCS.SetBuffer(kernelP, "particlePosRW", particlePosCB);
        particlesCS.SetBuffer(kernelP, "particleVelRW", particleVelCB);
        sortCS.SetBuffer(kernelP, "particlePosRW", particlePosCB);
        sortCS.SetBuffer(kernelP, "particleVelRW", particleVelCB);
    }


    private void UpdateParticles()
    {
        float dt = Time.deltaTime;
        particlesCS.SetFloat("g_fTimestep", dt);
        float[] randPos = new float[3];
        randPos[0] = (rnd.Next(0, 2) * 2 - 1f) * (0.25f + (float)rnd.NextDouble() * 0.5f);
        randPos[1] = rnd.Next(0, 2);
        randPos[2] = (rnd.Next(0, 2) * 2 - 1f) * (0.25f + (float)rnd.NextDouble() * 0.5f);
        particlesCS.SetFloats("g_fRandPos", randPos);

        particlesCS.Dispatch(kernelP, Mathf.CeilToInt(numberParticles / 256f), 1, 1);
        Shader.SetGlobalBuffer("g_vVertices", particlePosCB);
    }

    private void SortParticles()
    {
        float[] camPos = new float[3];
        camPos[0] = Camera.main.transform.position.x;
        camPos[1] = Camera.main.transform.position.y;
        camPos[2] = Camera.main.transform.position.z;
        sortCS.SetFloats("g_vCameraPos", camPos);
        sortCS.SetInt("g_iNumPart", (int)numberParticles);
        /*for (int k = 2; k <= numberParticles ; k *= 2)
        {
            sortCS.SetInt("g_iStage", k);
            sortCS.Dispatch(kernelS, Mathf.CeilToInt(numberParticles / BLOCK_SIZE / 2), 1, 1);
        }*/
        sortCS.Dispatch(kernelS, 1, 1, 1);
        //particlePosCB.GetData(particlePos);
        for (int i = 0; i < numberParticles - 1; i++)
        {
            if (Vector3.Distance(particlePos[i], Camera.main.transform.position) < Vector3.Distance(particlePos[i + 1], Camera.main.transform.position))
            {
                print(i.ToString() + " - not sorted!");
                break;
            }
        }
        /*
        for (uint k = (BLOCK_SIZE << 1); k <= numberParticles; k <<= 1)
        {
            sortCS.SetInt("g_iStage", (int)(k / BLOCK_SIZE));
            sortCS.SetInt("g_iStage_2", (int)(k & ~numberParticles));
            sortCS.SetInt("g_iWidth", (int)BLOCK_SIZE);
            sortCS.SetInt("g_iHeight", (int)(numberParticles/BLOCK_SIZE));
            sortCS.Dispatch(kernelT, (int)(BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE), (int)(numberParticles / BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE), 1);            
            sortCS.Dispatch(kernelS, Mathf.CeilToInt(numberParticles / (float)BLOCK_SIZE), 1, 1);
            
            sortCS.SetInt("g_iStage", (int)BLOCK_SIZE);
            sortCS.SetInt("g_iStage_2", (int)k);
            sortCS.Dispatch(kernelT, (int)(numberParticles / BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE), (int)(BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE), 1);
            sortCS.Dispatch(kernelS, Mathf.CeilToInt(numberParticles / (float)BLOCK_SIZE), 1, 1);
        }
        /*
        particlePosCB.GetData(particlePos);
        for (int i = 0; i < numberParticles / 2; i++)
        {
            if (Vector3.Distance(particlePos[i], Camera.main.transform.position) > Vector3.Distance(particlePos[i + 1], Camera.main.transform.position))
            {
                print(i.ToString() + " - not sorted!");
            }
        }*/
        Shader.SetGlobalBuffer("g_vVertices", particlePosCB);
    }


    public void Update()
    {
        UpdateParticles();
        // SortParticles();
    }

    private void CreateMesh()
    {
        Vector2[] newUV;

        newUV = new Vector2[particlePos.Length];

        //  initialize texture coordinates
        for (int i = 0; i < particlePos.Length; i++)
        {
            newUV[i] = new Vector2(particlePos[i].x, particlePos[i].z);
        }

        //  create new mesh
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.MarkDynamic();
        mesh.vertices = particlePos;
        mesh.SetIndices(particleIdx, MeshTopology.Points, 0);
        mesh.uv = newUV;
        GetComponent<MeshFilter>().mesh = mesh;
        /*
        Texture2D tex;
        tex = new Texture2D((int)numberParticles, 1, TextureFormat.ARGB32, true);
        var cols = new Color[numberParticles];
        int idx = 0;
        Color c = Color.white;
        tex.SetPixels(cols);
        tex.Apply();*/
    }

    void OnApplicationQuit()
    {
        particleVelCB.Release();
        particlePosCB.Release();
        vectorFieldCBIn.Release();
    }
}
