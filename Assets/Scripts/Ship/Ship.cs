using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MastersOfTempest
{
    [RequireComponent(typeof(ShipManipulator))]
    public class Ship : MonoBehaviour
    {
        private Gamemaster context;
        private ShipManipulator shipManipulator;

        private void Awake()
        {
            shipManipulator = GetComponent<ShipManipulator>();
            if (shipManipulator == null)
            {
                throw new InvalidOperationException($"{nameof(ShipManipulator)} is not specified!");
            }
        }

        private void Start()
        {
            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Ship)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
        }

        public ShipManipulator GetShipManipulator()
        {
            return shipManipulator;
        }
    }
}
