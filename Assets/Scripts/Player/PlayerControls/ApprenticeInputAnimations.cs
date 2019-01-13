﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class ApprenticeInputAnimations : MonoBehaviour
    {
        public Animator leftHandAnimator;
        public Animator rightHandAnimator;
        public Transform meditate;

        private Hammer hammer;
        private bool isRepairing = false;
        private bool isThrowing = false;
        private bool isMeditating = false;

        protected void Start()
        {
            hammer = GetComponentInChildren<Hammer>();
        }

        public void Repair ()
        {
            if (!IsBusy())
            {
                StartCoroutine(RepairAnimation(1));
            }
        }

        public void Throw (Camera firstPersonCamera)
        {
            if (!IsBusy())
            {
                StartCoroutine(ThrowAnimation(firstPersonCamera, 50, 1.0f, 1000));
            }
        }

        public void Meditate ()
        {
            if (!IsBusy())
            {
                StartCoroutine(MeditateAnimation(0.9f));
            }
        }

        private bool IsBusy ()
        {
            return isRepairing || isMeditating || isThrowing;
        }

        private IEnumerator RepairAnimation (float time)
        {
            isRepairing = true;
            rightHandAnimator.SetTrigger("Repair");
            yield return new WaitForSeconds(time / 2);
            hammer.charge = Mathf.Clamp01(hammer.charge - 0.2f);
            yield return new WaitForSeconds(time / 2);
            isRepairing = false;
        }

        private IEnumerator MeditateAnimation (float time)
        {
            isMeditating = true;
            leftHandAnimator.SetTrigger("Meditate");
            rightHandAnimator.SetTrigger("Meditate");

            float t = 0;
            float startCharge = hammer.charge;
            Vector3 startPosition = hammer.transform.localPosition;
            Quaternion startRotation = hammer.transform.localRotation;

            while (t < time)
            {
                hammer.transform.localPosition = Vector3.Lerp(hammer.transform.localPosition, meditate.transform.localPosition, t / time);
                hammer.transform.localRotation = Quaternion.Lerp(hammer.transform.localRotation, meditate.transform.localRotation, t / time);

                hammer.charge = Mathf.Clamp01(startCharge + 0.33f * (t / time));

                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            t = 0;

            while (t < time)
            {
                hammer.transform.localPosition = Vector3.Lerp(hammer.transform.localPosition, startPosition, t / time);
                hammer.transform.localRotation = Quaternion.Lerp(hammer.transform.localRotation, startRotation, t / time);

                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            hammer.transform.localPosition = startPosition;
            hammer.transform.localRotation = startRotation;
            isMeditating = false;
        }

        private IEnumerator ThrowAnimation (Camera firstPersonCamera, float distance, float time, float rotationSpeed)
        {
            isThrowing = true;
            rightHandAnimator.SetTrigger("Throw");

            yield return new WaitForSeconds(0.375f);

            Transform startParent = hammer.transform.parent;
            Vector3 startPosition = hammer.transform.localPosition;
            Quaternion startRotation = hammer.transform.localRotation;

            float t = 0;
            hammer.EnableCollider(true);
            hammer.transform.SetParent(firstPersonCamera.transform, true);

            while (t < time)
            {
                Vector3 endPosition = firstPersonCamera.transform.position + distance * firstPersonCamera.transform.forward;

                hammer.transform.position = Vector3.Lerp(startParent.TransformPoint(startPosition), endPosition, t / time);
                hammer.transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0, Space.Self);

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
                    hammer.transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
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
            hammer.EnableCollider(false);
            isThrowing = false;
        }
    }
}
