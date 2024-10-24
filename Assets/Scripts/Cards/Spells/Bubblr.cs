using UnityEngine;

public class Bubblr : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        creatureTargeted.keywords.Add(SpellSiegeData.Keywords.BubbleShield);
        creatureTargeted.GiveBubble();
    }
}
