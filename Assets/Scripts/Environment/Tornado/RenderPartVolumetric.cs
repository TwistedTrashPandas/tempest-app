using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class RenderPartVolumetric : MonoBehaviour
    {
        private Material material;

        private ComputeBuffer particles;
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

            particles = new ComputeBuffer(10, 12);
            Vector3[] pos = new Vector3[10];
            particles.SetData(pos);
            material.SetBuffer("g_vVertices", particles);
            Vector3[] v = new Vector3[10];
            int[] t = new int[6];
            t[0] = 0;
            t[1] = 1;
            t[2] = 2;
            t[3] = 0;
            t[4] = 1;
            t[5] = 2;
            t[3] = 0;
            t[4] = 1;
            t[5] = 2;
            v[0] = new Vector3();
            v[1] = new Vector3(0,-5,0);
            v[2] = new Vector3(0,-10,0);

            // Camera.main.cameraToWorldMatrix;
            Camera.main.cullingMatrix = Matrix4x4.Ortho(-99999, 99999, -99999, 99999, 2f, 99999) *
                                Matrix4x4.Translate(Vector3.forward * -99999 / 2f) *
                                Camera.main.worldToCameraMatrix;
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
            Mesh m = GetComponent<MeshFilter>().mesh;
            m = new Mesh();
            m.vertices = v;
            m.triangles = t;
            Vector3 center = Vector3.zero;
            GetComponent<MeshFilter>().mesh = m;
            //transform.position = new Vector3();
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
