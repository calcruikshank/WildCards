using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leviathan : Creature
{
    public override void OnDeath()
    {
        base.OnDeath();
        playerOwningCreature.GiveLeviathanBoon();
    }
}
