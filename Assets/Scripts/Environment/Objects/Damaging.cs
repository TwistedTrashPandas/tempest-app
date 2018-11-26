using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Damaging : EnvObject
    {
        public Damaging(float sp) : base(sp)
        {

        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                // TODO: collision
            }
        }
    }
}
