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
            StartCoroutine(ListenMovement());
        }

        private IEnumerator ListenMovement()
        {
            while (true)
            {
                if (isActive)
                {
                    positionManipulator.MoveCharacter(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), DirectionCamera.transform.forward, DirectionCamera.transform.right);
                }
                yield return new WaitForSeconds(1f / 10f);
            }
        }
    }
}
