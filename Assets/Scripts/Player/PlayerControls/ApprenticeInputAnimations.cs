using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class ApprenticeInputAnimations : MonoBehaviour
    {
        public Animator leftHandAnimator;
        public Animator rightHandAnimator;

        public void Repair ()
        {
            rightHandAnimator.SetTrigger("Repair");
        }

        public void Throw ()
        {
            rightHandAnimator.SetTrigger("Throw");
        }

        public void Meditate ()
        {
            leftHandAnimator.SetTrigger("Meditate");
            rightHandAnimator.SetTrigger("Meditate");
        }
    }
}
