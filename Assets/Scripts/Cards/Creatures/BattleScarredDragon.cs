using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleScarredDragon : Creature
{
    List<Creature> creaturesDoubled = new List<Creature>();
    protected override void CheckForCreaturesWithinRange()
    {
        base.CheckForCreaturesWithinRange();
    }


    public override void OnDeath()
    {
        base.OnDeath();
        foreach (Creature creatureOwned in playerOwningCreature.creaturesOwned)
        {
            creatureOwned.numberOfTimesThisCanDie++;
        }
    }
}
