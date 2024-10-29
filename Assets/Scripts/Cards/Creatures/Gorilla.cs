using UnityEngine;

public class Gorilla : Leader
{
    public override void OtherCreatureTookDamage(Creature creatureThatTookDamage)
    {
        base.OtherCreatureTookDamage(creatureThatTookDamage);
        this.GiveCounter(1);
    }
}
