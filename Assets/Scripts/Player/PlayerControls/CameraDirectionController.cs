using System;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Controller that makes the camera to follow mouse/joystick movement
    /// </summary>
    public class CameraDirectionController : MonoBehaviour
    {
        public Camera FirstPersonCamera { get; private set; }
        public float speedH = 2.0f;
        public float speedV = 2.0f;

        private float yaw = 0.0f;
        private float pitch = 0.0f;

        public bool Active { get; set; } = true;

        private void Awake()
        {
            FirstPersonCamera = Camera.main;
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
            //Set parent to the camera so that it moves with the player
            FirstPersonCamera.transform.SetParent(this.transform, false);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Active)
            {
                yaw += speedH * Input.GetAxis("Mouse X");
                pitch -= speedV * Input.GetAxis("Mouse Y");

                FirstPersonCamera.transform.localEulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
        }
    }
}
