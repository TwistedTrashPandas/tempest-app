using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.QTE
{
    public class QTEDriver : MonoBehaviour
    {
        /*
         *  The idea for now is that the driver will be raising events 
         *  and corresponding renderers will react to it and display stuff
         *  to players
         */
        public event EventHandler Start;
        public event EventHandler End;
        public event EventHandler Success;
        public event EventHandler Fail;
        public event EventHandler StartedWaitingForTheNextKey;

        private List<KeyCode> possibleKeys = new List<KeyCode> {KeyCode.A, KeyCode.B, KeyCode.C };


        public void StartQuickTimeEvent()
        {
            throw new NotImplementedException();
        }

        public void EndQuickTimeEvent()
        {
            throw new NotImplementedException();
        }

        private IEnumerator QTE()
        {
            //TODO: fire Start

            const float timeToReact = 2f;
            float timeElapsed;

            //TODO: have exit condition here
            while (true)
            {
                var expectedKey = GetNextExpectedKey();
                timeElapsed = 0f;
                //TODO: fire StartWaitingForTheNextKey
                while (timeElapsed < timeToReact)
                {

                    yield return null;
                    timeElapsed += Time.deltaTime;
                    foreach (var key in possibleKeys)
                    {
                        //HACK: we assume that player can't press down 2 keys at the same time
                        if (Input.GetKeyDown(key))
                        {
                            if (expectedKey == key)
                            {
                                //TODO: fire success
                            }
                            else
                            {
                                //TODO: fire fail
                            }
                            break;
                        }
                    }
                }
                //TODO: if not success, then fire fail, player was too slow
            }

            //TODO: fire End
        }

        private KeyCode GetNextExpectedKey()
        {
            return possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Count)];
        }
    }
}
