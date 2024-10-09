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
        //InstantiateCardsBasedOnPlayerData(playerData);
        goldAmount = playerData.currentRound + 3;
        if (goldAmount > 10)
        {
            goldAmount = 10;
        }

    }
    protected override void StartGame()
    {
        StartGameCoroutine();
        SpawnCastleForPlayer(new Vector3(10, 0, 0));
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

        LocalPlaceCastle(new Vector3Int(8, 0, 0));

    }
    protected override void Update()
    {
        
    }
}
