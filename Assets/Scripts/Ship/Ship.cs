﻿using System;
using MastersOfTempest.Networking;
using UnityEngine;


namespace MastersOfTempest.ShipBL
{
    [RequireComponent(typeof(ForceManilpulator))]
    public class Ship : NetworkBehaviour
    {
        private Gamemaster context;
        private ForceManilpulator forceManipulator;
        private ShipPartManager shipPartManager;
        private ShipTornadoInteraction shipTornInteraction;
        private ShipStatus currentStatus;

        private void Awake()
        {
            forceManipulator = GetComponent<ForceManilpulator>();
            if (forceManipulator == null)
            {
                throw new InvalidOperationException($"{nameof(ForceManilpulator)} is not specified!");
            }
            shipPartManager = GetComponent<ShipPartManager>();
            if (shipPartManager == null)
            {
                throw new InvalidOperationException($"{nameof(ShipPartManager)} is not specified!");
            }
        }

        protected override void Start()
        {
            base.Start();
            //both client and server versions need the context object
            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Ship)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
            currentStatus = new ShipStatus();
            currentStatus.Condition = ShipCondition.None;
        }

        public ForceManilpulator GetShipForceManipulator()
        {
            return forceManipulator;
        }

        public ShipStatus GetCurrenStatus()
        {
            return currentStatus;
        }
    }
}
