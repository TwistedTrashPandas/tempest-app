using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest;

namespace MastersOfTempest.Environment.VisualEffects
{
    using Tools;
    // particle system for the tornado, uses vectorfield as the movement of particles
    public class TornadoPS : MonoBehaviour
    {
        /// Compute Shader for particle updates
        public ComputeShader particlesCS;
        public ComputeShader sortCS;
        public ComputeShader winAnimation;
        public Shader renderParticlesS;

        /// Vector field for tornado
        public VectorField vectorField;
        public Transform camPos;


        /// sort all particles with respect to the camera position each "sortEach" timestep
        [Range(1, 100)]
        public int sortEach;
        [Range(0, 20)]
        public uint particelNumExp;
        [Range(0f, 1f)]
        public float dampVel;

        [Range(0, 11)]
        public int numCloudSkyParticles;

        public float[] maxVel;

        /// In and out Computer buffers for the shader
        private ComputeBuffer particleVelCB;
        private ComputeBuffer particlePosCB;
        private ComputeBuffer particleinitialPosCB;
        private ComputeBuffer indicesCB;
        private ComputeBuffer indicesRCB;
        private ComputeBuffer vectorFieldCBIn;
        private ComputeBuffer particleVisibilityCB;

        private ComputeBuffer argsBuffer1;
        private ComputeBuffer argsBuffer2;
        private ComputeBuffer argsBuffer3;
        private ComputeBuffer argsBuffer4;

        /// kernel for computeshader
        private int kernelP;
        private int kernelS;
        private int kernelT;

        private int kernelWA;

        /// amount of particles
        private uint numberParticles;
        private float maxDist;
        /// arrays for particles
        private Vector3[] particlePos;
        private Vector3[] particleVel;
        private int[] particleIdx;

        private Material material;

        // counter for sorting particles
        private int counter;
        private bool targetShip;
        private GameObject ship;

        const uint BLOCK_SIZE = 256;
        const uint TRANSPOSE_BLOCK_SIZE = 16;

        void Start()
        {
            if (maxVel == null || maxVel.Length != 3)
                throw new System.InvalidOperationException("initialize max Vel with 3 elements!");

            InitTornado();
            initBuffers();
            Load3DTextures();
            CreateMesh();
            camPos = Camera.main.transform;
            // TODO: seperate script
            Camera.main.cullingMatrix = Matrix4x4.Ortho(-99999, 99999, -99999, 99999, 2f, 99999) *
                                Matrix4x4.Translate(Vector3.forward * -99999 / 2f) *
                                Camera.main.worldToCameraMatrix;

            //ComputeAttenuationProperties();
        }

