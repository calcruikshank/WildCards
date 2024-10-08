using UnityEngine;

public class LogicForPurchasingCards : MonoBehaviour
{
    private void OnMouseOver()
    {
        if (GameManager.singleton.playerInScene.locallySelectedCard != null)
        {
            if (GameManager.singleton.playerInScene.locallySelectedCard.playerOwningCard == null)
            {
                GameManager.singleton.playerInScene.SetToPurchaseACard(GameManager.singleton.playerInScene.locallySelectedCard);
            }
        }
    }

    private void OnMouseExit()
    {
        GameManager.singleton.playerInScene.SetToDontPurchaseCard();
    }
}
