using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CardInHand : MonoBehaviour
{
    [SerializeField]public Transform GameObjectToInstantiate;
    public int indexOfCard;

    public int greenManaCost;
    public int whiteManaCost;
    public int blackManaCost;
    public int redManaCost;
    public int genericManaCost;

    public int remainingMana;

    public int tier;

    public int currentAttack;
    public int currentHealth;

    [SerializeField] public Image cardArt;
    [SerializeField] public Image[] raritySymbols;

    [SerializeField] public TextMeshProUGUI attackText;
    [SerializeField] public TextMeshProUGUI healthText;
    [SerializeField] public TextMeshProUGUI cardAbilityText;

    public Keywords keywords;

    [SerializeField] TextMeshProUGUI greenManaText;
    [SerializeField] TextMeshProUGUI redManaText;
    [SerializeField] TextMeshProUGUI whiteManaText;
    [SerializeField] TextMeshProUGUI blackManaText;
    [SerializeField] TextMeshProUGUI blueManaText;

    [SerializeField] TextMeshProUGUI genericManaText;

    Transform purchasableGlow;

    SpellSiegeData.ManaType manaType;

    public Controller playerOwningCard;

    public bool isPurchasable;

    public SpellSiegeData.Cards cardAssignedToObject;


    public TextMeshProUGUI cardTitle;

    public SpellSiegeData.CardType cardType;

    public SpellSiegeData.CreatureType creatureType;
    public SpellSiegeData.cardRarity rarity;
    public SpellSiegeData.travType traversableType;
    // Start is called before the first frame update

    float discardPositiony;

    private void Awake()
    {
        UpdateMana();

        UpdateAttack();
        UpdateRarity();
    }

    public void UpdateAttack()
    {
        if (this.cardType == SpellSiegeData.CardType.Spell || this.cardType == SpellSiegeData.CardType.Structure)
        {
            attackText.transform.parent.gameObject.SetActive(false);
            healthText.transform.parent.gameObject.SetActive(false);
        }
        if (this.cardType == SpellSiegeData.CardType.Creature)
        {
            attackText.transform.parent.gameObject.SetActive(true);
            healthText.transform.parent.gameObject.SetActive(true);

            this.attackText.text = currentAttack.ToString();
            this.healthText.text = currentHealth.ToString();
        }
    }

    void Start()
    {
        if (purchasableGlow == null)
        {
            purchasableGlow = Instantiate(GameManager.singleton.purchasableGlow, this.transform);
            purchasableGlow.SetAsFirstSibling();
            purchasableGlow.gameObject.SetActive(false);
        }
    }


    [SerializeField] Sprite commonRarityImage;
    [SerializeField] Sprite uncommon;
    [SerializeField] Sprite rare;
    [SerializeField] Sprite mythic;
    [SerializeField] Sprite legendary;
    public void UpdateRarity()
    {
        foreach (Image i in raritySymbols)
        {
            switch (rarity)
            {
                case SpellSiegeData.cardRarity.common:
                    i.sprite = commonRarityImage;
                    break;
                case SpellSiegeData.cardRarity.uncommon:
                    i.sprite = uncommon;
                    break;
                case SpellSiegeData.cardRarity.rare:
                    i.sprite = rare;
                    break;
                case SpellSiegeData.cardRarity.mythic:
                    i.sprite = mythic;
                    break;
                case SpellSiegeData.cardRarity.Legendary:
                    i.sprite = legendary;
                    break;
            }
        }
    }
    public void UpdateMana()
    {
        if (genericManaCost == 0)
        {
            if (genericManaText != null)
            {
                genericManaText.transform.parent.gameObject.SetActive(false);
            }
        }
        if (greenManaCost == 0)
        {
            greenManaText.transform.parent.gameObject.SetActive(false);
        }
        if (blackManaCost == 0)
        {
            blackManaText.transform.parent.gameObject.SetActive(false);
        }
        if (whiteManaCost == 0)
        {
            whiteManaText.transform.parent.gameObject.SetActive(false);
        }
        if (redManaCost == 0)
        {
            redManaText.transform.parent.gameObject.SetActive(false);
        }




        if (greenManaCost != 0)
        {
            greenManaText.transform.parent.gameObject.SetActive(true);
            greenManaText.text = greenManaCost.ToString();
        }
        if (blackManaCost != 0)
        {
            blackManaText.transform.parent.gameObject.SetActive(true);
            blackManaText.text = blackManaCost.ToString();
        }
        if (whiteManaCost != 0)
        {
            whiteManaText.transform.parent.gameObject.SetActive(true);
            whiteManaText.text = whiteManaCost.ToString();
        }
        if (redManaCost != 0)
        {
            redManaText.transform.parent.gameObject.SetActive(true);
            redManaText.text = redManaCost.ToString();
        }
    }

    void Update()
    {
    }

    public virtual void CheckToSeeIfPurchasable(PlayerResources resources)
    {
        if (purchasableGlow == null)
        {
            purchasableGlow = Instantiate(GameManager.singleton.purchasableGlow, this.transform);
            purchasableGlow.SetAsFirstSibling();
            purchasableGlow.gameObject.SetActive(false);
        }
        int tempRedMana;
        int tempGreenMana;
        int tempWhiteMana;
        int tempBlackMana;
        int tempGenericMana;

        tempRedMana = resources.redMana - redManaCost;
        tempGreenMana = resources.greenMana - greenManaCost;
        tempWhiteMana = resources.whiteMana - whiteManaCost;
        tempBlackMana = resources.blackMana - blackManaCost;

        if (tempRedMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempGreenMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempWhiteMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        if (tempBlackMana < 0)
        {

            SetToNotPurchasable();
            return;
        }
        tempGenericMana = tempBlackMana + tempGreenMana + tempWhiteMana + tempRedMana;
        if (tempGenericMana < genericManaCost)
        {
            SetToNotPurchasable();
            return;
        }

        SetToPurchasable();

        /*remainingMana = tempBlueMana + tempRedMana + tempGreenMana + tempWhiteMana + tempBlackMana;
        if (remainingMana >= genericManaCost)
        {
        }*/
    }

    private void SetToPurchasable()
    {
        if (purchasableGlow != null)
        {
            purchasableGlow.gameObject.SetActive(true);
        }
        isPurchasable = true;
    }

    private void SetToNotPurchasable()
    {
        if (purchasableGlow != null)
        {
            purchasableGlow.gameObject.SetActive(false);
        }
        isPurchasable = false;
    }
    GameObject visualVersion;
    private void OnMouseOver()
    {
        Debug.Log("Mouse over");
        if (purchasableGlow != null)
        {
            purchasableGlow.gameObject.SetActive(true);
        }
        if (playerOwningCard != null)
        {
            if (playerOwningCard.locallySelectedCard != null)
            {
                playerOwningCard.locallySelectedCard.gameObject.SetActive(false);
            }
            if (visualVersion == null && playerOwningCard.locallySelectedCard == null)
            {
                visualVersion = Instantiate(this.gameObject, GameManager.singleton.canvasMain.transform);
                visualVersion.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + .525f);
                visualVersion.transform.localEulerAngles = Vector3.zero;
                visualVersion.transform.localScale = visualVersion.transform.localScale * 2.5f;
                visualVersion.GetComponentInChildren<Collider>().enabled = false;
            }
        }
        else
        {
        }
    }

    private void OnMouseExit()
    {
        if (playerOwningCard != null)
        {
            if (playerOwningCard.locallySelectedCard != null)
            {
                playerOwningCard.locallySelectedCard.gameObject.SetActive(true);
            }
        }
        TurnOffVisualCard();
        if (purchasableGlow != null)
        {
            purchasableGlow.gameObject.SetActive(false);
        }
    }

    internal void TurnOffVisualCard()
    {
        if (visualVersion != null)
        {
            Destroy(visualVersion);
        }
    }

    internal void DiscardAnimation()
    {
        this.GetComponent<Collider>().enabled = false;
        this.transform.parent = null;
    }
}
