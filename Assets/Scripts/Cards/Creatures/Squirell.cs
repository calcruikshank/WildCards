using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squirell : Creature
{
    public override void OnAttack()
    {
        base.OnAttack();
        this.GiveCounter(1);
    }
}
