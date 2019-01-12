using System;
using MastersOfTempest.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        private struct RepairShipPartAreaMessage
        {
            public ShipPartArea shipPartArea;
            public float repairAmount;
        }

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
            // Switch active scene so that instantiate creates the object as part of the client scene       
            Scene previouslyActiveScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(gameObject.scene);
            //both client and server versions need the context object
            context = FindObjectOfType<Gamemaster>();

            // Switch back to the previously active scene  
            SceneManager.SetActiveScene(previouslyActiveScene);

            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Ship)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
            currentStatus = new ShipStatus();
            currentStatus.Condition = ShipCondition.None;
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            RepairShipPartAreaMessage message = ByteSerializer.FromBytes<RepairShipPartAreaMessage>(data);

            foreach (ShipPart shipPart in shipPartManager.interactionAreas[message.shipPartArea])
            {
                // Negative destruction equals repairing
                shipPart.AddDestruction(-message.repairAmount);
            }
        }

        public void RepairShipPartAreaOnServer (ShipPartArea shipPartArea, float repairAmount)
        {
            if (serverObject.onServer)
            {
                Debug.LogError(nameof(RepairShipPartAreaOnServer) + " should not be called on the server!");
            }
            else
            {
                RepairShipPartAreaMessage message = new RepairShipPartAreaMessage();
                message.shipPartArea = shipPartArea;
                message.repairAmount = repairAmount;
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
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
