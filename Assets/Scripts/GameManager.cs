using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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


    public VisualAttackParticle rangedVisualAttackParticle;


    [SerializeField] GameObject parentOfCardSelections;

    public List<GameObject> cardChoices = new List<GameObject>();

     int currentMaxCardChoices = 5;


    public float turnTimer;
    public float turnThreshold = 1f;

    private void Awake()
    {
        if (singleton != null) Destroy(this);
        singleton = this;
        state = State.Setup;
        turnTimer = 0;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnCardChoices();
    }

    public Controller playerInScene;
    public Controller opponentInScene;

    [SerializeField]public List<GameObject> allCardsInGame;

    public void SpawnCardChoices()
    {
        if (cardChoices.Count > 0)
        {
            for (int i = 0; i < cardChoices.Count; i++)
            {
                Destroy(cardChoices[i]);
            }
        }
        cardChoices.Clear();
        for (int i = 0; i < currentMaxCardChoices; i++)
        {
            GameObject cardGrabbed = GrabRandomCard();
            GameObject instantiatedCard = Instantiate(cardGrabbed.gameObject, parentOfCardSelections.transform);
            cardChoices.Add(instantiatedCard);
        }
    }

    private GameObject GrabRandomCard()
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
        if (state == State.Game)
        {
            turnTimer += Time.deltaTime;
            if (turnTimer >= turnThreshold)
            {
                TriggerTurn();
            }
        }
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

        if (state == State.Game)
        {
            if (playerInScene.creaturesOwned.Count <= 0 && opponentInScene.creaturesOwned.Count <= 0)
            {
                EndRound();
            }
        }
    }

    private void EndRound()
    {
        playerInScene.StartRound();
        playerInScene.hoveringOverSubmit = false;
        state = State.Setup;
        foreach (KeyValuePair<int, Creature> creature in allCreaturesOnField)
        {
            creature.Value.StopFighting();
        }
        SpawnCardChoices();
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
    public GameObject GetCardAssociatedWithType(SpellSiegeData.Cards cardGrabbed)
    {
        GameObject selectedObject;

        selectedObject = allCardsInGame.FirstOrDefault(s => s.GetComponent<CardInHand>().cardData.cardAssignedToObject == cardGrabbed);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find prefab associated with -> " + cardGrabbed + " defaulting to 0");
            selectedObject = allCardsInGame[0];
        }
        return selectedObject;
    }
    public SpellSiegeData.Cards GetCardAssociatedWithRarity(SpellSiegeData.cardRarity cardRaritySent)
    {
        GameObject selectedObject;

        List<GameObject> shuffledListOfAllCards = Shuffle(allCardsInGame);
        selectedObject = shuffledListOfAllCards.FirstOrDefault(s => s.GetComponent<CardInHand>().cardData.rarity == cardRaritySent);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find rarity associated with -> " + cardRaritySent + " defaulting to null");
        }
        return selectedObject.GetComponent<CardInHand>().cardData.cardAssignedToObject;
    }
    public SpellSiegeData.Cards GetCardAssociatedWithTier(int tierSent)
    {
        //temp todo remove next line
        tierSent = 1;


        GameObject selectedObject;

        List<GameObject> shuffledListOfAllCards = Shuffle(allCardsInGame);
        selectedObject = shuffledListOfAllCards.FirstOrDefault(s => s.GetComponent<CardInHand>().cardData.tier == tierSent);

        if (selectedObject == null)
        {
            Debug.LogError("Could not find rarity associated with -> " + tierSent + " defaulting to null");
        }
        return selectedObject.GetComponent<CardInHand>().cardData.cardAssignedToObject;
    }

    public List<GameObject> Shuffle(List<GameObject> alpha)
    {
        for (int i = 0; i < alpha.Count; i++)
        {
            GameObject temp = alpha[i];
            int randomIndex = UnityEngine.Random.Range(i, alpha.Count);
            alpha[i] = alpha[randomIndex];
            alpha[randomIndex] = temp;
        }
        return alpha;
    }

    internal void StartGame()
    {
        playerInScene.hoveringOverSubmit = false;
        foreach (KeyValuePair<int, Creature> creature in allCreaturesOnField)
        {
            creature.Value.StartFighting();
        }
        state = State.Game;
        TriggerTurn();
    }

    public void TriggerTurn()
    {
        playerInScene.OnTurn();
        opponentInScene.OnTurn();
        playerInScene.OnMove();
        opponentInScene.OnMove();
        turnTimer = 0;
    }
}
