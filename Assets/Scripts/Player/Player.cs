using System;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls.QTE;
using UnityEngine;

namespace MastersOfTempest
{
    public class Player : NetworkBehaviour
    {
        //TODO: temporal solution, implement better UI management in future revisions
        public UnityEngine.UI.Text Text;
        public Camera FirstPersonCamera;
        private Gamemaster context;
        private PlayerInputController playerInput;

        protected override void Start()
        {
            base.Start();
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
            if (Text == null)
            {
                throw new InvalidOperationException($"{nameof(Text)} is not specified!");
            }

            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Player)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
        }

        protected override void StartClient()
        {
            base.StartClient();
            //Initialize player controllers based on the active role
            playerInput = PlayerRoleExtensions.AddActiveRoleInputController(gameObject);
            playerInput.Bootstrap(this);
            playerInput.ActionMade += ExecutePlayerAction;
        }

        void ExecutePlayerAction(object sender, EventArgs e)
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
