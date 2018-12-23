using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Glow;

namespace MastersOfTempest.PlayerControls
{
    public class InteractionsHighlighter : MonoBehaviour
    {
        public InteractionsController InteractionsController;

        private void Start()
        {
            if (InteractionsController == null)
            {
                throw new InvalidOperationException($"{nameof(InteractionsController)} is not specified!");
            }

            InteractionsController.NewInteractable += OnNewObjectInSight;
            InteractionsController.LostSight += OnObjectLostSight;
        }

        public void OnNewObjectInSight(object sender, EventArgs args)
        {
            var glowable = ((InteractionEventArgs)args).InteractableObject.GetComponent<GlowObjectCmd>();
            glowable?.TurnTheGlowOn();
        }

        public void OnObjectLostSight(object sender, EventArgs args)
        {
            var glowable = ((InteractionEventArgs)args).InteractableObject.GetComponent<GlowObjectCmd>();
            glowable?.TurnTheGlowOff();
        }
    }
}
