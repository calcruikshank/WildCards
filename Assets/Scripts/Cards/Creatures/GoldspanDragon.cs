using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldspanDragon : Creature
{
    public override void OnAttack()
    {
        base.OnAttack();
        playerOwningCreature.AddGoldToNextTurn();
    }
}
