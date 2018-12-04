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
        public Shader renderParticlesS;
        
        /// Vector field for tornado
        public VectorField vectorField;
        public Transform camPos;

        /// sort all particles with respect to the camera position each "sortEach" timestep
        [Range(1, 100)]
        public int sortEach;
        [Range(15, 20)]
        public uint particelNumExp;
        [Range(0f, 1f)]
        public float dampVel;

        public float g_fTimeDiff;
        public float g_fTimeStepTex;

        public string densityTexPrefix;
        public string normalTexPrefix;
        public int startIdx;
        public int endIdx;
        public int skipIdx;
        public float[] maxVel;

        /// In and out Computer buffers for the shader
        private ComputeBuffer particleVelCB;
        private ComputeBuffer particlePosCB;
        private ComputeBuffer particleinitialPosCB;
        private ComputeBuffer indicesCB;
        private ComputeBuffer indicesRCB;
        private ComputeBuffer vectorFieldCBIn;

        /// kernel for computeshader
        private int kernelP;
        private int kernelS;
        private int kernelT;

        /// amount of particles
        private uint numberParticles;
        private float maxDist;
        /// arrays for particles
        private Vector3[] particlePos;
        private Vector3[] particleVel;
        private Texture2D partTex;
        private Texture2D[] densityTextures;
        private Texture2D[] normalTextures;
        private int[] particleIdx;

        private Material material;

        // counter for sorting particles
        private int counter;

        const uint BLOCK_SIZE = 1024;
        const uint TRANSPOSE_BLOCK_SIZE = 32;

        System.Random rnd = new System.Random();

        void Start()
        {
            if (maxVel == null || maxVel.Length != 3)
                throw new System.InvalidOperationException("initialize max Vel with 3 elements!");

            numberParticles = (uint)Mathf.Pow(2, particelNumExp);
            counter = 0;
            rnd = new System.Random();
            particlePos = new Vector3[numberParticles];
            particleVel = new Vector3[numberParticles];
            particleIdx = new int[numberParticles];
            for (int i = 0; i < numberParticles; i++)
            {
                float x = Random.Range(0f, vectorField.GetDimensions()[0] * vectorField.GetCellSize());
                float z = Random.Range(0f, vectorField.GetDimensions()[2] * vectorField.GetCellSize());
                float y = Random.Range(-vectorField.GetDimensions()[1] * vectorField.GetCellSize() * 0.2f, vectorField.GetDimensions()[1] * vectorField.GetCellSize());
                particlePos[i] = new Vector3(x, y, z);
                particleIdx[i] = i;
            }
            material = GetComponent<MeshRenderer>().material;
            initBuffers();
            CreateMesh();
            LoadTextures();
            camPos = Camera.main.transform;
            StartCoroutine(UpdateTextures());
            partTex = GenNoiseTexture.Gen2DTexture(1024, 1024);
            // GetComponent<Renderer>().material.SetTexture("g_NoiseTex", partTex);
        }

        private void LoadTextures()
        {
            densityTextures = new Texture2D[(endIdx - startIdx) / skipIdx];
            normalTextures = new Texture2D[(endIdx - startIdx) / skipIdx];
            for (int i = startIdx; i < endIdx; i += skipIdx)
            {
                string filepath = Application.dataPath + "/UniFiles/DensityTextures/" + densityTexPrefix + i.ToString("D" + 4) + ".png";
                byte[] buffer = Tools.FileHandling.ReadFile(filepath);
                densityTextures[(i - startIdx) / skipIdx] = new Texture2D(720, 1024);
                densityTextures[(i - startIdx) / skipIdx].LoadImage(buffer);
                filepath = Application.dataPath + "/UniFiles/NormalTextures/" + normalTexPrefix + i.ToString("D" + 4) + ".png";
                buffer = Tools.FileHandling.ReadFile(filepath);
                normalTextures[(i - startIdx) / skipIdx] = new Texture2D(720, 1024);
                normalTextures[(i - startIdx) / skipIdx].LoadImage(buffer);
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
            vectorFieldCBIn = new ComputeBuffer(vectorField.GetAmountOfElements(), 12);
            //  get corresponding kernel index
            kernelP = particlesCS.FindKernel("UpdateParticles");
            kernelS = sortCS.FindKernel("BitonicSort");
            kernelT = sortCS.FindKernel("Transpose");
            float[] dims = new float[4];
            //  assume static grid size
            Vector3Int temp = vectorField.GetDimensions();
            dims[0] = temp.x;
            dims[1] = temp.y;
            dims[2] = temp.z;
            dims[3] = Mathf.RoundToInt(vectorField.GetCellSize());
            maxDist = temp.x * vectorField.GetCellSize() * 3f;
            float[] center = new float[3];
            center[0] = (temp.x - 1) * 0.5f * dims[3];
            center[1] = (temp.y - 1) * 0.5f * dims[3];
            center[2] = (temp.z - 1) * 0.5f * dims[3];
            int[] idcs = new int[numberParticles];
            for (int i = 0; i < numberParticles; i++)
                idcs[i] = i;

            particlesCS.SetFloats("g_i3Dimensions", dims);
            particlesCS.SetFloats("g_vCenter", center);
            particlesCS.SetFloat("g_fDampVel", dampVel);
            particlesCS.SetFloats("g_fMaxVel", maxVel);
            particlesCS.SetFloat("g_fMaxDist", maxDist);

            material.SetFloat("g_fHeightInterp", dims[1] * dims[3] * 0.333f);
            material.SetFloat("g_fMaxHeight", dims[1] * dims[3]);
            material.SetVector("g_i3Dimensions", new Vector4(dims[0], dims[1], dims[2], dims[3]));
            material.SetVector("g_vCenter", new Vector4(center[0], center[1], center[2], 1.0f));

            //  assume static data for compute buffers
            vectorFieldCBIn.SetData(vectorField.GetVectorField());
            particlePosCB.SetData(particlePos);
            particleinitialPosCB.SetData(particlePos);
            particleVelCB.SetData(particleVel);
            indicesCB.SetData(idcs);
            indicesRCB.SetData(idcs);
            //  assume static vector field
            particlesCS.SetBuffer(kernelP, "vectorFieldIn", vectorFieldCBIn);
            particlesCS.SetBuffer(kernelP, "particlePosRW", particlePosCB);
            particlesCS.SetBuffer(kernelP, "particleVelRW", particleVelCB);
            sortCS.SetBuffer(kernelS, "particlePos", particlePosCB);
            sortCS.SetBuffer(kernelS, "indicesRW", indicesCB);
            sortCS.SetBuffer(kernelT, "indices", indicesRCB);
            material.SetBuffer("g_vVertices", particlePosCB);
            material.SetBuffer("g_vInitialWorldPos", particleinitialPosCB);
            material.SetBuffer("g_iIndices", indicesCB);
            material.SetFloat("g_fTimeStepTex", g_fTimeStepTex);
        }

        IEnumerator UpdateTextures()
        {
            while (true)
            {
                Vector3 look =- camPos.position + vectorField.GetCenter();
                look.y = 0;
                look = Vector3.Normalize(look);
                float angle =Mathf.Rad2Deg * (Mathf.Atan2(look.x, look.z) - Mathf.Atan2(0f, 1f));
                angle = (angle < 0f) ? 360f+angle : angle;
                int idx = Mathf.FloorToInt(angle / 5f);
                material.SetTexture("g_Tex1", densityTextures[idx % densityTextures.Length]);
                material.SetTexture("g_Tex2", densityTextures[(idx + 1) % densityTextures.Length]);
                material.SetTexture("g_NormalTex1", normalTextures[idx % densityTextures.Length]);
                material.SetTexture("g_NormalTex2", normalTextures[(idx + 1) % densityTextures.Length]);
                material.SetFloat("g_fTimeDiff", (angle-idx*5f)/5f * g_fTimeStepTex);
                material.SetFloat("g_fTimeStepTex", g_fTimeStepTex);
                yield return new WaitForSeconds(g_fTimeStepTex);
            }
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
            material.SetBuffer("g_vVertices", particlePosCB);
        }

        private void SortParticles()
        {
            float[] camPos = new float[3];
            camPos[0] = Camera.main.transform.position.x;
            camPos[1] = Camera.main.transform.position.y;
            camPos[2] = Camera.main.transform.position.z;
            sortCS.SetFloats("g_vCameraPos", camPos);
            sortCS.SetInt("g_iNumPart", (int)numberParticles);
            int groups = (int)((numberParticles / BLOCK_SIZE));

            // sort rows first
            for (int k = 2; k <= BLOCK_SIZE; k <<= 1)
            {
                sortCS.SetInt("k", k);
                sortCS.SetInt("g_iStage_2", k);
                sortCS.Dispatch(kernelS, groups, 1, 1);
            }
            uint width = BLOCK_SIZE;
            uint height = (numberParticles / BLOCK_SIZE);
            
            // transpose data and sort transposed columns then rows again
            for (uint k = (BLOCK_SIZE << 1); k <= numberParticles; k <<= 1)
            {
                sortCS.SetInt("k", (int)(k / BLOCK_SIZE));
                sortCS.SetInt("g_iStage_2", (int)((k & ~numberParticles) / BLOCK_SIZE));
                sortCS.SetInt("g_iWidth", (int)width);
                sortCS.SetInt("g_iHeight", (int)height);
                sortCS.SetBuffer(kernelT, "indicesRW", indicesRCB);
                sortCS.SetBuffer(kernelT, "indices", indicesCB);
                sortCS.Dispatch(kernelT, (int)(width / TRANSPOSE_BLOCK_SIZE), (int)(height / TRANSPOSE_BLOCK_SIZE), 1);
                
                sortCS.SetBuffer(kernelS, "indicesRW", indicesRCB);
                sortCS.Dispatch(kernelS, groups, 1, 1);

                sortCS.SetInt("k", (int)BLOCK_SIZE);
                sortCS.SetInt("g_iStage_2", (int)k);
                sortCS.SetInt("g_iWidth", (int)height);
                sortCS.SetInt("g_iHeight", (int)width);
                sortCS.SetBuffer(kernelT, "indicesRW", indicesCB);
                sortCS.SetBuffer(kernelT, "indices", indicesRCB);
                sortCS.Dispatch(kernelT, (int)(height / TRANSPOSE_BLOCK_SIZE), (int)(width / TRANSPOSE_BLOCK_SIZE), 1);

                sortCS.SetBuffer(kernelS, "indicesRW", indicesCB);
                sortCS.Dispatch(kernelS, groups, 1, 1);
            }
        }


        public void Update()
        {
            UpdateParticles();
            counter = (counter + 1) % sortEach;
            if (counter == 0)
                SortParticles();
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
        }

        void OnApplicationQuit()
        {
            // releasing compute buffers
            particleVelCB.Release();
            particlePosCB.Release();
            indicesCB.Release();
            vectorFieldCBIn.Release();
        }
    }
}