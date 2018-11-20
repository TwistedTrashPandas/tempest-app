using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GenNoiseTexture : MonoBehaviour
{

    public int resolution_x = 1024;
    public int resolution_y = 1024;

    public Texture2D tex;
    private float[,] noiseArr; 

    // Use this for initialization
    void Awake()
    {
        noiseArr = new float[resolution_x, resolution_y];
        for (int i = 0; i < resolution_x; i++)
        {
            for (int j = 0; j < resolution_y; j++)
            {
                noiseArr[i,j] = Random.Range(0f, 1f);
            }
        }
        Gen2DTexture();
    }

    void Gen2DTexture()
    {
        tex = new Texture2D(resolution_x, resolution_y, TextureFormat.RGB24, false);
        float size = 256f;
        float avg = 0f;
        for (int i = 0; i < resolution_x; i++)
        {
            for (int j = 0; j < resolution_y; j++)
            {
                float currVal = Turbulence(i, j, size);
                tex.SetPixel(i, j, new Color(currVal, currVal, currVal));
                avg += currVal;
            }
        }
        tex.Apply(false);
    }

    float Turbulence(int x, int y, float size)
    {
        float value = 0.0f, initialSize = size;

        while (size >= 1)
        {
            value += smoothNoise(x / size, y / size) * size;
            size /= 2.0f;
        }

        return (value / initialSize);
    }

    float smoothNoise(float x, float y)
    {
        //get fractional part of x and y
        float fractX = x - (int) x;
        float fractY = y - (int) y;

        //wrap around
        int x1 = ((int)x + resolution_x) % resolution_x;
        int y1 = ((int)y + resolution_y) % resolution_y;

        //neighbor values
        int x2 = (x1 + resolution_x - 1) % resolution_x;
        int y2 = (y1 + resolution_y - 1) % resolution_y;

        //smooth the noise with bilinear interpolation
        float value = 0f;
        value += fractX * fractY * noiseArr[x1, y1];
        value += (1 - fractX) * fractY * noiseArr[x2,y1];
        value += fractX * (1 - fractY) * noiseArr[x1,y2];
        value += (1 - fractX) * (1 - fractY) * noiseArr[x2, y2];

        return value;
    }

    public Texture2D GetNoise()
    {
        return tex;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
