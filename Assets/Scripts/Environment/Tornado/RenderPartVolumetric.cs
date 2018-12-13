using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class RenderPartVolumetric : MonoBehaviour
    {
        struct SGlobalCloudAttribs
        {
            uint uiInnerRingDim;
            uint uiRingExtension;
            uint uiRingDimension;
            uint uiNumRings;

            uint uiMaxLayers;
            uint uiNumCells;
            uint uiMaxParticles;
            uint uiDownscaleFactor;

            float fCloudDensityThreshold;
            float fCloudThickness;
            float fCloudAltitude;
            float fParticleCutOffDist;

            float fTime;
            float fCloudVolumeDensity;
            Vector2 f2LiSpCloudDensityDim;

            uint uiBackBufferWidth;
            uint uiBackBufferHeight;
            uint uiDownscaledBackBufferWidth;
            uint uiDownscaledBackBufferHeight;

            float fBackBufferWidth;
            float fBackBufferHeight;
            float fDownscaledBackBufferWidth;
            float fDownscaledBackBufferHeight;

            float fTileTexWidth;
            float fTileTexHeight;
            uint uiLiSpFirstListIndTexDim;
            uint uiNumCascades;
            Vector4 f4Parameter;

            float fScatteringCoeff;
            float fAttenuationCoeff;
            uint uiNumParticleLayers;
            uint uiDensityGenerationMethod;

            bool bVolumetricBlending;
            Vector3 f3Dummy;
            Vector4 f4TilingFrustumPlanes1;
            Vector4 f4TilingFrustumPlanes2;
            Vector4 f4TilingFrustumPlanes3;
            Vector4 f4TilingFrustumPlanes4;
            Vector4 f4TilingFrustumPlanes5;
            Vector4 f4TilingFrustumPlanes6;
            Matrix4x4 mParticleTiling;
        }

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

            Texture3D t1 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/3DNoiseTex.dds", TextureFormat.Alpha8, 1), TextureFormat.Alpha8);
            material.SetTexture("g_tex3DNoise", t1);

            Texture3D t2 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/Density_1.dds", TextureFormat.RG16, 2), TextureFormat.RGHalf);
            material.SetTexture("g_tex3DParticleDensityLUT", t2);

            Texture3D t3 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/SingleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DSingleScatteringInParticleLUT", t3);

            Texture3D t4 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/MultipleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DMultipleScatteringInParticleLUT", t4);

            particles = new ComputeBuffer(10, 12);
            Vector3[] pos = new Vector3[10];
            particles.SetData(pos);
            material.SetBuffer("g_vVertices", particles);
            Vector3[] v = new Vector3[1];
            int[] t = new int[3];
            t[0] = 0;
            t[1] = 0;
            t[2] = 0;

            Mesh m = GetComponent<MeshFilter>().mesh;
            m.triangles = t;
            m.vertices = v;

            GetComponent<MeshFilter>().mesh = m;
        }

        private Texture3D Tex2DArrtoTex3D(Texture2D[] tex2DArr, TextureFormat tf)
        {
            int d = tex2DArr.Length;
            int w = tex2DArr[0].width;
            int h = tex2DArr[0].height;
            Texture3D tex = new Texture3D(w, h, d, tf, false);
            Color[] colors = new Color[w * d * h];
            int idx = 0;
            for (int k = 0; k < d; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++, idx++)
                    {
                        colors[idx] = tex2DArr[k].GetPixel(j, i);
                    }
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
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
