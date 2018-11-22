using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MastersOfTempest
{
    public class Ship : MonoBehaviour
    {
        private Gamemaster context;

        private void Start()
        {
            context = FindObjectOfType<Gamemaster>();
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(Ship)} cannot operate without Gamemaster in the same scene!");
            }
            context.Register(this);
        }
    }
}
