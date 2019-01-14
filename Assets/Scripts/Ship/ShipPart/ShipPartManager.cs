using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipPartManager : MonoBehaviour
    {
        public event EventHandler ActionRequest;
        public Dictionary<ShipPartArea, List<ShipPart>> interactionAreas { get; private set; }

        void Start()
        {
            interactionAreas = new Dictionary<ShipPartArea, List<ShipPart>>();
            // create different interaction areas, shipparts still have to be added according to its enum value
            foreach (ShipPartArea area in Enum.GetValues(typeof(ShipPartArea)))
            {
                interactionAreas.Add(area, new List<ShipPart>());
            }

            ShipPart[] shipparts = GetComponentsInChildren<ShipPart>();
            if (shipparts == null)
                throw new System.InvalidOperationException("Ship has to have at least one ship part attached");
            else
            {
                for (int i = 0; i < shipparts.Length; i++)
                {
                    interactionAreas[shipparts[i].interactionArea].Add(shipparts[i]); // Add(i);
                    shipparts[i].ShipPartHit += OnShipPartHit;
                }
            }
        }

        // test output
        private void FixedUpdate()
        {
            //CheckDestruction();
        }

        void CheckDestruction()
        {
            Debug.Log(CalculateOverallDestruction());
        }

        // calculates average of the destruction of the ship (health = 1 - destruction)
        public float[] CalculateOverallDestruction()
        {
            float result = 0.0f;
            int num = 0;
            float[] out_arr = new float[interactionAreas.Values.Count];
            int j = 0;
            foreach (List<ShipPart> partList in interactionAreas.Values)
            {
                result = 0f;
                num = partList.Count;
                for (int i = 0; i < partList.Count; i++)
                {
                    result += partList[i].GetDestruction();
                }
                if (num != 0)
                    out_arr[j] = result / num;
                else
                    out_arr[0] = 0f;
                j++;
            }
            return out_arr;
        }

        public void ChangeShaderDestructionValue()
        {
            foreach (List<ShipPart> partList in interactionAreas.Values)
            {
                for (int i = 0; i < partList.Count; i++)
                {
                    partList[i].ChangeShaderDestructionValue();
                }
            }
        }

        public void ResetShaderDestructionValue()
        {
            foreach (List<ShipPart> partList in interactionAreas.Values)
            {
                for (int i = 0; i < partList.Count; i++)
                {
                    partList[i].ResetShaderDestructionValue();
                }
            }
        }

        private void OnShipPartHit(object sender, EventArgs args)
        {
            var origin = (ShipPart)sender;
            //var damage = origin.status == ShipPartStatus.Fragile ? 1f : ((ShipPartHitEventArgs)args).damageAmount;
            //We can later use another action, e.g. ShipPart hit to also add the sound of the impact
            ActionRequest?.Invoke(this, new ActionMadeEventArgs(new ShakeCameraAction(((ShipPartHitEventArgs)args).damageAmount, origin.transform.position)));
        }
    }
}
