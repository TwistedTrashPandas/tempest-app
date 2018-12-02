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

        private void Update()
        {
            if(isActive)
            {
                var speed = defaultSpeed;
                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    speed *= 5;
                }
                if(Input.GetKey(KeyCode.W))
                {
                    transform.Translate(FirstPersonCamera.transform.forward * speed * Time.deltaTime);
                }
                else if(Input.GetKey(KeyCode.S))
                {
                    transform.Translate(-FirstPersonCamera.transform.forward * speed * Time.deltaTime);
                }
                if(Input.GetKey(KeyCode.D))
                {
                    transform.Translate(FirstPersonCamera.transform.right * speed * Time.deltaTime);
                }
                else if(Input.GetKey(KeyCode.A))
                {
                    transform.Translate(-FirstPersonCamera.transform.right * speed * Time.deltaTime);
                }
                if(Input.GetKey(KeyCode.Space))
                {
                    transform.Translate(FirstPersonCamera.transform.up * speed * Time.deltaTime);
                }
                else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    transform.Translate(-FirstPersonCamera.transform.up * speed * Time.deltaTime);
                }
            }
        }

        public override void Bootstrap(Player player)
        {
            base.Bootstrap(player);
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
