using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class RenderPartVolumetric : MonoBehaviour
    {
        public float radius;
        public float height;
        public int numParts;
        public Vector3 center;

        private Material material;

        private ComputeBuffer particles;
        private ComputeBuffer indicesCB;
        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            material = GetComponent<Renderer>().material;

            Texture3D t1 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/v4/3DNoiseTex.dds", TextureFormat.Alpha8, 1), TextureFormat.Alpha8);
            material.SetTexture("g_tex3DNoise", t1);

            Texture3D t2 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/v4/Density.dds", TextureFormat.RGHalf, 4), TextureFormat.RGFloat, true);
            material.SetTexture("g_tex3DParticleDensityLUT", t2);

            Texture3D t3 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/v4/SingleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DSingleScatteringInParticleLUT", t3);

            Texture3D t4 = Tools.DDSImport.Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/v4/MultipleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DMultipleScatteringInParticleLUT", t4);


            // Camera.main.cameraToWorldMatrix;
            /*Camera.main.cullingMatrix = Matrix4x4.Ortho(-99999, 99999, -99999, 99999, 2f, 99999) *
                                Matrix4x4.Translate(Vector3.forward * -99999 / 2f) *
                                Camera.main.worldToCameraMatrix;*/
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
            InitMesh();
        }

        private void InitMesh()
        {
            ComputeBuffer rndAzimuthCB = new ComputeBuffer((int)numParts, 4);
            particles = new ComputeBuffer(numParts, 12);
            indicesCB = new ComputeBuffer(numParts, 4);
            Vector3[] pos = new Vector3[numParts];
            int[] indices = new int[numParts];
            for (int i = 0; i < numParts; i++)
            {
                do {
                    pos[i] = new Vector3(Random.Range(-radius, radius), height,Random.Range(-radius, radius));
                } while (Vector3.Distance(center, pos[i]) > radius);
                indices[i] = i;
            }

            float[] rndAzimuthBias = new float[numParts];
            for (int i = 0; i < rndAzimuthBias.Length; i++)
                rndAzimuthBias[i] = Random.Range(0, Mathf.PI * 2);

            rndAzimuthCB.SetData(rndAzimuthBias);
            particles.SetData(pos);
            indicesCB.SetData(indices);
            material.SetBuffer("g_vVertices", particles);
            material.SetBuffer("g_iIndices", indicesCB);
            material.SetBuffer("g_vRndAzimuth", rndAzimuthCB);

            Vector2[] newUV;
            newUV = new Vector2[pos.Length];

            //  initialize texture coordinates
            for (int i = 0; i < pos.Length; i++)
            {
                newUV[i] = new Vector2(pos[i].x, pos[i].z);
            }

            //  create new mesh
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();
            mesh.vertices = pos;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.uv = newUV;
            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void Update()
        {
            //this.transform.rotation = Quaternion.Euler(0f,Camera.main.transform.rotation.eulerAngles.y,0f);//
            Vector3 dir = (transform.position - Camera.main.transform.position);
            
            //this.transform.LookAt(transform.position + dir);
            /*Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Camera.main.transform.rotation, new Vector3(1, 1, 1));
            cam.worldToCameraMatrix = m * transform.worldToLocalMatrix;*/
        }

        private Texture2D Tex2DArrtoTex2D(Texture2D[] tex2DArr, TextureFormat tf)
        {
            int d = tex2DArr.Length;
            int w = tex2DArr[0].width;
            int h = tex2DArr[0].height;
            Texture2D tex = new Texture2D(w, d * h, tf, false);
            Color[] colors = new Color[w * d * h];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    for (int k = 0; k < d; k++)
                    {
                        colors[j + i * w + k * h * w] = tex2DArr[k].GetPixel(j, i);
                    }
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}
