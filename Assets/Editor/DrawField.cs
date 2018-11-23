using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MastersOfTempest.Environment;

[CustomEditor(typeof(VectorField))]
public class DrawField : Editor
{
    // draw vector field in scene view
    private void OnSceneGUI()
    {/*
        if (Application.isPlaying) { 
        VectorField grid = target as VectorField;
        // Handles.DrawLine(grid.transform.position + grid.v3_dimensions[0] * new Vector3(0,0,2), grid.transform.position);
        int x = grid.v3_dimensions[0], y = grid.v3_dimensions[1], z = grid.v3_dimensions[2];
        float c_s = grid.f_cellSize;
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
        }*/
    }
}
