using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lion : Creature
{
    public override void OnTurnMoveIfNoCreatures()
    {
        base.OnTurnMoveIfNoCreatures();

        foreach (Creature kvp in playerOwningCreature.creaturesOwned)
        {
            kvp.GiveCounter(1);
        }
    }
}
