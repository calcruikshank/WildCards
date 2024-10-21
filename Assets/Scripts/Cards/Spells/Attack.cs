using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : TargetedSpell
{
    protected override void SpecificSpellAbility()
    {
        if (creatureTargeted != null && creatureTargeted.transform != null)
        {
            creatureTargeted.baseAttack += 4;
            creatureTargeted.currentAttack += 4;
            creatureTargeted.cardData.currentAttack += 4;


            creatureTargeted.UpdateCreatureHUD();
        }
        creatureTargeted.WriteCurrentDataToCardData();
    }
}
