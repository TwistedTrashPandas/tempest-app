using System;
using System.Linq;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MastersOfTempest.ShipBL
{
    [RequireComponent(typeof(ForceManilpulator))]
    public class Ship : NetworkBehaviour
    {
        private const float freezingSlowDown = 0.25f;
        private Gamemaster context;
        private ForceManilpulator forceManipulator;
        private ShipPartManager shipPartManager;
        private ShipTornadoInteraction shipTornInteraction;
        private ShipStatus currentStatus;
        private Quaternion lastRotation;

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
            shipPartManager.ActionRequest += ExecuteAction;
        }

        protected override void Start()
        {
            base.Start();
            context = FindObjectsOfType<Gamemaster>().First(gm => gm.gameObject.scene == gameObject.scene);
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Ship)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
            currentStatus = new ShipStatus();
            lastRotation = this.transform.rotation;
            currentStatus.ActionRequest += ExecuteAction;
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            RepairShipPartAreaMessage message = ByteSerializer.FromBytes<RepairShipPartAreaMessage>(data);
            RepairShipPartAreaOnServer(message.shipPartArea, message.repairAmount);
        }

        public float GetFreezingSlowDown()
        {
            return freezingSlowDown;
        }

        public void RepairShipPartAreaOnServer(ShipPartArea shipPartArea, float repairAmount)
        {
            if (serverObject.onServer)
            {
                float maxDestruct = -1f;
                ShipPart toRepair = new ShipPart();
                foreach (ShipPart shipPart in shipPartManager.interactionAreas[shipPartArea])
                {
                    // Negative destruction equals repairing
                    if (shipPart.GetDestruction() > maxDestruct)
                    {
                        maxDestruct = shipPart.GetDestruction();
                        toRepair = shipPart;
                    }
                    //shipPart.AddDestruction(-repairAmount);
                }
                toRepair.AddDestruction(-repairAmount);
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

        public ShipPartManager GetShipPartManager()
        {
            return shipPartManager;
        }

        private void ExecuteAction(object sender, EventArgs args)
        {
            var action = ((ActionMadeEventArgs)args).Action;
            action.Execute(context);
        }

        public void StoreRotation()
        {
            lastRotation = this.transform.rotation;
        }

        public Quaternion GetLastRotation()
        {
            return lastRotation;
        }

        public void DestroyShip()
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].GetComponent<ServerObject>() != null)
                {
                    children[i].parent = null;
                    children[i].gameObject.layer = LayerMask.NameToLayer("Server");
                    children[i].gameObject.AddComponent<Rigidbody>();
                    children[i].gameObject.GetComponent<Rigidbody>().useGravity = false;
                    // children[i].gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) * 0.1f);
                    children[i].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

                    if (children[i].GetComponent<Collider>() == null)
                    {
                        children[i].gameObject.AddComponent<MeshCollider>();
                        children[i].gameObject.GetComponent<MeshCollider>().convex = true;
                    }
                }
            }
        }
    }
}
