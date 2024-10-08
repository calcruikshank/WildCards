using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.keywords.Add(SpellSiegeData.Keywords.Deathtouch);
    }
}
