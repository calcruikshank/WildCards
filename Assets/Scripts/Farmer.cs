using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Farmer : MonoBehaviour
{
    public CardData cardData;
    public Controller playerOwningFarmer;
    public CardInHand originalCard;
    Transform originalCardTransform;


    internal void SetToPlayerOwningFarmer(Controller controller)
    {
        playerOwningFarmer = controller;
        SetOriginalCard(cardData);
    }
    private void OnMouseOver()
    {
        if (playerOwningFarmer.locallySelectedCard != null)
        {
            if (playerOwningFarmer.locallySelectedCard.cardData.cardType != SpellSiegeData.CardType.Spell)
            {
                playerOwningFarmer.locallySelectedCard.gameObject.SetActive(false);
            }
        }
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);

        originalCardTransform.transform.localScale = Vector3.one * 200 / originalCardTransform.transform.position.z;
        originalCardTransform.gameObject.SetActive(true);


        if (playerOwningFarmer.state == Controller.State.NothingSelected)
        {
            playerOwningFarmer.currentFarmerHoveringOver = this;

        }
    }

    private void OnMouseExit()
    {
        //if (playerOwningCreature.locallySelectedCreature != this)
        //{
        if (originalCardTransform != null)
        {
            originalCardTransform.gameObject.SetActive(false);
        }
        if (playerOwningFarmer.locallySelectedCard != null)
        {
            playerOwningFarmer.locallySelectedCard.gameObject.SetActive(true);
        }

        playerOwningFarmer.currentFarmerHoveringOver = null;
        //}
    }
    internal void SetOriginalCard(CardData cardSelected)
    {
        originalCard = GameManager.singleton.GetCardAssociatedWithType(cardSelected.cardAssignedToObject).GetComponent<CardInHand>();
        originalCardTransform = Instantiate(originalCard.transform, GameManager.singleton.scalableUICanvas.transform);
        originalCardTransform.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        originalCardTransform.transform.localEulerAngles = Vector3.zero;
        originalCardTransform.transform.localScale = originalCardTransform.transform.localScale * 2f;

        originalCardTransform.GetComponentInChildren<BoxCollider>().enabled = false;
        originalCardTransform.gameObject.SetActive(false);

        foreach (Image img in originalCardTransform.GetComponentsInChildren<Image>())
        {
            img.enabled = true;
            img.gameObject.SetActive(true);
            Color imageColor = img.color;
            imageColor.a = 1f;
            img.color = imageColor;
        }
        foreach (TextMeshProUGUI tmp in originalCardTransform.GetComponentsInChildren<TextMeshProUGUI>())
        {
            Color imageColor = tmp.color;
            imageColor.a = 1f;
            tmp.color = imageColor;
        }
    }
    public void HideVisuals()
    {

        playerOwningFarmer.currentCreatureHoveringOver = null;
        //if (playerOwningCreature.locallySelectedCreature != this)
        //{
        if (originalCardTransform != null)
        {
            originalCardTransform.gameObject.SetActive(false);
        }
        if (playerOwningFarmer.locallySelectedCard != null)
        {
            playerOwningFarmer.locallySelectedCard.gameObject.SetActive(true);
        }
    }
    private void OnDestroy()
    {
        OnMouseExit();
    }
}
