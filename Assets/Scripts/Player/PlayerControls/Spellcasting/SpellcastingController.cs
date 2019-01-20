using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// This controller is responsible for the spellcasting part of the
    /// Wizard gameplay. Doesn't have to exist on the client
    /// </summary>
    public class SpellcastingController : MonoBehaviour
    {
        /// <summary>
        /// Triggers every time the spell action is called. Can be used by animation controllers to show some SFX and/or play sound when spell is active
        /// </summary>
        public event EventHandler SpellCasted;

        private const float SpellCheckFrequency = 5f;

        private bool isActive;

        public List<PowerRecepticleController> recepticles = new List<PowerRecepticleController>();

        private Gamemaster context;

        private void Start()
        {
            context = FindObjectsOfType<Gamemaster>().First(gm => gm.gameObject.scene == gameObject.scene);
            if (context == null)
            {
                throw new InvalidOperationException($"{nameof(context)} is not specified!");
            }
            if (recepticles == null || recepticles.Count < 1)
            {
                throw new InvalidOperationException($"{nameof(recepticles)} collection is not specified!");
            }
            foreach (var r in recepticles)
            {
                if (r == null) throw new InvalidOperationException($"Null element in {nameof(recepticles)} collection");
            }
            StartCoroutine(CheckSpell());
        }

        private IEnumerator CheckSpell()
        {
            while (true)
            {
                if (recepticles.All(recepticle => recepticle.CurrentCharge != Charge.None))
                {
                    Spell fittingSpell = null;
                    foreach (var spell in SpellList.Spells)
                    {
                        bool fail = false;
                        for (int i = 0; i < recepticles.Count; ++i)
                        {
                            if (recepticles[i].CurrentCharge != spell.SpellSequence[i])
                            {
                                fail = true;
                                break;
                            }
                        }
                        if (!fail)
                        {
                            fittingSpell = spell;
                            break;
                        }
                    }
                    if (fittingSpell != null)
                    {
                        fittingSpell.GetPlayerAction().Execute(context);
                        SpellCasted?.Invoke(this, new SpellCastedEventArgs(fittingSpell));
                        // Debug.Log($"Spell {fittingSpell.Name} called");
                    }
                }
                yield return new WaitForSeconds(1f / SpellCheckFrequency);
            }
        }
    }
}
