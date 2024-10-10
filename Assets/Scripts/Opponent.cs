using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Opponent : Controller
{
    protected override void Start()
    {
    }

    public void StartFromGameManager()
    {
        string directoryPath = $"{Application.persistentDataPath}/playerData/";

        if (!Directory.Exists(directoryPath) || Directory.GetFiles(directoryPath, "*.txt").Length == 0)
        {
            // No player data found, so create a new player
            playerData = new PlayerData();
            playerData.currentRound = 0;
            playerData.playerRoundConfigurations = new List<RoundConfiguration>();
            string newPlayerGuid = GenerateNewPlayerGuid();
            allOwnedCardsInScene = new List<CardData>();
            SavePlayerConfigLocally(playerData, newPlayerGuid);
            Debug.Log($"New player created with GUID: {newPlayerGuid}");

            currentGUIDForPlayer = newPlayerGuid;
        }
        else
        {
            // Existing player data found, load the first available player's data
            string[] existingFiles = Directory.GetFiles(directoryPath, "*.txt");
            string existingPlayerGuid = Path.GetFileNameWithoutExtension(existingFiles[0]); // Assuming you want to load the first file found

            playerData = GrabPlayerDataByGuid(existingPlayerGuid);
            currentGUIDForPlayer = existingPlayerGuid;
            Debug.Log($"Loaded existing player data with GUID: {existingPlayerGuid}");
        }

        GrabAllObjectsFromGameManager();
        mousePositionScript = GetComponent<MousePositionScript>();

        GameManager.singleton.playerList.Add(this);
        StartGame();



        InstantiateCardsBasedOnPlayerData(playerData);
        goldAmount = playerData.currentRound + 3;
        if (goldAmount > 10)
        {
            goldAmount = 10;
        }

    }


    public override void InstantiateCardsBasedOnPlayerData(PlayerData playerDataSent)
    {
        RoundConfiguration roundConfiguration = GrabBuildByCurrentRound(playerDataSent, playerDataSent.currentRound);

        if (roundConfiguration == null)
        {
            Debug.LogError("RoundConfiguration is null. Cannot proceed with card instantiation.");
            return;
        }

        if (roundConfiguration.allOwnedCards != null && roundConfiguration.allOwnedCards.Count > 0)
        {
            foreach (CardData cardData in roundConfiguration.allOwnedCards)
            {
                if (cardData != null && cardData.isInHand)
                {
                    InstantiateCardInHand(cardData);
                }
                else
                {
                    CardInHand cardToImmediatelyPlay = InstantiateCardInHand(cardData);
                    if (cardData.cardType == SpellSiegeData.CardType.Creature)
                    {
                        CastCreatureOnTile(cardToImmediatelyPlay, MirrorVector(cardData.positionOnBoard));
                    }
                    if (cardData.cardType == SpellSiegeData.CardType.Farmer)
                    {
                        CastFarmerOnTile(cardToImmediatelyPlay);
                        PurchaseHarvestTile(MirrorVector(cardData.positionOnBoard));
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No cards found in roundConfiguration.allOwnedCards.");
        }
    }

    protected override void StartGame()
    {
        StartGameCoroutine();
        SpawnCastleForPlayer(MirrorVector(new Vector3Int(-9, 0, 0)));
        OnTurn();
    }
    public override void StartGameCoroutine()
    {
        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        SpawnHUD();
        resources = new PlayerResources();
        resourcesChanged += UpdateHudForResourcesChanged;

        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        locallySelectedCard = null;
    }
    protected override void SpawnHUD()
    {
        //instantiatedPlayerUI = Instantiate(playerHud, canvasMain.transform);
        cardParent = instantiatedPlayerUI.GetComponentInChildren<CustomHorizontalLayoutGroup>().transform;
        //cardParent.gameObject.GetComponent<Image>().color = transparentCol;
        hudElements = instantiatedPlayerUI.GetComponent<HudElements>();
    }
    protected override void SpawnCastleForPlayer(Vector3 position)
    {

        GameObject castleInstance = Instantiate(castlePrefab, position, Quaternion.identity);


        instantiatedCaste = castleInstance.GetComponent<PlayerKeep>();
        LocalPlaceCastle(MirrorVector( new Vector3Int(-7, 0, 0) ));

    }
    protected override void Update()
    {
        
    }
    public static Vector3Int MirrorVector(Vector3Int input, int center = 0)
{
    // Calculate the mirrored position using the center point
    int mirroredX = 2 * center - input.x;
    
    // Add one to mirroredX only if y is even
    if (input.y % 2 == 0)
    {
        mirroredX += 1;
    }

    return new Vector3Int(mirroredX, -input.y, input.z);
}

}
