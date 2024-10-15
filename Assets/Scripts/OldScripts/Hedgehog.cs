using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hedgehog : Creature
{
    public override bool IsCreatureWithinRange(Creature creatureSent)
    {
        return true;
    }
    public override Creature ChooseTarget()
    {
        float lowestHealthCreatureWithinRange = -1;
        currentTargetedCreature = null;
        Creature closestCreature = null;
        float minDistance = float.MaxValue;
        tauntFound = false;



        if (playerOwningCreature.opponent)
        {
            if (playerOwningCreature.opponent.creaturesOwned.Count > 0)
            {

                foreach (Creature creatureWithinRange in playerOwningCreature.opponent.creaturesOwned)
                {
                    if (creatureWithinRange == null)
                    {
                    }

                    if (creatureWithinRange.playerOwningCreature != this.playerOwningCreature)
                    {
                        // Skip creatures with stealth
                        if (creatureWithinRange.keywords.Contains(SpellSiegeData.Keywords.Stealth))
                        {
                            continue;
                        }

                        if (creatureWithinRange.keywords.Contains(SpellSiegeData.Keywords.Taunt))
                        {
                            currentTargetedCreature = creatureWithinRange;
                            tauntFound = true;
                            break; // Since we always want to target the creature with taunt, we can break the loop here
                        }

                        float distance = Vector3.Distance(this.transform.position, creatureWithinRange.transform.position);
                        if (!tauntFound && distance < minDistance)
                        {
                            minDistance = distance;
                            closestCreature = creatureWithinRange;
                        }
                    }
                }
            }
        }

        if (!tauntFound && closestCreature != null)
        {
            currentTargetedCreature = closestCreature;
        }
        if (currentTargetedCreature != null)
        {
            if (!IsCreatureWithinRange(currentTargetedCreature))
            {
                currentTargetedCreature = null;
            }
        }
        if (targetToFollow != null && targetToFollow.playerOwningCreature != this.playerOwningCreature)
        {
            currentTargetedCreature = targetToFollow;
        }




        if (currentTargetedCreature != null && IsCreatureWithinRange(currentTargetedCreature) && Vector3.Distance(new Vector3(actualPosition.x, this.transform.position.y, actualPosition.z), new Vector3(tileCurrentlyOn.transform.position.x, this.transform.position.y, tileCurrentlyOn.transform.position.z)) < .1f)
        {
            creatureState = CreatureState.Idle;
            return closestCreature;
        }
        return closestCreature;

    }
}
