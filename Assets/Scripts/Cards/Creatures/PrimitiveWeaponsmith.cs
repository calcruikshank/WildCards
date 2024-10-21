using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveWeaponsmith : Creature
{
    List<Creature> creaturesThatHaveExtraRange = new List<Creature>();
    public override void OnCombatStart()
    {
        GivePlusOneRange();
    }

    private void GivePlusOneRange()
    {
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesThatHaveExtraRange.Contains(friendly))
            {
                if (friendly.range == 1)
                {
                    friendly.visualAttackParticle = this.visualAttackParticle;
                }

                friendly.AddOneRange();
            }
        }
        creaturesThatHaveExtraRange = friendlyCreaturesWithinRange;
    }

    public override void OnDeath()
    {
        base.OnDeath();
    }
}
