using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardKeybindAnimations : MonoBehaviour
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
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.Play("Armature|Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("q"))
        {
            anim.SetBool("holdBook",true);
            anim.CrossFade("Armature|GrabBook", 0.5f);
        }
        if(Input.GetKeyDown("w"))
        {
            anim.SetBool("holdBook", false);

        }
        if(Input.GetKeyDown("e"))
        {
            anim.SetBool("holdSpell", true);
            anim.CrossFade("Armature|RightHandGrab", 0.5f);
        }
        if(Input.GetKeyDown("t"))
        {
            anim.SetTrigger("pulse");
        }
        if(Input.GetKeyDown("r"))
        {
            anim.SetBool("holdSpell", false);
        }
    }
}
