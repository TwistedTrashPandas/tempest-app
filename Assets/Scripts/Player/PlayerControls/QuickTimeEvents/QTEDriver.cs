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
        public event EventHandler NewKey;

        //todo: have more keys and different way to initialize
        private readonly List<KeyCode> possibleKeys = new List<KeyCode> {KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.E, KeyCode.Z, KeyCode.G };

        /// <summary>
        /// Starts the quick time event that will be running until the cancellation is requested
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to interrupt the QTE generation</param>
        public void StartQuickTimeEvent(CoroutineCancellationToken cancellationToken)
        {
            StartCoroutine(QTE(cancellationToken));
        }

        private IEnumerator QTE(CoroutineCancellationToken cancellationToken)
        {
            Start?.Invoke(this, EventArgs.Empty);

            const float timeToReact = 2f;
            float timeElapsed;
            bool interactionFlag;
            
            while (!cancellationToken.CancellationRequested)
            {
                var expectedKey = GetNextExpectedKey();
                timeElapsed = 0f;
                interactionFlag = false;
                NewKey?.Invoke(this, new QTENewKeyEventArgs(expectedKey, timeToReact));
                while (timeElapsed < timeToReact && !cancellationToken.CancellationRequested)
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
                                Success?.Invoke(this, EventArgs.Empty);
                            }
                            else
                            {
                                Fail?.Invoke(this, EventArgs.Empty);
                            }
                            interactionFlag = true;
                            break;
                        }
                    }
                    if(interactionFlag)
                    {
                        break;
                    }
                }
                //If player haven't interacted in the given timeframe, then it's 
                //a fail too - too slow (we don't fire a fail if QTE was cancelled)
                if (!interactionFlag && !cancellationToken.CancellationRequested)
                {
                    Fail?.Invoke(this, EventArgs.Empty);
                }
            }
            End?.Invoke(this, EventArgs.Empty);
        }

        private KeyCode GetNextExpectedKey()
        {
            return possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Count)];
        }
    }
}
