using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avacyn : Creature
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();
        if (playerOwningCreature == GameManager.singleton.playerInScene)
        {
            Creature creatureBehind = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(cardData.positionOnBoard.x - 1, cardData.positionOnBoard.y, cardData.positionOnBoard.z)).creatureOnTile;
            if (creatureBehind)
            {
                creatureBehind.GiveBubble();
                creatureBehind.GiveSpeed();
            }
        }
        else
        {
            Creature creatureBehind = BaseMapTileState.singleton.GetBaseTileAtCellPosition(new Vector3Int(cardData.positionOnBoard.x +1, cardData.positionOnBoard.y, cardData.positionOnBoard.z)).creatureOnTile;
            if (creatureBehind)
            {
                creatureBehind.GiveBubble();
                creatureBehind.GiveSpeed();
            }
        }
    }
}
