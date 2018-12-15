using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Hologram : MonoBehaviour
{
    public Transform target;
    public Camera hologramCamera;

    public float distance = 20;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update ()
    {
        transform.LookAt(mainCamera.transform);

        Vector3 viewingDirection = (mainCamera.transform.position - transform.position).normalized;
        hologramCamera.transform.position = target.position + distance * viewingDirection;
        hologramCamera.transform.LookAt(target);
	}
}
