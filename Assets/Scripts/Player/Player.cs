using System;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls.QTE;
using UnityEngine;

namespace MastersOfTempest
{
    public class Player : NetworkBehaviour
    {
        /*TODO:
            Think about avatar objects for players. Most probably we will need to tell server 
            to spawn an avatar for the other players.
         */
        private Gamemaster context;
        private PlayerInputController playerInput;

        protected override void Start()
        {
            base.Start();

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
            playerInput.Bootstrap();
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
