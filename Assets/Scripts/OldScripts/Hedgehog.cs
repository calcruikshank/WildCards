using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hedgehog : Creature
{
    public override bool IsCreatureWithinRange(Creature creatureSent)
    {
        return true; // Assuming Hedgehog always considers creatures within range for simplicity
    }

    public override Creature ChooseTarget()
    {
        currentTargetedCreature = null;
        Creature closestCreature = null;
        Creature tauntCreature = null;
        float minDistance = float.MaxValue;
        float tauntMinDistance = float.MaxValue;
        tauntFound = false;

        // Ensure opponent exists and they own creatures
        if (playerOwningCreature.opponent && playerOwningCreature.opponent.creaturesOwned.Count > 0)
        {
            foreach (Creature creatureWithinRange in playerOwningCreature.opponent.creaturesOwned)
            {
                if (creatureWithinRange == null)
                {
                    continue; // Skip null creatures
                }

                // Skip creatures with stealth
                if (creatureWithinRange.keywords.Contains(SpellSiegeData.Keywords.Stealth))
                {
                    continue;
                }

                float distance = Vector3.Distance(this.transform.position, creatureWithinRange.transform.position);

                // Check for taunt creatures and prioritize the closest one
                if (creatureWithinRange.keywords.Contains(SpellSiegeData.Keywords.Taunt))
                {
                    tauntFound = true;
                    if (distance < tauntMinDistance)
                    {
                        tauntMinDistance = distance;
                        tauntCreature = creatureWithinRange;
                    }
                }
                // If no taunt creature found, check for the closest non-taunt creature
                else if (!tauntFound && distance < minDistance)
                {
                    minDistance = distance;
                    closestCreature = creatureWithinRange;
                }
            }
        }

        // If a taunt creature was found, target it
        if (tauntFound)
        {
            currentTargetedCreature = tauntCreature;
        }
        // If no taunt was found, target the closest creature
        else if (closestCreature != null)
        {
            currentTargetedCreature = closestCreature;
        }

        // Validate if the target is within range
        if (currentTargetedCreature != null && !IsCreatureWithinRange(currentTargetedCreature))
        {
            currentTargetedCreature = null; // Reset if target is out of range
        }

        // If there's an external target to follow and it belongs to the opponent, prioritize it
        if (targetToFollow != null && targetToFollow.playerOwningCreature != this.playerOwningCreature)
        {
            currentTargetedCreature = targetToFollow;
        }

        // Ensure the target is in the correct state before returning
        if (currentTargetedCreature != null && IsCreatureWithinRange(currentTargetedCreature) &&
            Vector3.Distance(new Vector3(actualPosition.x, this.transform.position.y, actualPosition.z),
            new Vector3(tileCurrentlyOn.transform.position.x, this.transform.position.y, tileCurrentlyOn.transform.position.z)) < 0.1f)
        {
            creatureState = CreatureState.Idle;
            return currentTargetedCreature; // Return the valid current target
        }

        return currentTargetedCreature; // Return the selected target (if any)
    }
}
