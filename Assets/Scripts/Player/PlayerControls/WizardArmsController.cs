using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class WizardArmsController : MonoBehaviour
    {
        /*
        * Explaination
        *  ANIMATIONS:
        *  - Armature|Idle; Armature|IdleLeft; Armature|IdleRight : selfexplaining, also used to reset the animationloops in the animator
        *  - Armature|GrabBook; Armature|HoldBook; Armature|StowBook : Animationstates of the Book-Loop
        *  - Armature|RightHandGrab; Armature|RightHandHold; Armature|RightHandStow : Animationstates of the Spell-Holding-Loop
        *  - Armature|HandRightPulse : This animation shall occure every time the Spell fails charging or, does not hit the target. But only if the wizards still holds on to the energy afterwards.
        *
        *  PARAMETERS
        *  bool holdBook : true -> enter HoldAnimation; false -> end HoldAnimation
        *  bool holdSpell : true -> enter RightHandHoldAnimation; false -> end RightHandHoldAnimation
        *  trigger pulse : "trigger" works like a onetime bool. It will let the animation "Pulse" loop one time, bevor leaving it again
        * */
        public Transform SuckPoint;
        public Spellbook book;

        public ParticleSystem [] HoldPS;
        public ParticleSystem [] DissipatePS;
        public ParticlesSucker FeedPS;
        private Animator anim;
        void Start()
        {
            if (SuckPoint == null)
            {
                throw new InvalidOperationException($"{nameof(SuckPoint)} is not specified!");
            }
            if (book == null)
            {
                throw new InvalidOperationException($"{nameof(book)} is not specified!");
            }
            if (HoldPS == null)
            {
                throw new InvalidOperationException($"{nameof(HoldPS)} is not specified!");
            }
            if (DissipatePS == null)
            {
                throw new InvalidOperationException($"{nameof(DissipatePS)} is not specified!");
            }
            if (FeedPS == null)
            {
                throw new InvalidOperationException($"{nameof(FeedPS)} is not specified!");
            }
            anim = GetComponent<Animator>();
            if (anim == null)
            {
                throw new InvalidOperationException($"{nameof(anim)} is not specified!");
            }
            anim.Play("Armature|Idle");
        }

        public void ToggleBook()
        {
            if(anim.GetBool("holdBook"))
            {
                HideBook();
            }
            else
            {
                TakeOutBook();
            }
        }

        public void NextPage()
        {
            book.NextPage();
        }
        public void PrevPage()
        {
            book.PreviousPage();
        }

        public void TakeOutBook()
        {
            anim.SetBool("holdBook", true);
            anim.CrossFade("Armature|GrabBook", 0.5f);
            //If we wait a bit before showing the book opening animation, it looks cooler
            const float DelayBeforeOpeningTheBook = 1.1f;
            book.OpenOrClose(DelayBeforeOpeningTheBook);
        }

        public void HideBook()
        {
            anim.SetBool("holdBook", false);
            book.OpenOrClose();
        }

        public void HoldSpell()
        {
            anim.SetBool("holdSpell", true);
            anim.CrossFade("Armature|RightHandGrab", 0.5f);
        }

        public void PulseRightHand()
        {
            anim.SetTrigger("pulse");
        }

        public void ReleaseSpell()
        {
            anim.SetBool("holdSpell", false);
        }
    }
}
