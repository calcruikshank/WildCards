using UnityEngine;

public class Taunt : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.keywords.Add(SpellSiegeData.Keywords.Deathtouch);
    }
}
