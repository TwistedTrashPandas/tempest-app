using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MastersOfTempest.ShipBL;

public class Hologram : MonoBehaviour
{
    public Transform target;
    public Camera hologramCamera;
    public RenderTexture hologramDepth;
    public RenderTexture hologramColor;

    public float distance = 20;

    public ShipPartManager shipPartManager;
    private Camera mainCamera;
    private bool renderTextureToggle = true;

    private void Start()
    {
        mainCamera = Camera.main;
        hologramCamera.enabled = false;
        shipPartManager = FindObjectOfType<ShipPartManager>();
    }

    private void Update ()
    {
        shipPartManager.ChangeShaderDestructionValue();
        
        // Orient the hologram that its always facing the main camera
        transform.LookAt(mainCamera.transform);
        transform.forward *= -1;

        // Orient the hologram camera relative to the way that the main camera are looking at the hologram
        Vector3 viewingDirection = (mainCamera.transform.position - transform.position).normalized;
        hologramCamera.transform.position = target.position + distance * viewingDirection;
        hologramCamera.transform.LookAt(target);

        // Switch render texture between depth and color
        renderTextureToggle = !renderTextureToggle;
        hologramCamera.targetTexture = renderTextureToggle ? hologramColor : hologramDepth;

        hologramCamera.Render();

        shipPartManager.ResetShaderDestructionValue();
    }
}
