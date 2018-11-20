using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;

public class VectorField : MonoBehaviour
{
    /// cellSize for the vector field
    public float f_cellSize = 10.0f;
    /// dimensions of the field
    public Vector3Int v3_dimensions = new Vector3Int(64, 64, 64);
    /// actual values
    public Vector3[,,] v3s_vectors;

    /// 3D texture for shader (vector field)
    public Texture3D tex;

    private Vector2 v2_rotCenter;

    // for loading files
    private static int header_size = 288; // + 4
    private static int grid_size = 64 * 64 * 64 * 12;

    void Awake()
    {
        v3s_vectors = new Vector3[v3_dimensions[0], v3_dimensions[1], v3_dimensions[2]];
        InitializeVectorField();
        // Create3DTexture();
        // LoadVectorFieldFromFile();
    }

    private void InitializeVectorField()
    {
        /*
        Vector2 middle = new Vector2(v3_dimensions[0] / 2f * f_cellSize, v3_dimensions[2] / 2f * f_cellSize);
        float angle = 10f;
        for (int i = 0; i < v3_dimensions[0]; i++)
        {
            for (int j = 0; j < v3_dimensions[1]; j++)
            {
                for (int k = 0; k < v3_dimensions[2]; k++)
                {
                    Vector2 localPos = new Vector2((i + 0.5f) * f_cellSize, (k + 0.5f) * f_cellSize) - middle;
                    float r = localPos.magnitude;
                    if (r >  f_cellSize * v3_dimensions[2] * 0.25f * ((j+1f) / (float) v3_dimensions[1]))
                        r *= 0.6f;
                    else if (r < f_cellSize * v3_dimensions[2] * 0.1f)
                    {
                        r += 0.01f;
                        r *= 1.2f;
                    }
                    float t = Vector2.SignedAngle(new Vector2(1, 0), localPos);
                    float a = Mathf.Deg2Rad * (t + angle + 90f);
                    float x = r * Mathf.Sin(a);
                    float z = r * Mathf.Cos(a) * Mathf.Sign(t) * Mathf.Sign(Vector2.SignedAngle(new Vector2(-1, 0), localPos));
                    v3s_vectors[i, j, k] = new Vector3((x - localPos.x) * f_cellSize, 5f, (z - localPos.y) * f_cellSize) * strength / (r);
                    }
            }
        }*/

        float strength = 50f * f_cellSize, y_vel = 0.125f, minScale = 0.85f, maxScale = 1.15f;
        int nx = v3_dimensions[0], ny = v3_dimensions[1], nz = v3_dimensions[2];
        float middleOffset = nx / 128f;
        v2_rotCenter = new Vector2((nx - 1f) / 2f, (nz - 1f) / 2f);
        Vector2 midOffset = new Vector2((nx - 1f) / 4f, (nz - 1f) / 4f);
        /*Vector2 midOffset_1 = new Vector2((nx - 1f) / 4f, (nz - 1f) / 4f);
        for (float k = 0; k < ny; k++)
        {
            Vector2 middle = new Vector2((nx - 1f) / 2f, (nz - 1f) / 2f);
            if (k < 3f * ny / 5f)
            {
                middle = Vector2.Lerp(middle, midOffset_1, k / ny * 5f / 3f);
            }
            else
            {
                middle = Vector2.Lerp(midOffset_1, middle, (k - (ny * 5f / 3f)) / ny * 5f / 3f);
            }*/
        for (int k = 0; k < ny; k++)
        {
            float strModifier = 1.0f;
            if (k > ny / 5f)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f && v2_rotCenter.x < midOffset.x + (nx - 1f) / 2f)
                {
                    v2_rotCenter.x += middleOffset;
                }
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f && v2_rotCenter.y < midOffset.y + (nz - 1f) / 2f)
                {
                    v2_rotCenter.y += middleOffset;
                }
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f && v2_rotCenter.x > midOffset.x)
                {
                    v2_rotCenter.x -= middleOffset;
                }
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f && v2_rotCenter.y > midOffset.y)
                {
                    v2_rotCenter.y -= middleOffset;
                }
            }
            //  if (k < ny / 3f)
            //    strModifier = (k + 1) / (ny / 3f);
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < nz; j++)
                {
                    float diffX = v2_rotCenter.x - i;
                    float diffZ = v2_rotCenter.y - j;
                    float hypotenuse = Mathf.Sqrt(diffX * diffX + diffZ * diffZ);
                    v3s_vectors[i, k, j] = new Vector3(diffZ / hypotenuse, y_vel, -diffX / hypotenuse) * (strength * strModifier);
                    v3s_vectors[i, k, j].Scale(new Vector3(UnityEngine.Random.Range(minScale, maxScale),
                        UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale)));
                }
            }
        }
    }

    public Vector3[,,] GetVectorField()
    {
        return v3s_vectors;
    }

    public float GetCellSize()
    {
        return f_cellSize;
    }

    public Vector3Int GetDimensions()
    {
        return v3_dimensions;
    }

    public int GetAmountOfElements()
    {
        return v3_dimensions[0] * v3_dimensions[1] * v3_dimensions[2];
    }

    public Vector3 GetVectorAtPos(Vector3 pos)
    {
        Vector3 tmp = pos - transform.position;
        float x_proj = tmp.x - v2_rotCenter.x;
        float z_proj = tmp.z - v2_rotCenter.y;
        int i = (int)(tmp.x / f_cellSize),
            j = (int)Mathf.Clamp((tmp.y / f_cellSize), 0, v3_dimensions[1] - 1),
            k = (int)(tmp.z / f_cellSize);
        if ((i < 0 || i >= v3_dimensions[0]) || (k < 0 || k >= v3_dimensions[2]))
        {
            if (Mathf.Abs(x_proj) > Mathf.Abs(z_proj))
            {
                z_proj /= x_proj;
                x_proj = Mathf.Sign(x_proj);
                z_proj *= x_proj;
            }
            else
            {
                x_proj /= z_proj;
                z_proj = Mathf.Sign(z_proj);
                x_proj *= z_proj;
            }
            i = (int)((x_proj * 0.5f + 0.5f) * (v3_dimensions[0] - 1));
            k = (int)((z_proj * 0.5f + 0.5f) * (v3_dimensions[2] - 1));
        }
        else
        {
            i = Math.Min(v3_dimensions[0] - 1, Math.Max(i, 0));
            k = Math.Min(v3_dimensions[2] - 1, Math.Max(k, 0));
        }
        //print(v3s_vectors[i, j, k]);
        //print(v3s_vectors[i+1, j, k]);
        return v3s_vectors[i, j, k];
    }

    private void LoadVectorFieldFromFile()
    {
        string name = "B:\\Unity\\ShaderOrTornado\\Assets\\mantaflow\\data\\velocity_low_0200.uni";
        FileInfo inf = new FileInfo(name);
        Decompress(inf);
        name = "B:\\Unity\\ShaderOrTornado\\Assets\\mantaflow\\data\\velocity_low_0200";
        inf = new FileInfo(name);
        byte[] buffer = ReadFile(inf);
        v3s_vectors = new Vector3[v3_dimensions[0], v3_dimensions[1], v3_dimensions[2]];
        // convert bytes to vectors

        for (int i = 0; i < v3_dimensions[0]; i++)
        {
            for (int j = 0; j < v3_dimensions[1]; j++)
            {
                for (int k = 0; k < v3_dimensions[2]; k++)
                {
                    int start_index = header_size + 4 + (i * v3_dimensions[0] + j * v3_dimensions[0] * v3_dimensions[2] + k) * 12;
                    float x = BitConverter.ToSingle(buffer, start_index);
                    float y = BitConverter.ToSingle(buffer, start_index + 4);
                    float z = BitConverter.ToSingle(buffer, start_index + 8);
                    v3s_vectors[k, i, j] = new Vector3(x, y, z) * 32;
                }
            }
        }
    }

    public static void Decompress(FileInfo fileToDecompress)
    {
        using (FileStream originalFileStream = fileToDecompress.OpenRead())
        {
            string currentFileName = fileToDecompress.FullName;
            string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

            using (FileStream decompressedFileStream = File.Create(newFileName))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    // decompressionStream.CopyTo(decompressedFileStream);
                }
            }
        }
    }

    public static byte[] ReadFile(FileInfo fileToRead)
    {
        byte[] buffer = new byte[header_size + grid_size + 4];
        using (FileStream originalFileStream = fileToRead.OpenRead())
        {
            originalFileStream.Read(buffer, 0, buffer.Length);
        }
        return buffer;
    }
}
