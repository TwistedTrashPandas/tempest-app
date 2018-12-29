using MastersOfTempest.Environment;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MastersOfTempest
{
    public class WinCondition : MonoBehaviour
    {
        public delegate void WinAnimation(GameObject ship);
        public static event WinAnimation OnWin;

        public float radiusCollider = 10f;
        private CapsuleCollider winCondition;

        public void Initialize(VectorField vectorField)
        {
            winCondition = gameObject.AddComponent<CapsuleCollider>();
            winCondition.center = vectorField.GetCenterWS();
            winCondition.height = vectorField.GetCellSize() * vectorField.GetDimensions().y * 1.5f;
            winCondition.direction = 1;
            winCondition.isTrigger = true;
            winCondition.radius = 10f;
            StartCoroutine(WinAfter10secs());
        }

        private IEnumerator WinAfter10secs()
        {
            yield return new WaitForSeconds(10f);
            OnWin(gameObject.GetComponent<Gamemaster>().GetShip().gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            // WIN EVENT
            if(other.tag == "Ship")
            {
                OnWin(other.transform.parent.gameObject);
                Debug.Log("Victory!");
            }
        }
    }
}
