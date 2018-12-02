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
            //todo: different initializations for different roles!
            //InitializeClientObjectWithOperatorControls();
            InitializeClientObjectWithNavigatorControls();
        }


        void ExecutePlayerAction(object sender, EventArgs e)
        {
            ((ActionMadeEventArgs)e).Action.Execute(context);
        }

        private void InitializeClientObjectWithOperatorControls()
        {
            var input = gameObject.AddComponent<SimpleInput>();
            var qteDriver = gameObject.AddComponent<QTEDriver>();
            input.FirstPersonCamera = FirstPersonCamera;
            input.QTEDriver = qteDriver;
            input.ActionMade += ExecutePlayerAction;
            playerInput = input;

            var qteRenderer = gameObject.AddComponent<QTESimpleUIRenderer>();
            qteRenderer.Driver = qteDriver;
            qteRenderer.InfoForUser = Text;
        }

        private void InitializeClientObjectWithNavigatorControls()
        {
            var input = gameObject.AddComponent<ApprenticeInput>();
            var qteDriver = gameObject.AddComponent<QTEDriver>();
            input.FirstPersonCamera = FirstPersonCamera;
            input.InteractionMessage = Text;
            input.QTEDriver = qteDriver;
            input.ActionMade += ExecutePlayerAction;
            playerInput = input;

            var qteRenderer = gameObject.AddComponent<QTESimpleUIRenderer>();
            qteRenderer.Driver = qteDriver;
            qteRenderer.InfoForUser = Text;
        }

        protected override void OnDestroyClient()
        {
            base.OnDestroyClient();
            playerInput.ActionMade -= ExecutePlayerAction;
        }
    }
}
