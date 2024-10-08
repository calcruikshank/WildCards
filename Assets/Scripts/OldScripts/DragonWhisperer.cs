using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonWhisperer : Creature
{
    public override void OnETB()
    {
        base.OnETB();
        foreach (CardInHand cardindeck in playerOwningCreature.cardsInHand)
        {
            if (cardindeck.cardData.creatureType == SpellSiegeData.CreatureType.Dragon)
            {
                if (cardindeck.cardData.redManaCost > 0)
                {
                    cardindeck.cardData.redManaCost--;
                    cardindeck.UpdateMana();
                }
            }
        }
    }
}
