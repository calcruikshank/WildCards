using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hamster : Creature
{
    public override void OnAttack()
    {
        base.OnAttack();
        GiveCounter(1);
    }
}
