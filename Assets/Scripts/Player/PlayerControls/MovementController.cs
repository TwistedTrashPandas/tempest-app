using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class MovementController : MonoBehaviour
    {
        public Camera DirectionCamera;


        private const float speed = .1f;
        private CharacterPositionManipulator positionManipulator;
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
            }
        }

        private void Start()
        {
            positionManipulator = GetComponent<CharacterPositionManipulator>();
            if (positionManipulator == null)
            {
                throw new InvalidOperationException($"{nameof(positionManipulator)} is not specified!");
            }
        }
        private void FixedUpdate()
        {
            if (isActive)
            {
                if (!(Mathf.Approximately(Input.GetAxis("Vertical"), 0f) && Mathf.Approximately(Input.GetAxis("Horizontal"), 0f)))
                {
                    positionManipulator.MoveCharacter(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), DirectionCamera.transform.forward, DirectionCamera.transform.right);
                }
            }
        }
    }
}
