﻿using System;
using System.Collections;
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

        private const float PitchMax = 70f;
        private const float PitchMin = -60f;

        // camera movement parameters
        private const float maxMovementDistance = 2f;
        private const float durationFraction = 4f; // duration / durationFraction == camera movement after spell cast

        private bool isActive = true;
        public bool Active
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                if (value)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void Awake()
        {
            FirstPersonCamera = Camera.main;
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
            //Set parent to the camera so that it moves with the player
            FirstPersonCamera.transform.SetParent(this.transform, false);
            Active = true;
        }

        void Update()
        {
            if (Active)
            {
                yaw += speedH * Input.GetAxis("Mouse X");
                pitch -= speedV * Input.GetAxis("Mouse Y");
                pitch = Mathf.Clamp(pitch, PitchMin, PitchMax);
                FirstPersonCamera.transform.localEulerAngles = new Vector3(pitch + pitchShake, yaw + yawShake, rollShake);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Active ^= true;
            }
        }

        /// <summary>
        /// Shake the camera with the set intensity
        /// </summary>
        /// <param name="intensity">How strong the camera should be shaken, in range (0, 1]</param>
        public void ShakeCamera(float intensity)
        {
            StartCoroutine(ShakeCameraCoroutine(intensity));
        }

        private float pitchShake = 0f;
        private float yawShake = 0f;
        private float rollShake = 0f;
        private IEnumerator ShakeCameraCoroutine(float intensity)
        {
            const float ShakeDuration = .5f;
            const float ShakeDurationHalved = ShakeDuration / 2f;
            const float MinAngle = 5f;
            const float MaxAngle = 45f;
            //Choose randomly positive or negative angle change and distort a bit the intensity value
            float yawDisplacement = (UnityEngine.Random.value > .5f ? 1f : -1f)
                                    * (MinAngle + (MaxAngle - MinAngle) * (Mathf.Clamp01(intensity - UnityEngine.Random.value / 10f)));
            float pitchDisplacement = (UnityEngine.Random.value > .5f ? 1f : -1f)
                                    * (MinAngle + (MaxAngle - MinAngle) * (Mathf.Clamp01(intensity - UnityEngine.Random.value / 10f)));
            float rollDisplacement = (UnityEngine.Random.value > .5f ? 1f : -1f)
                                    * (MinAngle + (MaxAngle - MinAngle) * (Mathf.Clamp01(intensity - UnityEngine.Random.value / 10f)));

            float timeElapsed = 0f;

            while (timeElapsed < ShakeDurationHalved)
            {
                yield return null;
                float deltaTime = Time.deltaTime;
                timeElapsed += deltaTime;
                if (timeElapsed > ShakeDurationHalved)
                {
                    //We don't want to accumulate extra angle change over dropped frames
                    deltaTime -= timeElapsed - ShakeDurationHalved;
                }
                pitchShake += deltaTime * (pitchDisplacement / ShakeDurationHalved);
                rollShake += deltaTime * (rollDisplacement / ShakeDurationHalved);
                yawShake += deltaTime * (yawDisplacement / ShakeDurationHalved);
            }
            //Now go back to the normal state
            timeElapsed = 0f;
            while (timeElapsed < ShakeDurationHalved)
            {
                yield return null;
                float deltaTime = Time.deltaTime;
                timeElapsed += deltaTime;
                if (timeElapsed > ShakeDurationHalved)
                {
                    //We don't want to accumulate extra angle change over dropped frames
                    deltaTime -= timeElapsed - ShakeDurationHalved;
                }
                pitchShake -= deltaTime * (pitchDisplacement / ShakeDurationHalved);
                rollShake -= deltaTime * (rollDisplacement / ShakeDurationHalved);
                yawShake -= deltaTime * (yawDisplacement / ShakeDurationHalved);
            }
        }

        /// <summary>
        /// Move the camera with the set intensity
        /// </summary>
        /// <param name="direction">Direction the camera is moving, normalized</param>
        /// <param name="intensity">Intensity of the camera movement</param>
        public void MoveCamera(Vector3 direction, float intensity)
        {
            StartCoroutine(MoveCameraCoroutine(direction, intensity));
        }

        private IEnumerator MoveCameraCoroutine(Vector3 direction, float intensity)
        {
            Transform cameraTransform = FirstPersonCamera.transform;
            const float MoveDuration = 1.0f;
            const float MoveDurationFraction = MoveDuration / durationFraction;
            Vector3 localPosBefore = cameraTransform.localPosition;
            Vector3 localTargetPos = cameraTransform.InverseTransformPoint(cameraTransform.position + direction * intensity);
            float timeElapsed = 0f;

            while (timeElapsed < MoveDurationFraction)
            {
                yield return null;
                float deltaTime = Time.unscaledDeltaTime;
                timeElapsed += deltaTime;
                cameraTransform.transform.localPosition = Vector3.Slerp(localPosBefore, localTargetPos, timeElapsed / MoveDurationFraction);
            }
            timeElapsed = MoveDurationFraction;
            while (timeElapsed < MoveDuration)
            {
                yield return null;
                float deltaTime = Time.unscaledDeltaTime;
                timeElapsed += deltaTime;
                cameraTransform.transform.localPosition = Vector3.Slerp(localTargetPos, localPosBefore, (timeElapsed - MoveDurationFraction) / MoveDuration);
            }
            cameraTransform.transform.localPosition = localPosBefore;
        }
    }
}
