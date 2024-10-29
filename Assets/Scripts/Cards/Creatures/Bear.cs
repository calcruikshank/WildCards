using UnityEngine;

public class Bear : Leader
{
    public override void OnCombatStart()
    {
        base.OnCombatStart();
        float highestCurrentHealth = 0;
        foreach (Creature creature in playerOwningCreature.creaturesOwned)
        {
            if (creature != this)
            {
                if (creature.CurrentHealth > highestCurrentHealth)
                {
                    highestCurrentHealth = creature.CurrentHealth;
                }
            }
        }
        GiveHealth((int)highestCurrentHealth);
    }
}
