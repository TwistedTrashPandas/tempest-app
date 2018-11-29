﻿using UnityEngine;
using System.IO;
using System;
using MastersOfTempest.Tools;

namespace MastersOfTempest.Environment
{
    public class VectorField : MonoBehaviour
    {
        /// cellSize for the vector field
        public float f_cellSize = 1.0f;
        /// dimensions of the field
        public Vector3Int v3_dimensions;
        /// actual values
        public Vector3[,,] v3s_vectors;
        /// 3D texture for shader (vector field)
        public Texture3D tex;
        public bool loadFromFile;

        /// scaling of vector field
        [Range(0.01f, 256f)]
        public float velScale;

        [Range(0.001f, 1.0f)]
        public float yVelScale;

        private Vector2 v2_rotCenter;
        private String uniFilePath = "/UniFiles/";
        private String fileName = "plume3DHighRes_vel_";
        private String currentVelFile;

        // for loading files
        private static uint header_size = 288;
        // fixed grid size for the loaded files (in byte)
        private static uint grid_size = 160 * 320 * 160 * 12;


        void Awake()
        {
            if (v3_dimensions == null)
                throw new System.InvalidOperationException("Dimensions of grid not set in prefab!");
            v3s_vectors = new Vector3[v3_dimensions[0], v3_dimensions[1], v3_dimensions[2]];
            if (loadFromFile)
                LoadVectorFieldFromFile(999);
            else
                InitializeVectorField();
        }

        // decompresses and loads uni file for the vectorfield
        private void LoadVectorFieldFromFile(int fileIndex)
        {
            // decompress uni file
            string uni_name = Application.dataPath + uniFilePath + fileName + fileIndex.ToString("D" + 4) + ".uni";
            FileInfo inf = new FileInfo(uni_name);
            FileHandling.Decompress(inf);

            // load decompressed file
            name = Application.dataPath + uniFilePath + fileName + fileIndex.ToString("D" + 4);
            currentVelFile = name;
            inf = new FileInfo(name);
            byte[] buffer = FileHandling.ReadFile(inf, header_size + grid_size + 4);
            v3s_vectors = new Vector3[v3_dimensions[0], v3_dimensions[1], v3_dimensions[2]];

            // convert bytes to vectors
            uint start_idx = 4;
            for (uint i = start_idx; i < 320 - start_idx; i++)
            {
                for (uint j = start_idx; j < 160 - start_idx; j++)
                {
                    for (uint k = start_idx; k < 160 - start_idx; k++)
                    {
                        uint start_index = header_size + 4 + ((i) * 160 + (j) * 160 * 320 + (k)) * 12;
                        float x = BitConverter.ToSingle(buffer, (int)start_index);
                        float y = BitConverter.ToSingle(buffer, (int)start_index + 4) * yVelScale;
                        float z = BitConverter.ToSingle(buffer, (int)start_index + 8);

                        v3s_vectors[k - start_idx, i - start_idx, j - start_idx] = new Vector3(x, y, z) * velScale;
                    }
                }
            }
        }

        private void InitializeVectorField()
        {
            float y_vel = 1.0f* yVelScale, minScale = 0.85f, maxScale = 1.15f;
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
                for (int i = 0; i < nx; i++)
                {
                    for (int j = 0; j < nz; j++)
                    {
                        float diffX = v2_rotCenter.x - i;
                        float diffZ = v2_rotCenter.y - j;
                        float hypotenuse = Mathf.Sqrt(diffX * diffX + diffZ * diffZ);
                        v3s_vectors[i, k, j] = new Vector3(-diffZ / hypotenuse, y_vel, diffX / hypotenuse) * (velScale * strModifier);
                        v3s_vectors[i, k, j].Scale(new Vector3(UnityEngine.Random.Range(minScale, maxScale),
                            UnityEngine.Random.Range(minScale, maxScale), UnityEngine.Random.Range(minScale, maxScale)));
                    }
                }
            }
        }

        // getter methods
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

        public Vector3 GetCenter()
        {
            return new Vector3(v3_dimensions[0] * f_cellSize / 2f - 0.5f, v3_dimensions[0] * f_cellSize / 2f - 0.5f, v3_dimensions[0] * f_cellSize / 2f - 0.5f);
        }

        // calculates the extrapolated position in the grid (in a circular fashion)
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
            return v3s_vectors[i, j, k];
        }

        private void OnApplicationQuit()
        {
            FileHandling.DeleteFile(currentVelFile);
        }
    }
}