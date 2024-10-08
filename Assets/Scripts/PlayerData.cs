using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public List<RoundConfiguration> playerRoundConfigurations = new List<RoundConfiguration>();
    public int currentRound;
}
