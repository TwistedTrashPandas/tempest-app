using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipPlayerColliders : MonoBehaviour
    {
        private Transform ship;
        private Vector3 lastPos;
        private Quaternion lastRot;
        // Start is called before the first frame update
        void Start()
        {
            ship = GameObject.Find("Ship").GetComponent<Transform>();
            lastRot = ship.transform.rotation;
            lastPos = ship.transform.position;
            this.gameObject.transform.position = ship.transform.position;
            this.gameObject.transform.rotation = ship.transform.rotation;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //this.gameObject.transform.rotation = ship.rotation;
            //this.gameObject.transform.position = ship.position;
        }
        private void Update()
        {
            this.gameObject.transform.rotation = ship.rotation;
            this.gameObject.transform.position = ship.position;
        }
        private void LateUpdate()
        {
            //this.gameObject.transform.rotation = ship.rotation;
            this.gameObject.transform.position = -ship.position;
        }
    }
}
