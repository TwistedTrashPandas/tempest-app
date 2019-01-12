using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class ApprenticeInputAnimations : MonoBehaviour
    {
        public Animator leftHandAnimator;
        public Animator rightHandAnimator;

        private Hammer hammer;
        private bool isThrowing = false;

        protected void Start()
        {
            hammer = GetComponentInChildren<Hammer>();
        }

        public void Repair ()
        {
            rightHandAnimator.SetTrigger("Repair");
        }

        public void Throw (Camera firstPersonCamera)
        {
            if (!isThrowing)
            {
                StartCoroutine(ThrowAnimation(firstPersonCamera, 5, 1.0f));
            }
        }

        public void Meditate ()
        {
            leftHandAnimator.SetTrigger("Meditate");
            rightHandAnimator.SetTrigger("Meditate");
        }

        private IEnumerator ThrowAnimation (Camera firstPersonCamera, float distance, float time)
        {
            isThrowing = true;
            rightHandAnimator.SetTrigger("Throw");

            yield return new WaitForSeconds(0.5f);

            Transform startParent = hammer.transform.parent;
            Vector3 startPosition = hammer.transform.localPosition;
            Quaternion startRotation = hammer.transform.localRotation;

            float t = 0;
            hammer.transform.SetParent(null, true);

            while (t < time)
            {
                Vector3 endPosition = firstPersonCamera.transform.position + distance * firstPersonCamera.transform.forward;

                hammer.transform.position = Vector3.Lerp(startParent.TransformPoint(startPosition), endPosition, t / time);
                hammer.transform.Rotate(1000 * Time.deltaTime, 0, 0, Space.Self);

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            t = 0;

            hammer.transform.SetParent(startParent, true);

            while (t < time)
            {
                Vector3 endPosition = firstPersonCamera.transform.position + distance * firstPersonCamera.transform.forward;

                hammer.transform.position = Vector3.Lerp(endPosition, startParent.TransformPoint(startPosition), t / time);

                if (t <  0.9f * time)
                {
                    hammer.transform.Rotate(1000 * Time.deltaTime, 0, 0, Space.Self);
                }
                else
                {
                    hammer.transform.localRotation = Quaternion.Lerp(hammer.transform.localRotation, startRotation, t / time);
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }

            hammer.transform.localPosition = startPosition;
            hammer.transform.localRotation = startRotation;
            isThrowing = false;
        }
    }
}
