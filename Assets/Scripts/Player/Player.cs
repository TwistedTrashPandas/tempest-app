using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.PlayerControls;

namespace MastersOfTempest
{
    [RequireComponent(typeof(PlayerInputController))]
    public class Player : MonoBehaviour
    {
        private Gamemaster context;
        private PlayerInputController playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInputController>();

            SanityCheck();
        }

        private void OnEnable()
        {
            playerInput.ActionMade += ExecutePlayerAction;
        }

        void ExecutePlayerAction(object sender, EventArgs e)
        {
            ((ActionMadeEventArgs)e).Action.Execute(context);
        }

        private void Start()
        {
            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Player)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
        }

        /// <summary>
        /// Check that the behaviour has everything it needs to work properly
        /// </summary>
        private void SanityCheck()
        {
            if (playerInput == null)
            {
                throw new InvalidOperationException($"{nameof(playerInput)} is not specified!");
            }
        }
    }
}
