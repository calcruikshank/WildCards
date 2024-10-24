using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritSummoner : Creature
{
    public override void OnDeath()
    {
        base.OnDeath();
        CardInHand randomCreatureInHand = playerOwningCreature.GetRandomCreatureInHand();
        if (randomCreatureInHand != null)
        {
           Creature iC =  playerOwningCreature.CastCreatureOnTile(randomCreatureInHand, this.tileCurrentlyOn.tilePosition);
            iC.StartFighting();
        }
    }
}
