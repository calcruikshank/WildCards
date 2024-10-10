using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CardInHand : MonoBehaviour
{
    #region Fields and Properties

    // Reference to CardData
    [SerializeField] public CardData cardData;
    [SerializeField] public GameObject GameObjectToInstantiate;

    // UI Components
    [SerializeField] private Image cardArt;
    [SerializeField] private Image[] raritySymbols;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI cardAbilityText;
    [SerializeField] private TextMeshProUGUI greenManaText;
    [SerializeField] private TextMeshProUGUI redManaText;
    [SerializeField] private TextMeshProUGUI whiteManaText;
    [SerializeField] private TextMeshProUGUI blackManaText;
    [SerializeField] private TextMeshProUGUI blueManaText;
    [SerializeField] private TextMeshProUGUI genericManaText;

    // Purchasable Information
    public bool isPurchasable;
    [HideInInspector]public Transform purchasableGlow;
    [HideInInspector] public GameObject visualVersion;

    [HideInInspector] public Controller playerOwningCard;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        UpdateMana();
        UpdateAttack();
        //UpdateRarity();
    }

    void Start()
    {
        InitializePurchasableGlow();
    }

    private void Update() { }

    private void OnMouseOver()
    {
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

            if (visualVersion == null) // Instantiate the visual card only if it doesn't already exist
            {
                visualVersion = Instantiate(this.gameObject, GameManager.singleton.canvasMain.transform);
                visualVersion.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + 0.525f);
                visualVersion.transform.localEulerAngles = Vector3.zero;
                visualVersion.transform.localScale *= 2.5f;
                visualVersion.GetComponentInChildren<Collider>().enabled = false;
            }
        }
    }

    private void OnMouseExit()
    {
        if (playerOwningCard != null && playerOwningCard.locallySelectedCard != null)
        {
            playerOwningCard.locallySelectedCard.gameObject.SetActive(true);
        }

        TurnOffVisualCard();

        if (purchasableGlow != null)
        {
            purchasableGlow.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Initialization Methods

    private void InitializePurchasableGlow()
    {
        if (purchasableGlow == null)
        {
            purchasableGlow = Instantiate(GameManager.singleton.purchasableGlow, this.transform);
            purchasableGlow.SetAsFirstSibling();
            purchasableGlow.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Update Methods

    public void UpdateAttack()
    {
        if (cardData.cardType == SpellSiegeData.CardType.Spell || cardData.cardType == SpellSiegeData.CardType.Structure)
        {
            attackText.transform.parent.gameObject.SetActive(false);
            healthText.transform.parent.gameObject.SetActive(false);
        }
        else if (cardData.cardType == SpellSiegeData.CardType.Creature)
        {
            attackText.transform.parent.gameObject.SetActive(true);
            healthText.transform.parent.gameObject.SetActive(true);
            attackText.text = cardData.currentAttack.ToString();
            healthText.text = cardData.currentHealth.ToString();
        }
    }

    public void UpdateRarity()
    {
        foreach (Image i in raritySymbols)
        {
            switch (cardData.rarity)
            {
                case SpellSiegeData.cardRarity.common:
                    //i.sprite = cardData.commonRarityImage;
                    break;
                case SpellSiegeData.cardRarity.uncommon:
                    //i.sprite = cardData.uncommonImage;
                    break;
                case SpellSiegeData.cardRarity.rare:
                    //i.sprite = cardData.rareImage;
                    break;
                case SpellSiegeData.cardRarity.mythic:
                    //i.sprite = cardData.mythicImage;
                    break;
                case SpellSiegeData.cardRarity.Legendary:
                    //i.sprite = cardData.legendaryImage;
                    break;
            }
        }
    }

    public void UpdateMana()
    {
        SetManaTextVisibility(genericManaText, cardData.genericManaCost);
        SetManaTextVisibility(greenManaText, cardData.greenManaCost);
        SetManaTextVisibility(blackManaText, cardData.blackManaCost);
        SetManaTextVisibility(whiteManaText, cardData.whiteManaCost);
        SetManaTextVisibility(redManaText, cardData.redManaCost);
    }

    private void SetManaTextVisibility(TextMeshProUGUI manaText, int manaCost)
    {
        if (manaText != null)
        {
            manaText.transform.parent.gameObject.SetActive(manaCost != 0);
            if (manaCost != 0)
            {
                manaText.text = manaCost.ToString();
            }
        }
    }

    #endregion

    #region Purchase Methods

    public virtual void CheckToSeeIfPurchasable(PlayerResources resources)
    {
        InitializePurchasableGlow();

        int tempRedMana = resources.redMana - cardData.redManaCost;
        int tempGreenMana = resources.greenMana - cardData.greenManaCost;
        int tempWhiteMana = resources.whiteMana - cardData.whiteManaCost;
        int tempBlackMana = resources.blackMana - cardData.blackManaCost;
        int tempGenericMana = tempBlackMana + tempGreenMana + tempWhiteMana + tempRedMana;

        if (tempRedMana < 0 || tempGreenMana < 0 || tempWhiteMana < 0 || tempBlackMana < 0 || tempGenericMana < cardData.genericManaCost)
        {
            SetToNotPurchasable();
            return;
        }

        SetToPurchasable();
    }

    private void SetToPurchasable()
    {
        purchasableGlow?.gameObject.SetActive(true);
        isPurchasable = true;
    }

    private void SetToNotPurchasable()
    {
        purchasableGlow?.gameObject.SetActive(false);
        isPurchasable = false;
    }

    #endregion

    #region Visual Card Methods

    private void InstantiateVisualCard()
    {
        if (visualVersion == null && playerOwningCard?.locallySelectedCard != null)
        {
            visualVersion = Instantiate(this.gameObject, GameManager.singleton.canvasMain.transform);
            visualVersion.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + 0.525f);
            visualVersion.transform.localEulerAngles = Vector3.zero;
            visualVersion.transform.localScale *= 2.5f;
            visualVersion.GetComponentInChildren<Collider>().enabled = false;
        }
    }

    internal void TurnOffVisualCard()
    {
        purchasableGlow?.gameObject.SetActive(false);
        if (visualVersion != null)
        {
            Destroy(visualVersion);
        }
    }

    #endregion

    #region Animation Methods

    internal void DiscardAnimation()
    {
        this.GetComponent<Collider>().enabled = false;
        this.transform.parent = null;
    }

    #endregion
}

[Serializable]
public class CardData
{
    // Mana Costs
    public int greenManaCost;
    public int whiteManaCost;
    public int blackManaCost;
    public int redManaCost;
    public int genericManaCost;

    public int tier;
    public int currentAttack;
    public int currentHealth;
    public int range;

    // Card Visual Information
    public string cardTitle;
    public Sprite cardArtSprite;
    public Sprite commonRarityImage;
    public Sprite uncommonImage;
    public Sprite rareImage;
    public Sprite mythicImage;
    public Sprite legendaryImage;

    public SpellSiegeData.ManaType manaType;
    public SpellSiegeData.Cards cardAssignedToObject;
    public SpellSiegeData.CardType cardType;
    public SpellSiegeData.CreatureType creatureType;
    public SpellSiegeData.cardRarity rarity;
    public SpellSiegeData.travType traversableType;

    public Vector3Int positionOnBoard;
    public bool isInHand;
    public int purchaseCost = 3;
    public int numberOfTimesThisCanDie = 1;

    [SerializeField] public List<SpellSiegeData.Keywords> keywords;
    // Constructor to initialize with default values if necessary
    public CardData()
    {
        // Add default initialization if required
    }

    // Method to save card data to player data
    public void SaveCardDataToPlayerData(PlayerData playerData, int round)
    {
        // Find the RoundConfiguration for the specified round
        RoundConfiguration roundConfig = playerData.playerRoundConfigurations.Find(r => r.round == round);

        if (roundConfig == null)
        {
            // If no configuration exists for this round, create a new one
            roundConfig = new RoundConfiguration { round = round, allOwnedCards = new List<CardData>() };
            playerData.playerRoundConfigurations.Add(roundConfig);
        }

        // Check if the card already exists in the round's owned cards list
        CardData existingCard = roundConfig.allOwnedCards.Find(card => card.cardTitle == this.cardTitle);

        if (existingCard != null)
        {
            // Update the existing card's data
            int index = roundConfig.allOwnedCards.IndexOf(existingCard);
            roundConfig.allOwnedCards[index] = this;
        }
        else
        {
            // Add the new card data to the list
            roundConfig.allOwnedCards.Add(this);
        }
    }
}


