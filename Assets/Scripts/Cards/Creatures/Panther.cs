using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panther : Creature
{
    public List<Creature> creaturesYouveDealtDamageTo = new List<Creature>();
    public override void Move()
    {
        Creature creatureOnCurrentTile = BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition);
        if (creatureOnCurrentTile != null && creatureOnCurrentTile != this && creatureOnCurrentTile.playerOwningCreature != this)
        {
            if (!creaturesYouveDealtDamageTo.Contains(creatureOnCurrentTile))
            {
                creaturesYouveDealtDamageTo.Add(creatureOnCurrentTile);
                VisualAttackAnimation(creatureOnCurrentTile);
            }
        }

        if (previousTilePosition != tileCurrentlyOn)
        {
            CalculateAllTilesWithinRange();
            previousTilePosition.RemoveCreatureFromTile(this);
            previousTilePosition = tileCurrentlyOn;
        }

        if (currentTargetedStructure != null)
        {
            targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);
            targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(currentCellPosition);

            // Determine the direction of movement
            Vector3Int nextCellPosition;
            if (currentTargetedStructure.currentCellPosition.x < this.currentCellPosition.x)
            {
                nextCellPosition = new Vector3Int(currentCellPosition.x - 1, currentCellPosition.y, currentCellPosition.z);
                animatorForObject.transform.localEulerAngles = new Vector3(0, -90, 0);
            }
            else
            {
                nextCellPosition = new Vector3Int(currentCellPosition.x + 1, currentCellPosition.y, currentCellPosition.z);
                animatorForObject.transform.localEulerAngles = new Vector3(0, 90, 0);
            }


            targetedCell = BaseMapTileState.singleton.GetBaseTileAtCellPosition(nextCellPosition);
            targetedCellForChoosingTargets = BaseMapTileState.singleton.GetBaseTileAtCellPosition(nextCellPosition);

            actualPosition = new Vector3(targetedCell.transform.position.x, this.transform.position.y, targetedCell.transform.position.z);
            currentCellPosition = targetedCell.tilePosition;


            Creature creatureOnCurrentTile2 = BaseMapTileState.singleton.GetCreatureAtTile(currentCellPosition);
            if (creatureOnCurrentTile2 != null && creatureOnCurrentTile2 != this && creatureOnCurrentTile2.playerOwningCreature != this)
            {
                if (!creaturesYouveDealtDamageTo.Contains(creatureOnCurrentTile2))
                {
                    creaturesYouveDealtDamageTo.Add(creatureOnCurrentTile2);
                    VisualAttackAnimation(creatureOnCurrentTile2);
                }
            }




            SetStateToIdle();
            CheckForCreaturesWithinRange();
            ChooseTarget();
        }

        if (playerOwningCreature == GameManager.singleton.playerInScene)
        {
            if (tileCurrentlyOn.tilePosition.y % 2 == 0)
            {
                if (playerOwningCreature.opponent.instantiatedCaste.tileCurrentlyOn.tilePosition.x == this.tileCurrentlyOn.tilePosition.x)
                {
                    ExplodeOnPlayerKeep();
                }
            }
            else
            {
                if (playerOwningCreature.opponent.instantiatedCaste.tileCurrentlyOn.tilePosition.x == this.tileCurrentlyOn.tilePosition.x + 1)
                {
                    ExplodeOnPlayerKeep();
                }
            }
        }
        else
        {
            if (tileCurrentlyOn.tilePosition.y % 2 == 0)
            {
                if (playerOwningCreature.opponent.instantiatedCaste.tileCurrentlyOn.tilePosition.x == this.tileCurrentlyOn.tilePosition.x)
                {
                    ExplodeOnPlayerKeep();
                }
            }
            else
            {
                if (playerOwningCreature.opponent.instantiatedCaste.tileCurrentlyOn.tilePosition.x == this.tileCurrentlyOn.tilePosition.x)
                {
                    ExplodeOnPlayerKeep();
                }
            }
        }

    }

    public override Creature ChooseTarget()
    {
        return null;
    }
}
