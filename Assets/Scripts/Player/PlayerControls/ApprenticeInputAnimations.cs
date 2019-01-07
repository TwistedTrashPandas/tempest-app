using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
public class ApprenticeInputAnimations : MonoBehaviour
{
        public ApprenticeInput ApprenticeInput;
        public Animator anim;
 
    private void Start()
    {
        if (ApprenticeInput == null)
            {
                throw new InvalidProgramException($"{nameof(ApprenticeInput)} is not specified!");
            }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}