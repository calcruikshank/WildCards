using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoundConfiguration 
{
    public List<CardInHand> cardsInHand;
    public List<Creature> creaturesOnField;
    public int round;
}
