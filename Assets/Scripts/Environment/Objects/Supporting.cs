using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Supporting : EnvObject
    {
        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {

            }
        }
    }
}