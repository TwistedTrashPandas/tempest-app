﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateReflectionTexture : MonoBehaviour
{
    /// <summary>
    /// Texture size of the reflection
    /// </summary>       
    public int textureSize = 512;
    /// <summary>
    /// Layermask to ignore certain layers
    /// </summary>       
    public LayerMask reflectLayers = -1;

    private float clipPlaneOffset = -0.07f;

    private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
    private int m_OldReflectionTextureSize;

    private RenderTexture reflectionTex;

    /// <summary>
    /// Calculates the Y-value of the water-heightfield at the given X- and Z-values of a position in world space.
    /// </summary>
    /// <param name="planeMesh">reflection plane</param> 
    /// <param name="cPO">current clipPlaneOffset</param> 
    public void renderReflection(Mesh planeMesh, float cPO)
    {
        Mesh oldMesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshFilter>().mesh = planeMesh;

        clipPlaneOffset = cPO;

        Camera cam = Camera.current;
        if (!cam)
        {
            return;
        }

        Camera reflectionCamera;
        CreateWaterObjects(cam, out reflectionCamera);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        UpdateCameraModes(cam, reflectionCamera);

        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

        // Set custom culling matrix from the current camera
        reflectionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;

        reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value; // never render water layer
        reflectionCamera.targetTexture = reflectionTex;
        bool oldCulling = GL.invertCulling;
        GL.invertCulling = !oldCulling;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        GL.invertCulling = oldCulling;
        GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", reflectionTex);
        GetComponent<MeshFilter>().mesh = oldMesh;
    }
    void OnDisable()
    {
        foreach (var kvp in m_ReflectionCameras)
        {
            DestroyImmediate((kvp.Value).gameObject);
        }
        m_ReflectionCameras.Clear();
    }

    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
        {
            return;
        }
        // set water camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent<Skybox>();
            Skybox mysky = dest.GetComponent<Skybox>();
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }
        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    // On-demand create any objects we need for water
    private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;

        // Reflection render texture
        if (!reflectionTex || m_OldReflectionTextureSize != textureSize)
        {
            if (reflectionTex)
            {
                DestroyImmediate(reflectionTex);
            }
            reflectionTex = new RenderTexture(textureSize, textureSize, 16);
            reflectionTex.name = "__WaterReflection" + GetInstanceID();
            reflectionTex.isPowerOfTwo = true;
            reflectionTex.hideFlags = HideFlags.DontSave;
            m_OldReflectionTextureSize = textureSize;
        }

        // Camera for reflection
        m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
        if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectionCameras[currentCamera] = reflectionCamera;
        }
    }

    static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

}
