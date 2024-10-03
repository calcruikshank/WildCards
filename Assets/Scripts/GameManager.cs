using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static SpellSiegeData;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    public static GameManager singleton;
    public enum State
    {
        Setup, //The state for placing your castle
        Game,
        End //Setup for scaling
    }
    public State state;
    public Grid grid;
    public Tilemap baseMap;
    public Tilemap enviornmentMap;
    public Tilemap waterTileMap;
    public Tilemap highlightMap;
    public Material RenderInFrontMat;
    public Material TransparentSharedMat;
    public Material rangeIndicatorMat;
    public Material OpaqueSharedMat;
    public Transform castleTransform;
    public TileBase highlightTile;
    public GameObject highlightForBaseTiles;

    public Transform cardParent;
    public Transform canvasMain;
    public List<Controller> playerList = new List<Controller>();
    public List<Controller> playersThatHavePlacedCastle = new List<Controller>();
    public List<Controller> playersThatHaveBeenReceived = new List<Controller>();

    public Transform canAttackIcon;
    [SerializeField] public Camera mainCam;
    public Dictionary<int, Creature> allCreaturesOnField = new Dictionary<int, Creature>();

    //public float tickTimeAverage;
    int playerCount; //TODO set this equal to players in scene and return if a player has not hit

    public int allCreatureGuidCounter;

    public int endingX;
    public int endingY;
    public int startingX = -10;
    public int startingY = 9;

    public Transform blueManaSymbol;
    public Transform redManaSymbol;
    public Transform greenManaSymbol;
    public Transform blackManaSymbol;
    public Transform whiteManaSymbol;

    public Transform purchasableGlow;

    [SerializeField] TextMeshPro damageText;

    [SerializeField] public Transform onDeathEffect;
    Transform instantiatedDamageText;
    Transform instantiatedHealthText;

    [SerializeField] public Canvas RectCanvas;
    [SerializeField] public Canvas scalableUICanvas;

    public bool hasStartedGame = false;
    public int turnTimer;


    public VisualAttackParticle rangedVisualAttackParticle;


    [SerializeField] GameObject parentOfCardSelections;

    public List<CardInHand> cardChoices;

    public int currentMaxCardChoices = 3;
    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnCardChoices();
    }

    public Controller playerInScene;

    [SerializeField]public List<CardInHand> allCardsInGame;


    public void SpawnCardChoices()
    {
        for (int i = 0; i < currentMaxCardChoices; i++)
        {
            CardInHand cardGrabbed = GrabRandomCard();
            GameObject instantiatedCard = Instantiate(cardGrabbed.gameObject, parentOfCardSelections.transform);
        }
    }

    private CardInHand GrabRandomCard()
    {
        int tierGrabbed = UnityEngine.Random.Range(1, playerInScene.farmersOwned.Count);
        if (tierGrabbed > 5)
        {
            tierGrabbed = 5;
        }
        return GetCardAssociatedWithType(GetCardAssociatedWithTier(tierGrabbed));
    }
    void Update()
    {
        
    }

    internal void CreatureDied(int creatureID)
    {
        if (allCreaturesOnField[creatureID] != null)
        {
            foreach (KeyValuePair<int, Creature> kvp in allCreaturesOnField)
            {
                kvp.Value.OtherCreatureDied(allCreaturesOnField[creatureID]);
            }
        }
    }

    internal void CreatureEntered(int creatureID)
    {
        foreach (KeyValuePair<int, Creature> kvp in allCreaturesOnField)
        {
            kvp.Value.OtherCreatureEntered(allCreaturesOnField[creatureID]);
        }
    }
    public void SpawnDamageText(Vector3 positionSent, float damageSent)
    {
        Transform instantiatedDamageText = Instantiate(damageText.transform, positionSent, Quaternion.identity);
        instantiatedDamageText.localEulerAngles = new Vector3(90, 0, 0);
        instantiatedDamageText.GetComponent<TextMeshPro>().text = damageSent.ToString();
    }
    [SerializeField] Transform healParticle;
    internal void SpawnHealText(Vector3 positionSent, float amount)
    {
        Transform instantiatedHealthText = Instantiate(damageText.transform, positionSent, Quaternion.identity);
        instantiatedHealthText.localEulerAngles = new Vector3(90, 0, 0);
        instantiatedHealthText.GetComponent<TextMeshPro>().text = amount.ToString();
        instantiatedHealthText.GetComponent<TextMeshPro>().color = Color.green;
        Instantiate(healParticle, positionSent, Quaternion.identity);
    }
    public CardInHand GetCardAssociatedWithType(SpellSiegeData.Cards cardGrabbed)
    {
        CardInHand selectedObject;

        selectedObject = allCardsInGame.FirstOrDefault(s => s.cardAssignedToObject == cardGrabbed);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find prefab associated with -> " + cardGrabbed + " defaulting to 0");
            selectedObject = allCardsInGame[0];
        }
        return selectedObject;
    }
    public SpellSiegeData.Cards GetCardAssociatedWithRarity(SpellSiegeData.cardRarity cardRaritySent)
    {
        CardInHand selectedObject;

        List<CardInHand> shuffledListOfAllCards = Shuffle(allCardsInGame);
        selectedObject = shuffledListOfAllCards.FirstOrDefault(s => s.rarity == cardRaritySent);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find rarity associated with -> " + cardRaritySent + " defaulting to null");
        }
        return selectedObject.cardAssignedToObject;
    }
    public SpellSiegeData.Cards GetCardAssociatedWithTier(int tierSent)
    {
        CardInHand selectedObject;

        List<CardInHand> shuffledListOfAllCards = Shuffle(allCardsInGame);
        selectedObject = shuffledListOfAllCards.FirstOrDefault(s => s.tier == tierSent);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find rarity associated with -> " + tierSent + " defaulting to null");
        }
        return selectedObject.cardAssignedToObject;
    }

    public List<CardInHand> Shuffle(List<CardInHand> alpha)
    {
        for (int i = 0; i < alpha.Count; i++)
        {
            CardInHand temp = alpha[i];
            int randomIndex = UnityEngine.Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
        return alpha;
    }
}
