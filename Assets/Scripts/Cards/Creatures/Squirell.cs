using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squirell : Creature
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();
        foreach (Creature creature in playerOwningCreature.creaturesOwned)
        {
            if (creature.cardData.cardAssignedToObject == SpellSiegeData.Cards.Squirell)
            {
                if (!creature.cardData.isInHand)
                {
                    this.GiveCounter(1);
                }
            }
        }
    }
}
