using System;
using MastersOfTempest.PlayerControls;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls.QTE;

namespace MastersOfTempest
{
    public class Player : NetworkBehaviour
    {
        //TODO: temporal solution, implement better UI management in future revisions
        public UnityEngine.UI.Text Text;
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
            //todo: different initializations for different roles!
            InitializeClientObjectWithOperatorControls();
        }


        void ExecutePlayerAction(object sender, EventArgs e)
        {
            ((ActionMadeEventArgs)e).Action.Execute(context);
        }

        private void InitializeClientObjectWithOperatorControls()
        {
            var input = gameObject.AddComponent<SimpleInput>();
            var qteDriver = gameObject.AddComponent<QTEDriver>();
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
