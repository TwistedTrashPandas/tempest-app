using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class InteractionsController : MonoBehaviour
    {
        public event EventHandler NewInteractable;
        public event EventHandler LostSight;
        public event EventHandler PlayerInteracted;

        private const string InteractableTagName = "Interactable";

        public Camera FirstPersonCamera;
        public float MaxInteractionDistance;

        public Func<bool> PlayerInteractionCheck;

        private bool isActive;
        public bool Active
        {
            get
            {
                return isActive && PlayerInteractionCheck != null;
            }
            set
            {
                isActive = value;
            }
        }

        public InteractablePart CurrentlyLookedAt { get; private set; }

        public void Setup(Camera cameraToShootFrom, float maxInteractionDistance, Func<bool> interactionCheck)
        {
            FirstPersonCamera = cameraToShootFrom;
            MaxInteractionDistance = maxInteractionDistance;
            PlayerInteractionCheck = interactionCheck;
            isActive = true;
        }

        private void Start()
        {
            if (FirstPersonCamera == null)
            {
                throw new InvalidOperationException($"{nameof(FirstPersonCamera)} is not specified!");
            }
        }

        private bool hasObjectReadyToInteract;
        private void Update()
        {
            if (Active)
            {
                RaycastHit hit;
                var ray = FirstPersonCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
                if (Physics.Raycast(ray, out hit, MaxInteractionDistance))
                {
                    if (hit.transform.CompareTag(InteractableTagName))
                    {
                        //Encountered new object
                        if (CurrentlyLookedAt?.transform != hit.transform)
                        {
                            if (hasObjectReadyToInteract)
                            {
                                LostSightOfAnObject();
                            }
                            CurrentlyLookedAt = hit.transform.GetComponent<InteractablePart>();
                        }
                        //Object is within interactable distance
                        if (hit.distance < CurrentlyLookedAt.GetDistance())
                        {
                            //The first time we came to the interactable distance
                            if (!hasObjectReadyToInteract)
                            {
                                LookingAtNewObject();
                            }
                        } //Object is out of its interactable distance
                        else
                        {
                            //The first time we went out of interactable distance
                            if (hasObjectReadyToInteract)
                            {
                                LostSightOfAnObject();
                            }
                        }
                        //At this point hasObjectReadyToInteract will be true only if there is and object
                        //within its interactable distance in our line of sight. Check for interaction from player
                        if (hasObjectReadyToInteract && PlayerInteractionCheck())
                        {
                            PlayerInteracted?.Invoke(this, new InteractionEventArgs(CurrentlyLookedAt));
                        }
                    }
                    else //Not looking at object with interactable tag
                    {
                        if (hasObjectReadyToInteract)
                        {
                            LostSightOfAnObject();
                            CurrentlyLookedAt = null;
                        }
                    }
                } 
                else //Not looking at anything
                {
                    if (hasObjectReadyToInteract)
                    {
                        LostSightOfAnObject();
                        CurrentlyLookedAt = null;
                    }
                }
            }
        }

        private void LookingAtNewObject()
        {
            hasObjectReadyToInteract = true;
            NewInteractable?.Invoke(this, new InteractionEventArgs(CurrentlyLookedAt));
        }
        private void LostSightOfAnObject()
        {
            hasObjectReadyToInteract = false;
            LostSight?.Invoke(this, new InteractionEventArgs(CurrentlyLookedAt));
        }
    }
}
