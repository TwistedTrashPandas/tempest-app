using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public Transform top;
    public float damage = 0.5f;

    private Collider collider;

    protected void Start()
    {
        collider = GetComponent<Collider>();
    }

    public void EnableCollider (bool enable)
    {
        collider.enabled = enable;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hammer colliding with " + collision.gameObject.name);

        if (collision.transform.tag.Equals("Rock"))
        {
            // Damage the rock from the environment manager
            EnvironmentManager[] environmentManagers = FindObjectsOfType<EnvironmentManager>();

            foreach (EnvironmentManager e in environmentManagers)
            {
                if (e.gameObject.scene.Equals(GameClient.Instance.gameObject.scene))
                {
                    e.DamageRockOnServer(collision.gameObject.GetComponent<ServerObject>().serverID, damage);
                    break;
                }
            }
        }
    }
}
