using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipPartManager : MonoBehaviour
    {
        private Dictionary<ShipPartArea, List<ShipPart>> interactionAreas;

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
        public float CalculateOverallDestruction()
        {
            float result = 0.0f;
            int num = 0;
            foreach (List<ShipPart> partList in interactionAreas.Values)
            {
                num += partList.Count;
                for (int i = 0; i < partList.Count; i++)
                {
                    result += partList[i].GetDestruction();
                }
            }
            return result / num;
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
    }
}