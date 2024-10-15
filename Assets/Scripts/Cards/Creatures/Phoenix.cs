using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phoenix : Creature
{
    public override void OnOwnerCastSpell()
    {
        base.OnOwnerCastSpell();
        numberOfTimesThisCanDie++;
        Debug.LogError(numberOfTimesThisCanDie + " num of times this can die");
    }
}
