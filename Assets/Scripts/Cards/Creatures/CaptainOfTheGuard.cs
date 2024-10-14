using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainOfTheGuard : Creature
{
    List<Creature> creaturesDoubled = new List<Creature>();

    public override void OnCombatStart()
    {
        base.OnCombatStart();
        GiveCountersOnCombatStart();
    }


    public void GiveCountersOnCombatStart()
    {
        CheckForCreaturesWithinRange();
        GiveCounterToCreeaturesInRange();
        base.CheckForCreaturesWithinRange();

    }

    private void GiveCounterToCreeaturesInRange()
    {
        foreach (Creature friendly in friendlyCreaturesWithinRange)
        {
            if (!creaturesDoubled.Contains(friendly))
            {
                friendly.GiveCounter(1);
            }
        }
        creaturesDoubled = friendlyCreaturesWithinRange;
    }

}
