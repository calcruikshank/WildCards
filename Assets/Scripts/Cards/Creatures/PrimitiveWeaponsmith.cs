using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveWeaponsmith : Creature
{
    public override void OnCombatStart()
    {
        GivePlusOneRange();
    }

    private void GivePlusOneRange()
    {
        foreach (Creature friendly in playerOwningCreature.creaturesOwned)
        {
            if (friendly.currentRange == 1)
            {
                friendly.visualAttackParticle = this.visualAttackParticle;
            }

            friendly.AddOneRange();
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
    }
}
