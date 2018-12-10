using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.VisualEffects
{
    public class RenderPartVolumetric : MonoBehaviour
    {
        private Material material;

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

            Texture3D t2 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/Density_1.dds", TextureFormat.RG16, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DParticleDensityLUT", t2);

            Texture3D t3 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/SingleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DSingleScatteringInParticleLUT", t3);

            Texture3D t4 = Tex2DArrtoTex3D(Tools.DDSImport.ReadAndLoadTextures(Application.dataPath + "/Textures/CloudParticles/MultipleSctr.dds", TextureFormat.RHalf, 2), TextureFormat.RHalf);
            material.SetTexture("g_tex3DMultipleScatteringInParticleLUT", t4);
        }

        private Texture3D Tex2DArrtoTex3D(Texture2D[] tex2DArr, TextureFormat tf)
        {
            int d = tex2DArr.Length;
            int w = tex2DArr[0].width;
            int h = tex2DArr[0].height;
            Texture3D tex = new Texture3D(w, h, d, tf, false);
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
