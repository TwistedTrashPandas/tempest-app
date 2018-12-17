using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgoCamera : MonoBehaviour
{
    float speed = 0.1f;

	void Update ()
    {
        Vector3 rotation = transform.localRotation.eulerAngles;
        rotation.x -= Input.GetAxis("Mouse Y");
        rotation.y += Input.GetAxis("Mouse X");
        transform.localRotation = Quaternion.Euler(rotation);

        transform.position += speed * (Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward);
	}
}
