using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MastersOfTempest.Tools
{
    public class DDSImport : MonoBehaviour
    {
        public struct DDS_PIXELFORMAT
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwFourCC;
            public uint dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
        };
        public struct DDS_HEADER
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;
            public uint dwDepth;
            public uint dwMipMapCount;
            public uint dwReserved1;
            public uint dwReserved2;
            public uint dwReserved3;
            public uint dwReserved4;
            public uint dwReserved5;
            public uint dwReserved6;
            public uint dwReserved7;
            public uint dwReserved8;
            public uint dwReserved9;
            public uint dwReserved10;
            public uint dwReserved11;
            public DDS_PIXELFORMAT ddspf;
            public uint dwCaps;
            public uint dwCaps2;
            public uint dwCaps3;
            public uint dwCaps4;
            public uint dwReserved12;
        };

        public static Texture2D[] ReadAndLoadTextures(string path, TextureFormat textureFormat, int sizeOfFormat)
        {
            byte[] data = Tools.FileHandling.ReadFile(path);
            return LoadTextureDXT(data, textureFormat, sizeOfFormat);
        }
        
        public static Texture3D Tex2DArrtoTex3D(Texture2D[] tex2DArr, TextureFormat tf)
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

        public static Texture2D[] LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat, int sizeOfFormat)
        {
            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

            DDS_HEADER header = new DDS_HEADER();
            var bufferSize = Marshal.SizeOf(header);
            IntPtr handle = Marshal.AllocHGlobal(bufferSize);
            Marshal.Copy(ddsBytes, 4, handle, bufferSize);
            header = Marshal.PtrToStructure<DDS_HEADER>(handle);
            int height = (int)header.dwHeight;
            int width = (int)header.dwWidth;
            int depth = (int)header.dwDepth;
            Texture2D[] texs = new Texture2D[depth];
            int texSize = height * width * sizeOfFormat;
            for (int i = 0; i < depth; i++)
            {
                byte[] dxtBytes = new byte[texSize];
                Buffer.BlockCopy(ddsBytes, bufferSize + 4 + i * texSize, dxtBytes, 0, texSize);
                Texture2D texture = new Texture2D(width, height, textureFormat, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
                texs[i] = texture;
            }

            return (texs);
        }
    }

}