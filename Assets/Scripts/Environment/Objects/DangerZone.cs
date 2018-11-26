using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest
{
    namespace Environment
    {
        namespace Interacting
        {
            public class DangerZone : EnvObject
            {
                protected override void OnCollisionEnter(Collision collision)
                {
                    if (collision.gameObject.tag == "Player")
                    {
                        // TODO: collision
                    }
                }
            }
        }
    }
}
