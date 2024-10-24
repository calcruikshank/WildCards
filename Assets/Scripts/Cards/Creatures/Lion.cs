using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lion : Creature
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();

        foreach (Creature kvp in playerOwningCreature.creaturesOwned)
        {
            if (kvp != this)
            {
                kvp.GiveCounter((int)this.currentAttack);
            }
        }
    }
}
