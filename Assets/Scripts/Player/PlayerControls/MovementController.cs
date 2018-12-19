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
        private TransformManipulator transformManipulator;
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
            transformManipulator = GetComponent<TransformManipulator>();
            if (transformManipulator == null)
            {
                throw new InvalidOperationException($"{nameof(transformManipulator)} is not specified!");
            }
        }
        private void FixedUpdate()
        {
            if(isActive)
            {
                var position = transform.position;
                position += speed * (Input.GetAxis("Horizontal") * DirectionCamera.transform.right + Input.GetAxis("Vertical") * DirectionCamera.transform.forward);
                if(position != transform.position)
                {
                    transformManipulator.ChangePosition(position);
                }
            }
        }
    }
}
