using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalGrowth : Spell
{
    protected override void SpecificCastEffect()
    {
        base.SpecificCastEffect();
        playerCastingSpell.CheckAffordableCards();
    }
}
