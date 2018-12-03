using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Controller that makes the camera to follow mouse/joystick movement
    /// </summary>
    public class CameraDirectionController : MonoBehaviour
    {
        public Camera FirstPersonCamera;

        public float speedH = 2.0f;
        public float speedV = 2.0f;

        private float yaw = 0.0f;
        private float pitch = 0.0f;

        public bool Active { get; set; } = true;

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
