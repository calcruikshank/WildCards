using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbioticOoze : Creature
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();
        this.currentAttack *= 2;
        this.CurrentHealth *= 2;
        UpdateCreatureHUD();
    }
}
