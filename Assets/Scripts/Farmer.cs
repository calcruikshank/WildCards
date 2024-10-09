using System;
using UnityEngine;

public class Farmer : MonoBehaviour
{
    public CardData cardData;
    public Controller playerOwningFarmer;
    internal void SetToPlayerOwningFarmer(Controller controller)
    {
        playerOwningFarmer = controller;
    }
}
