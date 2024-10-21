using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.maxRange = 3;
        creatureTargeted.CalculateAllTilesWithinRange();
    }
}
