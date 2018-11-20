using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvRock : EnvObject {

    public EnvRock(float sp) : base(sp)
    {
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        // base.OnTriggerEnter(other);
        //new stuff
        print("Trigger Entered! Rock");
    }

}
