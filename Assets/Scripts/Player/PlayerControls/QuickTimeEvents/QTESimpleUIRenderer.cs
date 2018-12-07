using System;
using TMPro;
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

        private const string InfoForUserPrefabPath = "UIPrefabs/SimpleQTERender/Hint";
        private TMP_Text infoForUser;

        private void Start()
        {
            infoForUser = UIManager.GetInstance().SpawnUIElement<TMP_Text>(InfoForUserPrefabPath);
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
            infoForUser.text = $"PRESS THE KEY \"{args.AwaitedKey}\" QUICK! YOU HAVE ONLY {args.TimeToReact} SECONDS!";
        }


        private void OnQTEFail(object sender, EventArgs e)
        {
            infoForUser.text = "WROOONG";
            Debug.Log("QTE failed");
        }


        private void OnQTESuccess(object sender, EventArgs e)
        {
            infoForUser.text = "GOOD JOB";
            Debug.Log("QTE success");
        }


        private void OnQTEEnd(object sender, EventArgs e)
        {
            infoForUser.text = "END";
            Debug.Log("QTE finished");
        }


        private void OnQTEStart(object sender, EventArgs e)
        {
            infoForUser.text = "START";
            Debug.Log("QTE started");
        }

        private void SanityCheck()
        {
            if (Driver == null)
            {
                throw new InvalidOperationException($"{nameof(Driver)} is not specified!");
            }
            if (infoForUser == null)
            {
                throw new InvalidOperationException($"{nameof(infoForUser)} is not specified!");
            }
        }
    }
}
