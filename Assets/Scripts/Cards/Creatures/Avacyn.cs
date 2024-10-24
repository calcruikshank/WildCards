using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avacyn : Creature
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();
        foreach (Creature creatureOwned in playerOwningCreature.creaturesOwned)
        {
            creatureOwned.GiveBubble();
        }
    }
}
