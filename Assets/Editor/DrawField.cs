using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VectorField))]
public class DrawField : Editor
{


    // draw vector field in scene view
    private void OnSceneGUI()
    {
        if (Application.isPlaying) { 
        VectorField grid = target as VectorField;
        // Handles.DrawLine(grid.transform.position + grid.v3_dimensions[0] * new Vector3(0,0,2), grid.transform.position);
        int x = grid.v3_dimensions[0], y = grid.v3_dimensions[1], z = grid.v3_dimensions[2];
        float c_s = grid.f_cellSize;/*
        for (int i = 0; i < x+1; i++)
        {
            for (int j = 0; j < y+1; j++)
            {
                Handles.DrawLine(grid.transform.position + new Vector3(0f, 1f, 0f) * c_s * j + new Vector3(1f, 0f, 0f) * c_s * i,
                grid.transform.position + new Vector3(0f, 0f, 1f) * z * c_s
                + new Vector3(0f, 1f, 0f) * c_s * j + new Vector3(1f, 0f, 0f) * c_s * i);
           }
        }
        for (int i = 0; i < y+1; i++)
        {
            for (int j = 0; j < z+1; j++)
            {
                Handles.DrawLine(grid.transform.position + new Vector3(0f, 1f, 0f) * c_s * i + new Vector3(0f, 0f, 1f) * c_s * j, 
                grid.transform.position + new Vector3(1f, 0f, 0f) * x * c_s
                + new Vector3(0f, 1f, 0f) * c_s * i + new Vector3(0f, 0f, 1f) * c_s * j);
           }
        }
        for (int i = 0; i < z + 1; i++)
        {
            for (int j = 0; j < x + 1; j++)
            {
                Handles.DrawLine(grid.transform.position + new Vector3(1f, 0f, 0f) * c_s * j + new Vector3(0f, 0f, 1f) * c_s * i,
                grid.transform.position + new Vector3(0f, 1f, 0f) * y * c_s
                + new Vector3(1f, 0f, 0f) * c_s * j + new Vector3(0f, 0f, 1f) * c_s * i);
            }
        }*/
        
        int stepsize = 16;
            for (int i = 0; i < x; i += stepsize)
            {
                for (int j = 0; j < y; j += stepsize)
                {
                    for (int k = 0; k < z; k += stepsize)
                    {
                        // Handles.color = new Color(grid.v3s_vectors[i, j, k].x, grid.v3s_vectors[i, j, k].y, grid.v3s_vectors[i, j, k].z);
                        Handles.DrawLine(grid.transform.position + new Vector3(i, j, k) * c_s, grid.transform.position + new Vector3(i, j, k) * c_s + grid.v3s_vectors[i, j, k]);
                    }
                }
            }
        }
    }
}
