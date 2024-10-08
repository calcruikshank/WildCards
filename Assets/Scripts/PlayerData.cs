using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    public List<RoundConfiguration> playerRoundConfigurations = new List<RoundConfiguration>();
    public int currentRound = 0;


    public void AddOrUpdateRoundConfiguration(RoundConfiguration newRoundConfiguration)
    {
        // Find the existing RoundConfiguration for the next round
        RoundConfiguration existingConfig = playerRoundConfigurations
            .Find(config => config.round == currentRound + 1);

        if (existingConfig != null)
        {
            // Update the existing RoundConfiguration
            int index = playerRoundConfigurations.IndexOf(existingConfig);
            playerRoundConfigurations[index] = newRoundConfiguration;
        }
        else
        {
            // Add the new RoundConfiguration to the list if not found
            playerRoundConfigurations.Add(newRoundConfiguration);
        }
    }
}
