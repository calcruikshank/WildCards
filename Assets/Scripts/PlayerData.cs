using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData 
{
    public int round;
    public List<CardInHand> cardsInHand;
    public List<Creature> creaturesOnField;
}
