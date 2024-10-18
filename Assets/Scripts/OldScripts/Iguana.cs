using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iguana : Creature
{
    public override void OtherCreatureAttacked(Creature creature)
    {
        base.OtherCreatureAttacked(creature);

        if (creature.playerOwningCreature == this.playerOwningCreature)
        {
            if (creature != null && creature.transform != null)
            {
                creature.baseAttack += 3;
                creature.currentAttack += 3;
                creature.cardData.currentAttack += 3;


                creature.UpdateCreatureHUD();
            }
            creature.WriteCurrentDataToCardData();
            if (creature.playerOwningCreature == GameManager.singleton.playerInScene)
            {
                creature.playerOwningCreature.SavePlayerConfigLocallyInRoundGUID(playerOwningCreature.playerData, playerOwningCreature.currentGUIDForPlayer);
            }
        }

    }
}
