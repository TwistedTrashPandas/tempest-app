using System.Collections;
using UnityEngine;

namespace MastersOfTempest
{
    [RequireComponent(typeof(Rigidbody))]
    public class ShipManipulator : MonoBehaviour
    {
        private new Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        public void AddForce(Vector3 force)
        {
            rigidbody.AddForce(force);
        }

        public void AddForce(Vector3 force, float duration)
        {
            rigidbody.AddForce(force);
            StartCoroutine(RemoveForce(force, duration));
        }

        public Vector3 GetCurrentDirection()
        {
            return rigidbody.velocity.normalized;
        }

        private IEnumerator RemoveForce(Vector3 force, float time)
        {
            yield return new WaitForSeconds(time);
            rigidbody.AddForce(-force);
        }
    }
}
