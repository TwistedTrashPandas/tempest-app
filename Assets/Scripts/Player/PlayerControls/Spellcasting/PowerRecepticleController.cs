using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    [RequireComponent(typeof(PowerRecepticle))]
    public class PowerRecepticleController : NetworkBehaviour
    {
        public Charge CurrentCharge { get; private set; }
        private PowerRecepticle powerRecepticle;
        private struct ChargeMessage
        {
            public ChargeMessage(Charge charge, bool destabilise = false)
            {
                ChargeType = charge;
                Destabilising = destabilise;
            }
            public Charge ChargeType;
            /// <summary>
            /// Used to signal the client that the charge is destabilising and is about to expire
            /// </summary>
            public bool Destabilising;
        }
        /// <summary>
        /// How much in advance do we tell the player that the charge is about to expire
        /// </summary>
        private const float TimeToRemindOfDecay = 2f;
        private CoroutineCancellationToken globalCancellationToken;

        protected void Awake()
        {
            powerRecepticle = GetComponent<PowerRecepticle>();
            if (powerRecepticle == null)
            {
                throw new InvalidOperationException($"{nameof(powerRecepticle)} is not specified!");
            }
        }

        public void SetCharge(Charge chargeType)
        {
            if (serverObject.onServer)
            {
                CurrentCharge = chargeType;
                if (globalCancellationToken != null)
                {
                    globalCancellationToken.CancellationRequested = true;
                }
                globalCancellationToken = new CoroutineCancellationToken();
                StartCoroutine(ChargeDecay(globalCancellationToken));
                SendToAllClients(ByteSerializer.GetBytes(new ChargeMessage(chargeType)), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
            else
            {
                SendToServer(ByteSerializer.GetBytes(new ChargeMessage(chargeType)), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var msg = ByteSerializer.FromBytes<ChargeMessage>(data);
            if (msg.Destabilising)
            {
                powerRecepticle.Destabilise();
            }
            else
            {
                #pragma warning disable 618
                powerRecepticle.Charge(msg.ChargeType);
                #pragma warning restore 618
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var msg = ByteSerializer.FromBytes<ChargeMessage>(data);
            SetCharge(msg.ChargeType);
        }

        private IEnumerator ChargeDecay(CoroutineCancellationToken cancellationToken)
        {
            if (!serverObject.onServer)
            {
                throw new InvalidOperationException("Charge decay logic should execute only on the server!");
            }
            /*
                Should charge decay with constant speed?
                Options:
                    - Constant
                    - Normal distribution
                    - Increasing intervals

                For now we use constant timer, might be improved later.
             */
            var expireTime = DecayTime;
            bool warned = false;
            float timeSinceLastCharge = 0f;
            while (timeSinceLastCharge < expireTime && !cancellationToken.CancellationRequested)
            {
                yield return null;
                timeSinceLastCharge += Time.deltaTime;
                if (!warned && (expireTime - timeSinceLastCharge) < TimeToRemindOfDecay)
                {
                    warned = true;
                    SendToAllClients(ByteSerializer.GetBytes(new ChargeMessage(CurrentCharge, true)), Facepunch.Steamworks.Networking.SendType.Reliable);
                }
            }
            if (!cancellationToken.CancellationRequested)
            {
                globalCancellationToken = null;
                SetCharge(Charge.None);
            }
            //If cancellation was requested we just exit the coroutine and don't do anything
        }

        /// <summary>
        /// Returns how much the charge will last.
        /// Property can be modified later to return more interesting values,
        /// e.g. following normal distribution
        /// </summary>
        /// <value></value>
        private float DecayTime
        {
            get
            {
                //TODO: tweak this value
                return 120f;
            }
        }
    }
}
