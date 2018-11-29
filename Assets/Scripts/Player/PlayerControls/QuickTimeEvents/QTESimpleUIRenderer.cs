using System;
using UnityEngine;
using UnityEngine.UI;

namespace MastersOfTempest.PlayerControls.QTE
{
    /// <summary>
    /// QTE renderer for testing. Logs qte events and displays
    /// information for player in a very simple manner
    /// </summary>
    public class QTESimpleUIRenderer : MonoBehaviour
    {
        public QTEDriver Driver;
        public Text InfoForUser;

        private void Start()
        {
            SanityCheck();
            Driver.Start += OnQTEStart;
            Driver.End += OnQTEEnd;
            Driver.Success += OnQTESuccess;
            Driver.Fail += OnQTEFail;
            Driver.NewKey += OnQTENewKey;
        }



        private void OnDestroy()
        {
            Driver.Start -= OnQTEStart;
            Driver.End -= OnQTEEnd;
            Driver.Success -= OnQTESuccess;
            Driver.Fail -= OnQTEFail;
            Driver.NewKey -= OnQTENewKey;
        }

        private void OnQTENewKey(object sender, EventArgs e)
        {
            var args = (QTENewKeyEventArgs)e;
            InfoForUser.text = $"PRESS THE KEY \"{args.AwaitedKey}\" QUICK! YOU HAVE ONLY {args.TimeToReact} SECONDS!";
        }


        private void OnQTEFail(object sender, EventArgs e)
        {
            InfoForUser.text = "WROOONG";
            Debug.Log("QTE failed");
        }


        private void OnQTESuccess(object sender, EventArgs e)
        {
            InfoForUser.text = "GOOD JOB";
            Debug.Log("QTE success");
        }


        private void OnQTEEnd(object sender, EventArgs e)
        {
            InfoForUser.text = "END";
            Debug.Log("QTE finished");
        }


        private void OnQTEStart(object sender, EventArgs e)
        {
            InfoForUser.text = "START";
            Debug.Log("QTE started");
        }

        private void SanityCheck()
        {
            if (Driver == null)
            {
                throw new InvalidOperationException($"{nameof(Driver)} is not specified!");
            }
            if (InfoForUser == null)
            {
                throw new InvalidOperationException($"{nameof(InfoForUser)} is not specified!");
            }
        }
    }

}