        private void InitTornado()
        {
            material = GetComponent<MeshRenderer>().material;
            float height = 1315f;
            float radius = 7500f;
            switch (QualitySettings.GetQualityLevel())
            {
                case 0:
                    particelNumExp = 10;
                    numCloudSkyParticles = 8;
                    material.SetFloat("g_fSize", 64);
                    material.SetFloat("g_fSizeTop", 8);
                    radius = 5000f;
                    break;
                case 1:
                    particelNumExp = 10;
                    numCloudSkyParticles = 8;
                    material.SetFloat("g_fSize", 96);
                    material.SetFloat("g_fSizeTop", 8);
                    radius = 5000f;
                    break;
                case 2:
                    particelNumExp = 11;
                    numCloudSkyParticles = 9;
                    material.SetFloat("g_fSize", 96);
                    material.SetFloat("g_fSizeTop", 8);
                    radius = 6000f;
                    break;
                case 3:
                    particelNumExp = 12;
                    numCloudSkyParticles = 9;
                    material.SetFloat("g_fSize", 64);
                    material.SetFloat("g_fSizeTop", 12);
                    radius = 6000f;
                    break;
                case 4:
                    particelNumExp = 13;
                    numCloudSkyParticles = 10;
                    material.SetFloat("g_fSize", 64);
                    material.SetFloat("g_fSizeTop", 12);
                    break;
                case 5:
                    particelNumExp = 14;
                    numCloudSkyParticles = 10;
                    material.SetFloat("g_fSize", 48);
                    material.SetFloat("g_fSizeTop", 16);
                    break;
                default:
                    particelNumExp = 13;
                    numCloudSkyParticles = 10;
                    material.SetFloat("g_fSize", 64);
                    material.SetFloat("g_fSizeTop", 12);
                    break;
            }

            numberParticles = (uint)Mathf.Pow(2, particelNumExp);

            targetShip = false;
            counter = 0;
            particlePos = new Vector3[numberParticles];
            particleVel = new Vector3[numberParticles];
            particleIdx = new int[numberParticles];
            Vector3 center = vectorField.GetCenterWS();
            center.y = height;
            for (int i = 0; i < numberParticles; i++)
            {
                if (i > Mathf.RoundToInt(Mathf.Pow(2, numCloudSkyParticles)))
                {
                    float x = Random.Range(0f, vectorField.GetDimensions()[0] * vectorField.GetHorizontalCellSize());
                    float z = Random.Range(0f, vectorField.GetDimensions()[2] * vectorField.GetHorizontalCellSize());
                    float y = Random.Range(-vectorField.GetDimensions()[1] * vectorField.GetCellSize() * 0.15f, vectorField.GetDimensions()[1] * vectorField.GetCellSize());
                    particlePos[i] = new Vector3(x, y, z);
                }
                else
                {
                    do
                    {
                        particlePos[i] = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius)) + center;
                    } while (Vector3.Distance(center, particlePos[i]) > radius);
                }
                particleIdx[i] = i;
            }
        }

        private void initBuffers()
        {
            //  particlesCB buffers
            particleVelCB = new ComputeBuffer((int)numberParticles, 12);
            particlePosCB = new ComputeBuffer((int)numberParticles, 12);
            particleinitialPosCB = new ComputeBuffer((int)numberParticles, 12);
            indicesCB = new ComputeBuffer((int)numberParticles, 4);
            indicesRCB = new ComputeBuffer((int)numberParticles, 4);
            particleVisibilityCB = new ComputeBuffer((int)numberParticles, 4);
            vectorFieldCBIn = new ComputeBuffer(vectorField.GetAmountOfElements(), 12);
            argsBuffer1 = new ComputeBuffer(3, 4, ComputeBufferType.IndirectArguments);
            argsBuffer2 = new ComputeBuffer(3, 4, ComputeBufferType.IndirectArguments);
            argsBuffer3 = new ComputeBuffer(3, 4, ComputeBufferType.IndirectArguments);
            argsBuffer4 = new ComputeBuffer(3, 4, ComputeBufferType.IndirectArguments);
            ComputeBuffer rndAzimuthCB = new ComputeBuffer((int)numberParticles, 4);

            int[] args1 = new int[3];
            args1[0] = Mathf.CeilToInt(numberParticles / 256f);
            args1[1] = 1;
            args1[2] = 1;
            argsBuffer1.SetData(args1);

            int[] args2 = new int[3];
            args2[0] = (int)((numberParticles / BLOCK_SIZE));
            args2[1] = 1;
            args2[2] = 1;
            argsBuffer2.SetData(args2);

            int[] args3 = new int[3];
            args3[0] = (int)(BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE);
            args3[1] = (int)((numberParticles / BLOCK_SIZE) / TRANSPOSE_BLOCK_SIZE);
            args3[2] = 1;
            argsBuffer3.SetData(args3);

            int[] args4 = new int[3];
            args4[1] = (int)(BLOCK_SIZE / TRANSPOSE_BLOCK_SIZE);
            args4[0] = (int)((numberParticles / BLOCK_SIZE) / TRANSPOSE_BLOCK_SIZE);
            args4[2] = 1;
            argsBuffer4.SetData(args4);
            //  get corresponding kernel index
            kernelP = particlesCS.FindKernel("UpdateParticles");
            kernelS = sortCS.FindKernel("BitonicSort");
            kernelT = sortCS.FindKernel("Transpose");
            float[] dims = new float[3];
            //  assume static grid size
            Vector3Int temp = vectorField.GetDimensions();
            dims[0] = temp.x;
            dims[1] = temp.y;
            dims[2] = temp.z;
            float[] cellsizes = {vectorField.GetHorizontalCellSize(), vectorField.GetCellSize(),vectorField.GetHorizontalCellSize() };
            maxDist = temp.x * vectorField.GetHorizontalCellSize() * 3.5f;
            float[] center = new float[3];
            center[0] = (temp.x - 1) * 0.5f * cellsizes[0];
            center[1] = (temp.y - 1) * 0.5f * cellsizes[1];
            center[2] = (temp.z - 1) * 0.5f * cellsizes[2];
            int[] idcs = new int[numberParticles];
            for (int i = 0; i < numberParticles; i++)
                idcs[i] = i;

            float[] rndAzimuthBias = new float[numberParticles];
            int[] visibilityData = new int[numberParticles];
            for (int i = 0; i < rndAzimuthBias.Length; i++)
            {
                rndAzimuthBias[i] = Random.Range(0, Mathf.PI * 2);
            }
            for (int i = 0; i < visibilityData.Length; i++)
            {
                visibilityData[i] = 1;
            }

            particlesCS.SetFloats("g_i3Dimensions", dims);
            particlesCS.SetFloats("g_f3CellSizes", cellsizes);
            particlesCS.SetFloats("g_f3Center", center);
            particlesCS.SetFloat("g_fDampVel", dampVel);
            particlesCS.SetInt("g_iNumCloudSkyParticles", Mathf.RoundToInt(Mathf.Pow(2, numCloudSkyParticles)));
            particlesCS.SetFloats("g_f3MaxVel", maxVel);
            particlesCS.SetFloat("g_fMaxDist", maxDist);
            particlesCS.SetInt("g_bEndAnimation", 0);

            material.SetFloat("g_fHeightInterp", dims[1] * cellsizes[1] * 0.333f);
            material.SetFloat("g_fMaxHeight", dims[1] * cellsizes[1]);
            //material.SetFloatArray("g_i3Dimensions", dims);
            material.SetVector("g_vCenter", new Vector4(center[0], center[1], center[2], 1.0f));
            material.SetFloat("g_fTopHeight",  dims[1] * cellsizes[1] * 1.05f);
            //  assume static data for compute buffers
            vectorFieldCBIn.SetData(vectorField.GetVectorField());
            particlePosCB.SetData(particlePos);
            particleinitialPosCB.SetData(particlePos);
            particleinitialPosCB.SetData(particlePos);
            particleVelCB.SetData(particleVel);
            indicesCB.SetData(idcs);
            indicesRCB.SetData(idcs);
            rndAzimuthCB.SetData(rndAzimuthBias);
            particleVisibilityCB.SetData(visibilityData);

            //  assume static vector field (apart from win animation)
            particlesCS.SetBuffer(kernelP, "vectorFieldIn", vectorFieldCBIn);
            particlesCS.SetBuffer(kernelP, "particlePosRW", particlePosCB);
            particlesCS.SetBuffer(kernelP, "particleVelRW", particleVelCB);
            particlesCS.SetBuffer(kernelP, "particleVisibilityRW", particleVisibilityCB);
            sortCS.SetBuffer(kernelS, "particlePos", particlePosCB);
            sortCS.SetBuffer(kernelS, "indicesRW", indicesCB);
            sortCS.SetBuffer(kernelT, "indices", indicesRCB);
            material.SetBuffer("g_vVertices", particlePosCB);
            material.SetBuffer("g_vInitialWorldPos", particleinitialPosCB);
            material.SetBuffer("g_iIndices", indicesCB);
            material.SetBuffer("g_vRndAzimuth", rndAzimuthCB);
            material.SetBuffer("particleVisibilityRW", particleVisibilityCB);

            rndAzimuthCB.Release();
        }

        private void UpdateParticles()
        {
            float dt = Time.deltaTime;
            particlesCS.SetFloat("g_fTimestep", dt);
            float[] randPos = new float[3];
            randPos[0] = Random.Range(-1f, 1f);//(rnd.Next(0, 2) * 2 - 1f) * (0.25f + (float)rnd.NextDouble() * 0.5f);
            randPos[1] = Random.Range(-1f, 1f);
            randPos[2] = Random.Range(-1f, 1f);//(rnd.Next(0, 2) * 2 - 1f) * (0.25f + (float)rnd.NextDouble() * 0.5f);
            particlesCS.SetFloats("g_f3RandPos", randPos);

            particlesCS.DispatchIndirect(kernelP, argsBuffer1);
            // particlesCS.Dispatch(kernelP, Mathf.CeilToInt(numberParticles / 1024f), 1, 1);            
        }

        private void SortParticles()
        {
            float[] camPos = new float[3];
            camPos[0] = Camera.main.transform.position.x;
            camPos[1] = Camera.main.transform.position.y;
            camPos[2] = Camera.main.transform.position.z;
            sortCS.SetFloats("g_vCameraPos", camPos);
            sortCS.SetInt("g_iNumPart", (int)numberParticles);
            //int groups = (int)((numberParticles / BLOCK_SIZE));

            // sort rows first
            for (int k = 2; k <= BLOCK_SIZE; k <<= 1)
            {
                sortCS.SetInt("k", k);
                sortCS.SetInt("g_iStage_2", k);
                //sortCS.Dispatch(kernelS, groups, 1, 1);
                sortCS.DispatchIndirect(kernelS, argsBuffer2);
            }
            uint width = BLOCK_SIZE;
            uint height = (numberParticles / BLOCK_SIZE);
            if (BLOCK_SIZE < numberParticles)
            {
                // transpose data and sort transposed columns then rows again
                for (uint k = (BLOCK_SIZE << 1); k <= numberParticles; k <<= 1)
                {
                    sortCS.SetInt("k", (int)(k / BLOCK_SIZE));
                    sortCS.SetInt("g_iStage_2", (int)((k & ~numberParticles) / BLOCK_SIZE));
                    sortCS.SetInt("g_iWidth", (int)width);
                    sortCS.SetInt("g_iHeight", (int)height);
                    sortCS.SetBuffer(kernelT, "indicesRW", indicesRCB);
                    sortCS.SetBuffer(kernelT, "indices", indicesCB);
                    sortCS.DispatchIndirect(kernelT, argsBuffer3);
                    //sortCS.Dispatch(kernelT, (int)(width / TRANSPOSE_BLOCK_SIZE), (int)(height / TRANSPOSE_BLOCK_SIZE), 1);

                    sortCS.SetBuffer(kernelS, "indicesRW", indicesRCB);
                    sortCS.DispatchIndirect(kernelS, argsBuffer2);
                    //sortCS.Dispatch(kernelS, groups, 1, 1);

                    sortCS.SetInt("k", (int)BLOCK_SIZE);
                    sortCS.SetInt("g_iStage_2", (int)k);
                    sortCS.SetInt("g_iWidth", (int)height);
                    sortCS.SetInt("g_iHeight", (int)width);
                    sortCS.SetBuffer(kernelT, "indicesRW", indicesCB);
                    sortCS.SetBuffer(kernelT, "indices", indicesRCB);
                    sortCS.DispatchIndirect(kernelT, argsBuffer4);
                    //sortCS.Dispatch(kernelT, (int)(height / TRANSPOSE_BLOCK_SIZE), (int)(width / TRANSPOSE_BLOCK_SIZE), 1);

                    sortCS.SetBuffer(kernelS, "indicesRW", indicesCB);
                    sortCS.DispatchIndirect(kernelS, argsBuffer2);
                    // sortCS.Dispatch(kernelS, groups, 1, 1);
                }
            }
        }


        public void Update()
        {
            UpdateParticles();
            counter = (counter + 1) % sortEach;
            if (counter == 0)
                SortParticles();
            if (targetShip)
                UpdateVectorFieldPS();
        }

        private void Load3DTextures()
        {
            Texture3D t1 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.streamingAssetsPath + "/v4/3DNoiseTex.dds", TextureFormat.Alpha8, 1), TextureFormat.Alpha8);
            material.SetTexture("g_tex3DNoise", t1);

            Texture3D t2 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.streamingAssetsPath + "/v4/Density.dds", TextureFormat.RGHalf, 4), TextureFormat.RGHalf);
            material.SetTexture("g_tex3DParticleDensityLUT", t2);

            Texture3D t3 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.streamingAssetsPath + "/v4/SingleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DSingleScatteringInParticleLUT", t3);

            Texture3D t4 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.streamingAssetsPath + "/v4/MultipleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DMultipleScatteringInParticleLUT", t4);
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
            //GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
        }

        private void OnEnable()
        {
            WinCondition.OnWin += ToggleTargetShip;
        }

        private void OnDisable()
        {
            WinCondition.OnWin -= ToggleTargetShip;
        }

        private void ToggleTargetShip(GameObject ship)
        {
            this.ship = ship;
            InitWinAnimationKernel();
        }

        private void InitWinAnimationKernel()
        {
            int kernelWA = winAnimation.FindKernel("UpdateVectorField");
            Vector3Int temp = vectorField.GetDimensions();
            maxVel[0] = float.MaxValue;
            maxVel[1] = float.MaxValue;
            maxVel[2] = float.MaxValue;
            float[] cellsizes = { vectorField.GetHorizontalCellSize(), vectorField.GetCellSize(), vectorField.GetHorizontalCellSize() };
            float[] dims = new float[3];
            dims[0] = temp.x;
            dims[1] = temp.y;
            dims[2] = temp.z;
            particlesCS.SetInt("g_bEndAnimation", 1);
            winAnimation.SetFloat("g_fVelocityScale", 6000.0f);
            winAnimation.SetFloats("g_i3Dimensions", dims);
            winAnimation.SetFloats("g_f3CellSizes", cellsizes);
            particlesCS.SetFloats("g_f3MaxVel", maxVel);
            particlesCS.SetFloat("g_fDampVel", 0.0f);
            particlesCS.SetInt("g_iNumCloudSkyParticles", 0);
            material.SetFloat("g_bSize", 1.0f);
            sortEach = 1;
            targetShip = true;
        }

        private void UpdateVectorFieldPS()
        {
            Vector3Int temp = vectorField.GetDimensions();
            float[] dims = new float[3];
            dims[0] = temp.x;
            dims[1] = temp.y;
            dims[2] = temp.z;
            float[] pos = new float[3];
            pos[0] = ship.transform.position.x;
            pos[1] = ship.transform.position.y;
            pos[2] = ship.transform.position.z;
            winAnimation.SetFloats("g_f3ShipPosition", pos);
            winAnimation.SetBuffer(kernelWA, "vectorField", vectorFieldCBIn);
            particlesCS.SetFloats("g_f3ShipPosition", pos);
            material.SetFloatArray("g_f3ShipPosition", pos);
            material.SetVector("g_f4ShipPosition", new Vector4(pos[0], pos[1], pos[2]));
            // hard coded values so far -> TODO: add as variables
            winAnimation.Dispatch(kernelWA, Mathf.CeilToInt(dims[0] / 8f), Mathf.CeilToInt(dims[1] / 8f), Mathf.CeilToInt(dims[2] / 4f));
        }

        /*
        private void ComputeAttenuationProperties()
        {
            //Graphics.SetRenderTarget()
            //RenderTexture rt = new RenderTexture(256, 256, 256, RenderTextureFormat.RFloat);
            //Graphics.SetRenderTarget(rt);
            RenderBuffer depth = new RenderBuffer();
            RenderBuffer color = new RenderBuffer();
            RenderTexture rt = new RenderTexture(1024, 1024, 1);
            cam.depthTextureMode = DepthTextureMode.Depth;
            ComputeBuffer bf = new ComputeBuffer(100, 1);
            //bf.SetData(depth.GetNativeRenderBufferPtr());
            //cam.forceIntoRenderTexture = true;
            //cam.worldToCameraMatrix = Matrix4x4.TRS(new Vector3(), Quaternion.Euler(90, 0, 0), Vector3.zero);
            //cam.projectionMatrix = Matrix4x4.Ortho(-320f, 320f, -320f, 320f, 0.001f, 40f);
            //cam.SetTargetBuffers(color, depth);
            //cam.targetTexture = rt;
            //cam.forceIntoRenderTexture = true;
            //cam.targetDisplay = 2;
            //cam.Render();
            print(Camera.main.depthTextureMode);
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
            //Graphics.DrawMesh(GetComponent<MeshFilter>().mesh, Matrix4x4.identity, GetComponent<Renderer>().material, 0, cam);
            //Graphics.DrawMeshNow()
        }*/

        void OnApplicationQuit()
        {
            // releasing compute buffers
            particleVelCB.Release();
            particlePosCB.Release();
            particleinitialPosCB.Release();
            indicesRCB.Release();
            indicesCB.Release();
            particleVisibilityCB.Release();
            vectorFieldCBIn.Release();
            argsBuffer1.Release();
            argsBuffer2.Release();
            argsBuffer3.Release();
            argsBuffer4.Release();
        }
    }
}