using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipPartManager : MonoBehaviour
    {
        private ShipPart[] shipparts;
        private List<int>[] interactionAreas;
        void Start()
        {
            interactionAreas = new List<int>[5];
            for (int i = 0; i < interactionAreas.Length; i++)
            {
                interactionAreas[i] = new List<int>();
            }
            shipparts = GetComponentsInChildren<ShipPart>();
            if (shipparts == null)
                throw new System.InvalidOperationException("Ship has to have at least one shippart attached");
            else
            {
                for (int i = 0; i < shipparts.Length; i++)
                {
                    interactionAreas[shipparts[i].interactionArea].Insert(0, i); // Add(i);
                }
            }
        }

        private void FixedUpdate()
        {
            CheckDestruction();
        }

        void CheckDestruction()
        {
            Debug.Log(CalculateOverallDestruction());
        }

        public float CalculateOverallDestruction()
        {
            float result = 0.0f;
            for (int i = 0; i < shipparts.Length; i++)
            {
                result += shipparts[i].GetDestruction();
            }
            return result / shipparts.Length;
        }
    }
}