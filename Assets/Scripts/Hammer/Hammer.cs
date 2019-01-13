using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    public Transform top;
    public Transform center;
    [Range(0, 1)]
    public float charge = 0;
    [SerializeField]
    private float scaleSmall;
    [SerializeField]
    private float scaleLarge;
    [SerializeField]
    private float smallPosY;
    [SerializeField]
    private float largePosY;

    private Collider collider;

    protected void Start()
    {
        collider = GetComponent<Collider>();
    }

    protected void Update()
    {
        top.localPosition = new Vector3(0, (1.0f - charge) * smallPosY + charge * largePosY, 0);
        top.localScale = ((1.0f - charge) * scaleSmall + charge * scaleLarge) * Vector3.one;
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
                    e.DamageRockOnServer(collision.gameObject.GetComponentInParent<ServerObject>().serverID, 0.1f + charge);
                    charge = 0;
                    break;
                }
            }
        }
    }
}
