using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls 
{
    public class SpectatorInput : PlayerInputController
    {
        private bool isActive = true;
        private float defaultSpeed = 3f;
        private Camera FirstPersonCamera;
        private void Update()
        {
            if(isActive)
            {
                var speed = defaultSpeed;
                Vector3 movement = Vector3.zero;
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    speed *= 5;
                }
                if(Input.GetKey(KeyCode.W))
                {
                    movement += FirstPersonCamera.transform.forward * speed * Time.deltaTime;
                }
                else if(Input.GetKey(KeyCode.S))
                {
                    movement += -FirstPersonCamera.transform.forward * speed * Time.deltaTime;
                }
                if(Input.GetKey(KeyCode.D))
                {
                    movement += FirstPersonCamera.transform.right * speed * Time.deltaTime;
                }
                else if(Input.GetKey(KeyCode.A))
                {
                    movement += -FirstPersonCamera.transform.right * speed * Time.deltaTime;
                }
                if(Input.GetKey(KeyCode.Space))
                {
                    movement += FirstPersonCamera.transform.up * speed * Time.deltaTime;
                }
                else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    movement += -FirstPersonCamera.transform.up * speed * Time.deltaTime;
                }
                FirstPersonCamera.transform.position += movement;
            }
        }

        public override void Bootstrap()
        {
            FirstPersonCamera = CameraDirectionController.FirstPersonCamera;
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
            //We have custom WASD movement on the spectator
            MovementController.Active = false;
        }

        public override void Interrupt()
        {
            //We cannot interrupt specatator
        }

        public override void Resume()
        {
            isActive = true;
        }

        public override void Suppress()
        {
            isActive = false;
        }
    }
}
