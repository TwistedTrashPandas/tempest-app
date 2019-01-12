using System;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls.QTE;
using UnityEngine;
using static Facepunch.Steamworks.Networking;

namespace MastersOfTempest
{
    [RequireComponent(typeof(CharacterPositionManipulator))]
    public class Player : NetworkBehaviour
    {
        [Serializable]
        private struct PlayerMessage
        {
            public ulong playerId;
        }

        public ulong PlayerId { get; set; }
        private Gamemaster context;
        private PlayerInputController playerInput;
        public CharacterPositionManipulator CharacterPositionManipulator { get; private set; }

        public CharacterController CharacterController { get; private set; }

        public PlayerRole Role;

        private void Awake()
        {
            CharacterPositionManipulator = GetComponent<CharacterPositionManipulator>();
            if (CharacterPositionManipulator == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterPositionManipulator)} is not specified!");
            }
            CharacterController = GetComponent<CharacterController>();
            if (CharacterController == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterController)} is not specified!");
            }
        }

        protected override void Start()
        {
            base.Start();
            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Player)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
            CharacterController = GetComponent<CharacterController>();
            if(serverObject.onServer)
            {
                CharacterPositionManipulator.Character = CharacterController;
            }
            else
            {
                Destroy(CharacterController);
            }
        }

        protected override void StartServer()
        {
            if (PlayerId == default(ulong))
            {
                throw new InvalidOperationException($"{nameof(Player)} should have initialized PlayerId value!");
            }

            var message = new PlayerMessage { playerId = PlayerId };
            SendToAllClients(ByteSerializer.GetBytes(message), SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            //Initialize player id
            var message = ByteSerializer.FromBytes<PlayerMessage>(data);
            PlayerId = message.playerId;
            //Set controls if this Player belongs to this client
            if (Facepunch.Steamworks.Client.Instance.SteamId == PlayerId)
            {
                playerInput = PlayerRoleExtensions.AddActiveRoleInputController(gameObject);
                playerInput.Bootstrap();
                playerInput.ActionMade += ExecutePlayerAction;
                context.SetCurrentPlayer(this);
            }
        }

        private void ExecutePlayerAction(object sender, EventArgs e)
        {
            ((ActionMadeEventArgs)e).Action.Execute(context);
        }

        protected override void OnDestroyClient()
        {
            base.OnDestroyClient();
            playerInput.ActionMade -= ExecutePlayerAction;
        }
    }
}
