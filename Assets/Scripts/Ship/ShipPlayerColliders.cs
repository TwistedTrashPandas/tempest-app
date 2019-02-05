using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipPlayerColliders : MonoBehaviour
    {
        private Transform ship;

        void Start()
        {
            StartCoroutine(SearchForShip());
        }

        private IEnumerator SearchForShip()
        {
            Ship shipObj = null;
            while(shipObj == null)
            {
                shipObj = FindObjectsOfType<Ship>().FirstOrDefault(ship => ship.gameObject.scene == this.gameObject.scene);
                yield return null;
            }
            ship = shipObj.transform;
            if (!shipObj.GetComponent<ServerObject>().onServer)
            {
                transform.parent = ship;
                transform.position = ship.position;
                transform.rotation = ship.rotation;
                ship = null;
            }
        }

        private void Update()
        {
            if (ship != null)
            {
                this.gameObject.transform.rotation = ship.rotation;
                this.gameObject.transform.position = ship.position;
            }
        }
    }
}
