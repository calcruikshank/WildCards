using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public bool isAI = false;
    public State state;
    public enum State
    {
        NothingSelected,
        CreatureInHandSelected,
        SpellInHandSelected,
        StructureInHandSeleced,
        PlacingCastle,
        SelectingDeck,
        FarmerInHandSelected,
        FarmerOnBoardSelected,
        CreatureOnBoardSelected,
        Waiting
    }

    public List<CardData> allOwnedCardsInScene;

    public int goldAmount = 0;
    public int turn = 0;

    public Dictionary<Vector3Int, BaseTile> tilesOwned = new Dictionary<Vector3Int, BaseTile>();


    protected MousePositionScript mousePositionScript;

    public Color col;
    public Color transparentCol;


    protected Vector3 mousePosition;
    protected TileBase highlightTile;
    protected Tilemap highlightMap;
    protected Tilemap baseMap;
    protected Tilemap environmentMap;
    protected Tilemap waterMap;
    protected Grid grid;
    protected Vector3Int previousCellPosition;

    protected Transform castle;
    public PlayerKeep instantiatedCaste;
    protected Vector3Int currentLocalHoverCellPosition;
    protected Vector3Int cellPositionSentToClients;
    protected Vector3Int targetedCellPosition;

    [SerializeField] protected LayerMask creatureMask;

    protected Vector3Int placedCellPosition;

    protected int maxHandSize = 9;
    [SerializeField] protected List<CardInHand> dragonDeck = new List<CardInHand>();
    [SerializeField] protected List<CardInHand> demonDeck = new List<CardInHand>();
    public List<CardInHand> cardsInDeck = new List<CardInHand>();
    public List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardInHand locallySelectedCard;
    public List<Vector3> allVertextPointsInTilesOwned = new List<Vector3>();

    [SerializeField] public Transform instantiatedPlayerUI;
    protected Transform cardParent;

    protected Canvas canvasMain;


    protected PlayerResources resources;

    public delegate void ResourcesChanged(PlayerResources resources);
    public ResourcesChanged resourcesChanged;

    //[SerializeField] protected Transform playerHud;
    protected HudElements hudElements;
    protected int numOfPurchasableHarvestTiles = 1;

    public List<BaseTile> harvestedTiles = new List<BaseTile>();

    public int spellCounter = 0;

    [SerializeField] protected Color[] colorsToPickFrom;


    public List<Structure> structuresOwned = new List<Structure>();


    public List<Farmer> farmersOwned = new List<Farmer>();


    public enum ActionTaken
    {
        LeftClickBaseMap,
        SelectedCardInHand,
        RightClick,
        TilePurchased
    }


    public Controller opponent;


    public bool hoveringOverSubmit;

    public Creature currentCreatureHoveringOver;
    public Farmer currentFarmerHoveringOver;

    // Start is called before the first frame update
    protected virtual void Start()
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

        SpawnAFarmerUnderFarmerParent();

        GameManager.singleton.opponentInScene.GetComponent<Opponent>().StartFromGameManager();

    }

    [SerializeField] Transform farmerParent;

    [SerializeField] CardInHand farmerCardInHand;
    protected void SpawnAFarmerUnderFarmerParent()
    {
        Instantiate(farmerCardInHand, farmerParent);
    }

    public List<ulong> gameSceneController = new List<ulong>();
    public void OnPlayerJoinedGameSceneServerRpc(ulong controllerSent)
    {
        gameSceneController.Add(controllerSent);
    }
    private void OnDestroy()
    {
    }
    private bool gameStarted = false;
    protected virtual void StartGame()
    {
        StartGameCoroutine();
        SpawnCastleForPlayer(new Vector3(-9, 0, 0));
        OnTurn();
    }

    [SerializeField] protected GameObject castlePrefab;

    protected virtual void SpawnCastleForPlayer(Vector3 position)
    {

        GameObject castleInstance = Instantiate(castlePrefab, position, Quaternion.identity);


        instantiatedCaste = castleInstance.GetComponent<PlayerKeep>();

        LocalPlaceCastle(new Vector3Int(-7, 0, 0));

    }
    public virtual void StartGameCoroutine()
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

    protected void GrabAllObjectsFromGameManager()
    {
        canvasMain = GameManager.singleton.canvasMain.GetComponent<Canvas>();
        highlightTile = GameManager.singleton.highlightTile;
        highlightMap = GameManager.singleton.highlightMap;
        baseMap = GameManager.singleton.baseMap;
        environmentMap = GameManager.singleton.enviornmentMap;
        waterMap = GameManager.singleton.waterTileMap;
        grid = GameManager.singleton.grid;
        castle = GameManager.singleton.castleTransform;
    }
    protected virtual void SpawnHUD()
    {
        //instantiatedPlayerUI = Instantiate(playerHud, canvasMain.transform);
        cardParent = instantiatedPlayerUI.GetComponentInChildren<CustomHorizontalLayoutGroup>().transform;
        //cardParent.gameObject.GetComponent<Image>().color = transparentCol;
        hudElements = instantiatedPlayerUI.GetComponent<HudElements>();
        instantiatedPlayerUI.gameObject.SetActive(true);
    }

    protected virtual void OnTurn()
    {
        StartTurnPhase();
    }



    public virtual void StartTurnPhase()
    {
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureInHandSelected:
                TriggerAllCreatureAbilities();
                break;
            case State.SpellInHandSelected:
                TriggerAllCreatureAbilities();
                break;
            case State.StructureInHandSeleced:
                TriggerAllCreatureAbilities();
                break;
        }
    }

    public Farmer selectedOnBoardFarmer;
    public Creature selectedOnBoardCreature;

    // Update is called once per frame
    protected virtual void Update()
    {
        currentLocalHoverCellPosition = grid.WorldToCell(mousePosition);
        mousePosition = mousePositionScript.GetMousePositionWorldPoint();
        if (currentLocalHoverCellPosition != previousCellPosition)
        {
            highlightMap.SetTile(previousCellPosition, null);
            highlightMap.SetTile(currentLocalHoverCellPosition, highlightTile);

            previousCellPosition = currentLocalHoverCellPosition;

        }

        if (locallySelectedCard != null)
        {
            if (locallySelectedCard.GetComponentInChildren<BoxCollider>().enabled == true)
            {
                locallySelectedCard.GetComponentInChildren<BoxCollider>().enabled = false;
                locallySelectedCard.gameObject.SetActive(true);
                foreach (Image img in locallySelectedCard.GetComponentsInChildren<Image>())
                {
                    Color imageColor = img.color;
                    imageColor.a = .4f;
                    img.color = imageColor;
                }
                foreach (TextMeshProUGUI tmp in locallySelectedCard.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    Color imageColor = tmp.color;
                    imageColor.a = .4f;
                    tmp.color = imageColor;
                }
            }
            var screenPoint = Input.mousePosition;
            screenPoint.z = Camera.main.transform.position.y - 3; //distance of the plane from the camera
            Vector3 cardPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            locallySelectedCard.transform.position = new Vector3(cardPosition.x, cardPosition.y, cardPosition.z);
        }

        if (state == State.CreatureOnBoardSelected)
        {
            if (selectedOnBoardCreature != null)
            {
                var screenPoint = Input.mousePosition;
                screenPoint.z = Camera.main.transform.position.y - 3; //distance of the plane from the camera
                Vector3 positionToMoveCreatureTo = Camera.main.ScreenToWorldPoint(screenPoint);
                selectedOnBoardCreature.transform.position = new Vector3(positionToMoveCreatureTo.x, positionToMoveCreatureTo.y, positionToMoveCreatureTo.z);
            }
        }
        if (state == State.FarmerOnBoardSelected)
        {
            if (selectedOnBoardFarmer != null)
            {
                var screenPoint = Input.mousePosition;
                screenPoint.z = Camera.main.transform.position.y - 3; //distance of the plane from the camera
                Vector3 positionToMoveCreatureTo = Camera.main.ScreenToWorldPoint(screenPoint);
                selectedOnBoardFarmer.transform.position = new Vector3(positionToMoveCreatureTo.x, positionToMoveCreatureTo.y, positionToMoveCreatureTo.z);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (cardToPurchase != null)
            {
                PurchaseCard();
                return;
            }
            if (locallySelectedCard != null || selectedOnBoardCreature || selectedOnBoardFarmer)
            {
                if (cellPositionSentToClients != null)
                {
                    if (cellPositionSentToClients != grid.WorldToCell(mousePosition))
                    {
                        if (selectedOnBoardCreature || selectedOnBoardFarmer)
                        {
                            LeftClickQueue(grid.WorldToCell(mousePosition));
                        }
                        if (!CheckForRaycast())
                        {
                            LeftClickQueue(grid.WorldToCell(mousePosition));
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (hoveringOverSubmit)
            {
                SubmitPlayerData();
            }
            if (cardToPurchase != null)
            {
                PurchaseCard();
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePositionWorldPoint;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, mousePositionScript.baseTileMap))
            {
                mousePositionWorldPoint = raycastHit.point;
                cellPositionSentToClients = grid.WorldToCell(mousePositionWorldPoint);
            }
            if (selectedOnBoardCreature || selectedOnBoardFarmer)
            {
                LeftClickQueue(cellPositionSentToClients);
            }
            if (currentFarmerHoveringOver && state != State.FarmerOnBoardSelected)
            {
                SetStateToFarmerOnBoardSelected();
                return;
            }
            if (currentCreatureHoveringOver && state != State.CreatureOnBoardSelected)
            {
                SetStateToCreatureOnBoardSelected();
                return;
            }
            if (!CheckForRaycast())
            {
                LeftClickQueue(cellPositionSentToClients);
            }


            if (state == State.NothingSelected && locallySelectedCard == null)
            {
                if (ShowingPurchasableHarvestTiles)
                {
                    if (CheckToSeeIfClickedHarvestTileCanBePurchased(cellPositionSentToClients))
                    {
                        if (numOfPurchasableHarvestTiles <= 0)
                        {
                        }
                        Debug.Log(ShowingPurchasableHarvestTiles + " fgmkoijiojio");
                        //ShowingPurchasableHarvestTiles = false;
                        PurchaseHarvestTile(cellPositionSentToClients);
                    }
                }
            }
        }

    }

    private void SetStateToCreatureOnBoardSelected()
    {
        if (state != State.NothingSelected)
        {
            return;
        }
        selectedOnBoardFarmer = null;
        selectedOnBoardCreature = null;
        selectedOnBoardCreature = currentCreatureHoveringOver;

        foreach (Collider col in selectedOnBoardCreature.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        selectedOnBoardCreature.HideVisuals();
        state = State.CreatureOnBoardSelected;
        ShowViablePlacableTilesDoNotRequireCardInHand();
    }

    private void SetStateToFarmerOnBoardSelected()
    {
        if (state != State.NothingSelected)
        {
            return;
        }
        selectedOnBoardFarmer = null;
        selectedOnBoardCreature = null;
        selectedOnBoardFarmer = currentFarmerHoveringOver;

        foreach (Collider col in selectedOnBoardFarmer.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        selectedOnBoardFarmer.HideVisuals(); 
        state = State.FarmerOnBoardSelected;
        ShowViablePlacableTilesDoNotRequireCardInHand();
    }

    private bool CheckToSeeIfClickedHarvestTileCanBePurchased(Vector3Int tilePositionSent)
    {
        if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent)))
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent))
            {
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent).playerOwningTile == this)
                {
                    return true;
                }
            }
        }
        return false;
    }
    protected void PurchaseHarvestTile(Vector3Int vector3Int)
    {
        //ShowingPurchasableHarvestTiles = false;
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int).isBeingHarvested)
        {
            return;
        }
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int));

        //IncreaseCostOfHarvestTiles();
    }
    private void HideHarvestedTiles()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            bt.Value.HideHarvestIcon();
            bt.Value.UnHighlightTile();
        }
        highlightedTiles.Clear();
        ShowingPurchasableHarvestTiles = false;
    }
    private void ShowHarvestedTiles()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (harvestedTiles.Contains(bt.Value))
            {
                bt.Value.ShowHarvestIcon();
            }
            if (!harvestedTiles.Contains(bt.Value))
            {
                bt.Value.HighlightTile();
                highlightedTiles.Add(bt.Value);
            }
        }
        ShowingPurchasableHarvestTiles = true;
    }
    public bool ShowingPurchasableHarvestTiles = false;
    void LeftClickQueue(Vector3Int positionSent)
    {
        if (selectedOnBoardCreature || selectedOnBoardFarmer)
        {
            if (CheckToSeeIfCanSpawnCreature(positionSent))
            {
                LocalLeftClick(positionSent);
                return;
            }
        }
        if (locallySelectedCard != null && locallySelectedCard.cardData.cardType == SpellSiegeData.CardType.Creature && locallySelectedCard.playerOwningCard != null)
        {
            if (CheckToSeeIfCanSpawnCreature(positionSent))
            {
                SpawnVisualCreatureOnTile(positionSent);
                LocalLeftClick(positionSent);
            }
            return;
        }
        if (locallySelectedCard != null && locallySelectedCard.cardData.cardType == SpellSiegeData.CardType.Spell && locallySelectedCard.playerOwningCard != null)
        {
            LocalLeftClick(positionSent);
            return;
        }
        if (locallySelectedCard != null && locallySelectedCard.cardData.cardType == SpellSiegeData.CardType.Farmer && locallySelectedCard.playerOwningCard != null)
        {
            LocalLeftClick(positionSent);
            return;
        }

    }

    public void SetVisualsToNothingSelectedLocally()
    {

        if (locallySelectedCard != null)
        {
            if (locallySelectedCardInHandToTurnOff != null)
            {
                locallySelectedCardInHandToTurnOff.gameObject.SetActive(true);
            }
            Destroy(locallySelectedCard.gameObject);
        }

        foreach (BaseTile bt in highlightedTiles)
        {
            bt.UnHighlightTile();
        }
        highlightedTiles.Clear();


    }

    private void FixedUpdate()
    {
        if (GameManager.singleton == null)
        {
            return;
        }
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                break;
            case State.CreatureInHandSelected:
                break;
            case State.SpellInHandSelected:
                break;
            case State.StructureInHandSeleced:
                break;
        }
    }

    protected void TriggerAllCreatureAbilities()
    {
        foreach (Creature kp in creaturesOwned)
        {
            kp.OnTurn();
            Debug.Log(kp + " triggering");
        }
        foreach (KeyValuePair<Vector3Int, BaseTile> kp in tilesOwned)
        {
            if (kp.Value.CreatureOnTile() != null && kp.Value.CreatureOnTile().playerOwningCreature == kp.Value.playerOwningTile)
            {
                kp.Value.CreatureOnTile().Garrison();
            }
        }
    }




    void AddIndexOfCardInHandToTickQueueLocal(int index)
    {
    }
    void AddIndexOfCreatureOnBoard(int index)
    {
        SetToCreatureOnFieldSelected(GameManager.singleton.allCreaturesOnField[index]);
    }


    void LocalLeftClick(Vector3Int positionSent)
    {
        Debug.Log(state + " state before farm check");
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                break;
            case State.CreatureInHandSelected:
                HandleCreatureInHandSelected(positionSent);
                break;
            case State.SpellInHandSelected:
                HandleSpellInHandSelected(positionSent);
                break;
            case State.StructureInHandSeleced:
                HandleStructureInHandSelected(positionSent);
                break;
            case State.FarmerInHandSelected:
                HandleFarmerInHandSelected(positionSent);
                break;
            case State.FarmerOnBoardSelected:
                HandleFarmerOnBoardSelected(positionSent);
                break;
            case State.CreatureOnBoardSelected:
                HandleCreatureOnBoardSelected(positionSent);
                break;
        }
    }

    private void HandleFarmerOnBoardSelected(Vector3Int cellSent)
    {
        RemoveTileFromHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(selectedOnBoardFarmer.cardData.positionOnBoard));
        PurchaseHarvestTile(cellSent);
        selectedOnBoardFarmer.cardData.positionOnBoard = cellSent;
        SetStateToNothingSelected();
    }

    private void HandleCreatureOnBoardSelected(Vector3Int cellSent)
    {
        creaturesOwned.Remove(selectedOnBoardCreature);
        selectedOnBoardCreature.cardData.positionOnBoard = cellSent;
        CardInHand cardToImmediatelyPlay = InstantiateCardInHand(selectedOnBoardCreature.cardData);
        CastCreatureOnTile(cardToImmediatelyPlay, selectedOnBoardCreature.cardData.positionOnBoard);
        Destroy(selectedOnBoardCreature.gameObject);

        SetStateToNothingSelected();
    }

    private void HandleFarmerInHandSelected(Vector3Int cellSent)
    {
        Debug.Log("handle farmer in hand");
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (locallySelectedCard != null)
        {
            if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)) && tilesOwned.ContainsValue(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
                if (environmentMap.GetInstantiatedObject(cellSent))
                {
                    GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
                    if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
                    {
                        instantiatedObject.AddComponent<ChangeTransparency>();
                    }
                    ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
                    instantiatedObjectsChangeTransparency.ChangeTransparent(100);

                    Destroy(instantiatedObject);
                }

                SetOwningTile(cellSent);
                locallySelectedCard.GetComponent<CardInHand>().cardData.positionOnBoard = cellSent;
                CastFarmerOnTile(locallySelectedCard.GetComponent<CardInHand>());
                SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());

                AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent));
                RemoveCardFromHand(locallySelectedCard);
                RemoveCardFromHand(locallySelectedCardInHandToTurnOff);
                Destroy(locallySelectedCardInHandToTurnOff.gameObject);
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
        }
    }
    public void CastFarmerOnTile(CardInHand cardInHandSent)
    {
        Debug.Log("Spawning creature on tile ");
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cardInHandSent.cardData.positionOnBoard);

        CardInHand cardAssociatedWithType = GameManager.singleton.GetCardAssociatedWithType(cardInHandSent.cardData.cardAssignedToObject).GetComponent<CardInHand>();
        GameObject instantiatedFarmer = Instantiate(cardAssociatedWithType.GameObjectToInstantiate, positionToSpawn, Quaternion.identity);
        if (environmentMap.GetInstantiatedObject(cardInHandSent.cardData.positionOnBoard))
        {
            GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cardInHandSent.cardData.positionOnBoard);
            if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
            {
                instantiatedObject.AddComponent<ChangeTransparency>();
            }
            ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
            instantiatedObjectsChangeTransparency.ChangeTransparent(100);
        }
        instantiatedFarmer.GetComponent<Farmer>().cardData = cardInHandSent.cardData;
        instantiatedFarmer.GetComponent<Farmer>().cardData.isInHand = false;
        instantiatedFarmer.GetComponent<Farmer>().SetToPlayerOwningFarmer(this);

        farmersOwned.Add(instantiatedFarmer.GetComponent<Farmer>());
        RemoveCardFromHand(cardInHandSent);
    }

    protected virtual void LocalPlaceCastle(Vector3Int positionSent)
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in BaseMapTileState.singleton.baseTiles)
        {
            bt.Value.SetAllNeighborTiles();
        }

        placedCellPosition = positionSent;
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent) == null){

            Debug.LogError("tile at " + positionSent + " is null");
            return;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            return;
        }
        //Vector3 positionToSpawn = baseMap.GetCellCenterWorld(placedCellPosition);
        SetOwningTile(placedCellPosition);


        foreach (BaseTile neighbor in BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).neighborTiles)
        {
            foreach (BaseTile neighborOfNeighbor in neighbor.neighborTiles)
            {
                foreach (BaseTile neighborOfNeighbor2 in neighborOfNeighbor.neighborTiles)
                {
                    SetOwningTile(neighborOfNeighbor2.tilePosition);
                }
            }
        }
    }


    protected void AddStructureToTile(Structure structure, Vector3Int positionSent)
    {
        BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent).structureOnTile = structure;
        structure.tileCurrentlyOn = BaseMapTileState.singleton.GetBaseTileAtCellPosition(positionSent);
        structure.playerOwningStructure = this;
        structuresOwned.Add(structure);
    }

    public virtual void AddTileToHarvestedTilesList(BaseTile baseTileSent)
    {
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Green)
        {
            resources.greenManaCap++;
            resources.greenMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Black)
        {
            resources.blackManaCap++;
            resources.blackMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.White)
        {
            resources.whiteManaCap++;
            resources.whiteMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Red)
        {
            resources.redManaCap++;
            resources.redMana++;
        }
        if (!harvestedTiles.Contains(baseTileSent))
        {
            harvestedTiles.Add(baseTileSent);
            baseTileSent.SetBeingHarvested();
        }
        baseTileSent.ShowHarvestIcon();

        resourcesChanged.Invoke(resources);
    }

    public virtual void RemoveTileFromHarvestedTilesList(BaseTile baseTileSent)
    {
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Green)
        {
            resources.greenManaCap--;
            resources.greenMana--;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Black)
        {
            resources.blackManaCap--;
            resources.blackMana--;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.White)
        {
            resources.whiteManaCap--;
            resources.whiteMana--;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Red)
        {
            resources.redManaCap--;
            resources.redMana--;
        }
        harvestedTiles.Remove(baseTileSent);
        baseTileSent.SetToNotBeingHarvested();

        resourcesChanged.Invoke(resources);
    }




    CardInHand locallySelectedCardInHandToTurnOff;
    bool CheckForRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHitCardInHand, Mathf.Infinity))
        {
            if (locallySelectedCard == null)
            {
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                if (raycastHitCardInHand.transform.GetComponent<CardInHand>() != null && state != State.CreatureInHandSelected)
                {
                    //todo
                    locallySelectedCardInHandToTurnOff = raycastHitCardInHand.transform.GetComponent<CardInHand>();
                    locallySelectedCardInHandToTurnOff.TurnOffVisualCard();
                    locallySelectedCard = Instantiate(locallySelectedCardInHandToTurnOff.gameObject, canvasMain.transform).GetComponent<CardInHand>();
                    locallySelectedCard.transform.position = locallySelectedCardInHandToTurnOff.transform.position;
                    locallySelectedCard.transform.localEulerAngles = Vector3.zero;
                    locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);

                    if (raycastHitCardInHand.transform.GetComponent<CardInHand>().cardData.cardType == SpellSiegeData.CardType.Creature)
                    {
                        state = State.CreatureInHandSelected;
                    }
                    if (raycastHitCardInHand.transform.GetComponent<CardInHand>().cardData.cardType == SpellSiegeData.CardType.Farmer)
                    {
                        state = State.FarmerInHandSelected;
                    }
                    if (raycastHitCardInHand.transform.GetComponent<CardInHand>().cardData.cardType == SpellSiegeData.CardType.Spell)
                    {
                        state = State.SpellInHandSelected;
                    }
                    if (raycastHitCardInHand.transform.GetComponent<CardInHand>().playerOwningCard == null)
                    {
                        return true;
                    }
                    else
                    {
                        ShowViablePlacableTiles(locallySelectedCard);
                        return true;
                    }
                }
            }
        }
        if (Physics.Raycast(ray, out RaycastHit raycastHitCreatureOnBoard, Mathf.Infinity, creatureMask))
        {
            if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>() != null)
            {
                Debug.Log("Hit creature on board " + raycastHitCreatureOnBoard.transform.GetComponent<Creature>());
                if (raycastHitCreatureOnBoard.transform.GetComponent<Creature>().playerOwningCreature == this && state != State.SpellInHandSelected)
                {
                    SetVisualsToNothingSelectedLocally();
                    AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                    return true;
                }
                if (state == State.SpellInHandSelected || state == State.StructureInHandSeleced)
                {
                    SetVisualsToNothingSelectedLocally();
                    if (locallySelectedCard.GameObjectToInstantiate != null)
                    {
                        if (locallySelectedCard.GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
                        {
                            AddIndexOfCreatureOnBoard(raycastHitCreatureOnBoard.transform.GetComponent<Creature>().creatureID);
                            return true;
                        }
                    }
                }
            }
        }


        return false;
    }
    private void ShowViablePlacableTilesDoNotRequireCardInHand()
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
        {
            if (kvp.Value.CreatureOnTile() == null && !harvestedTiles.Contains(kvp.Value))
            {
                kvp.Value.HighlightTile();
                highlightedTiles.Add(kvp.Value);
            }
        }
    }
    List<BaseTile> highlightedTiles = new List<BaseTile>();
    private void ShowViablePlacableTiles(CardInHand locallySelectedCardInHandToTurnOff)
    {
        Debug.Log("Showing viable placable tiles");
        if (locallySelectedCardInHandToTurnOff.cardData.cardType == SpellSiegeData.CardType.Creature)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                kvp.Value.HighlightTile();
                highlightedTiles.Add(kvp.Value);
            }
        }
        if (locallySelectedCardInHandToTurnOff.cardData.cardType == SpellSiegeData.CardType.Structure)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                if (!harvestedTiles.Contains(kvp.Value))
                {
                    kvp.Value.HighlightTile();
                    highlightedTiles.Add(kvp.Value);
                }
            }
        }
        if (locallySelectedCardInHandToTurnOff.cardData.cardType == SpellSiegeData.CardType.Spell)
        {
            if (locallySelectedCardInHandToTurnOff.GameObjectToInstantiate.GetComponent<Spell>() != null)
            {
                if (locallySelectedCardInHandToTurnOff.GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
                {
                    foreach (BaseTile bt in harvestedTiles)
                    {
                        bt.HighlightTile();
                        highlightedTiles.Add(bt);
                    }
                }
            }
        }
    }
    private void TargetACreatureLocal(int selectedCreatureID, int creatureToTargetID, Vector3 actualPosition)
    {
        GameManager.singleton.allCreaturesOnField[selectedCreatureID].SetTargetToFollow(GameManager.singleton.allCreaturesOnField[creatureToTargetID], actualPosition);

    }

    public List<Creature> creaturesOwned = new List<Creature>();


    void HandleCreatureInHandSelected(Vector3Int cellSent)
    {
        Creature creatureToETB = CastCreatureOnTile(locallySelectedCard, cellSent);

        creatureToETB.OnETB();
        cardsInHand.Remove(locallySelectedCard);
        SetStateToNothingSelected();
    }
    public virtual bool CheckToSeeIfCanSpawnCreature(Vector3Int cellSent)
    {
        if (harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == SpellSiegeData.traversableType.SwimmingAndFlying && locallySelectedCard.GameObjectToInstantiate.GetComponent<Creature>().thisTraversableType == SpellSiegeData.travType.Walking)
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).playerOwningTile == this)
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).structureOnTile == null && BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).CreatureOnTile() == null)
            {
                SetVisualsToNothingSelectedLocally();
                return true;
            }
        }
        SetVisualsToNothingSelectedLocally();
        SetStateToNothingSelected();
        return false;
    }

    private void SpawnVisualCreatureOnTile(Vector3Int positionSent)
    {
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(positionSent);
        cardsInHand.Remove(locallySelectedCard);
        Destroy(locallySelectedCard.gameObject);

        cardsInHand.Remove(locallySelectedCardInHandToTurnOff);
        locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);
        Destroy(locallySelectedCardInHandToTurnOff.gameObject);
    }

    public Creature CastCreatureOnTile(CardInHand cardSelectedSent, Vector3Int cellSent)
    {
        Debug.Log("Spawning creature on tile ");
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
        GameObject instantiatedCreature = Instantiate(cardSelectedSent.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
        if (environmentMap.GetInstantiatedObject(cellSent))
        {
            GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
            if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
            {
                instantiatedObject.AddComponent<ChangeTransparency>();
            }
            ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
            instantiatedObjectsChangeTransparency.ChangeTransparent(100);
        }
        instantiatedCreature.GetComponent<Creature>().cardData.positionOnBoard = cellSent;
        instantiatedCreature.GetComponent<Creature>().cardData.isInHand = false;
        instantiatedCreature.GetComponent<Creature>().cardData = cardSelectedSent.cardData;
        instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>());
        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent.cardData);

        #region assignRebirthAbility
        if (instantiatedCreature.GetComponent<Cat>() != null)
        {
            instantiatedCreature.GetComponent<Cat>().numberOfTimesThisCanDie++;
        }
        if (instantiatedCreature.GetComponent<Bat>() != null)
        {
            instantiatedCreature.GetComponent<Bat>().numberOfTimesThisCanDie++;
        }
        #endregion
        cardSelectedSent.transform.parent = null;


        RemoveCardFromHand(cardSelectedSent);


        if (!instantiatedCreature.GetComponent<Creature>().garrison)
        {
            //instantiatedCreature.GetComponent<Creature>().SetStructureToFollow(opponent.instantiatedCaste, instantiatedCreature.GetComponent<Creature>().actualPosition);
        }

        return instantiatedCreature.GetComponent<Creature>();
    }


    public void SpawnCreatureOnTileWithoutCard(GameObject animalToSpawn, Vector3Int cellSent, CardInHand cardSelectedSent)
    {
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
        GameObject instantiatedCreature = Instantiate(animalToSpawn.gameObject, positionToSpawn, Quaternion.identity);
        if (environmentMap.GetInstantiatedObject(cellSent))
        {
            GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
            if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
            {
                instantiatedObject.AddComponent<ChangeTransparency>();
            }
            ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
            instantiatedObjectsChangeTransparency.ChangeTransparent(100);
        }
        instantiatedCreature.GetComponent<Creature>().cardData = cardSelectedSent.cardData;
        instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>());
        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent.cardData);
        instantiatedCreature.GetComponent<Creature>().OnETB();

        instantiatedCreature.GetComponent<Creature>().SetStructureToFollow(opponent.instantiatedCaste, instantiatedCreature.GetComponent<Creature>().actualPosition);
    }

    private void HandleSpellInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>() != null)
        {
            if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range != 0)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
                if (environmentMap.GetInstantiatedObject(cellSent))
                {
                    GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
                    if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
                    {
                        instantiatedObject.AddComponent<ChangeTransparency>();
                    }
                    ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
                    instantiatedObjectsChangeTransparency.ChangeTransparent(100);
                }

                RemoveCardFromHand(locallySelectedCard);
                SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(locallySelectedCard.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && !locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                RemoveCardFromHand(locallySelectedCard);
                SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(locallySelectedCard.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                if (harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
                {
                    Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                    RemoveCardFromHand(locallySelectedCard);
                    SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());
                    GameObject instantiatedSpell = Instantiate(locallySelectedCard.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                    instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                    OnSpellCast();
                    SetVisualsToNothingSelectedLocally();
                    SetStateToNothingSelected();
                    return;
                }
            }
        }
    }
    private void HandleStructureInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (locallySelectedCard != null)
        {
            if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)) && tilesOwned.ContainsValue(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);
                if (environmentMap.GetInstantiatedObject(cellSent))
                {
                    GameObject instantiatedObject = environmentMap.GetInstantiatedObject(cellSent);
                    if (instantiatedObject.GetComponent<ChangeTransparency>() == null)
                    {
                        instantiatedObject.AddComponent<ChangeTransparency>();
                    }
                    ChangeTransparency instantiatedObjectsChangeTransparency = instantiatedObject.GetComponent<ChangeTransparency>();
                    instantiatedObjectsChangeTransparency.ChangeTransparent(100);

                    Destroy(instantiatedObject);
                }

                SetOwningTile(cellSent);

                SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());

                foreach (BaseTile bt in BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).neighborTiles)
                {
                    SetOwningTile(bt.tilePosition);
                }
                AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent));
                RemoveCardFromHand(locallySelectedCard);
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
        }

    }



    protected void SpendManaToCast(CardInHand cardSelectedSent)
    {
        /*resources.redMana -= cardSelectedSent.redManaCost;
        resources.whiteMana -= cardSelectedSent.whiteManaCost;
        resources.blackMana -= cardSelectedSent.blackManaCost;
        resources.greenMana -= cardSelectedSent.greenManaCost;
        SpendGenericMana(cardSelectedSent.genericManaCost);*/
        resourcesChanged.Invoke(resources);
    }

    public void SpendGenericMana(int genericManaCost)
    {
        int totalManaSpent = 0;
        for (int i = 0; i < genericManaCost; i++)
        {
            if (resources.blackMana > 0)
            {
                resources.blackMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.redMana > 0)
            {
                resources.redMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.whiteMana > 0)
            {
                resources.whiteMana--;
                totalManaSpent++;
                continue;
            }
            if (resources.greenMana > 0)
            {
                resources.greenMana--;
                totalManaSpent++;
                continue;
            }
        }
        resourcesChanged.Invoke(resources);
    }

    protected void SetOwningTile(Vector3Int cellPosition)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition).traverseType == SpellSiegeData.traversableType.Untraversable)
        {
            return;
        }
        if (!tilesOwned.ContainsKey(cellPosition))
        {
            tilesOwned.Add(cellPosition, BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition));
            BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition).SetOwnedByPlayer(this);
            BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellPosition).SetNotBeingHarvested();
        }
    }

    public CardInHand InstantiateCardInHand(CardData cardSent)
    {
        GameObject instantiatedCardInHand = Instantiate(GameManager.singleton.GetCardAssociatedWithType(cardSent.cardAssignedToObject), cardParent).gameObject;
        CardInHand instantiatedCardInHandBehaviour = instantiatedCardInHand.GetComponent<CardInHand>();
        instantiatedCardInHandBehaviour.playerOwningCard = this;
        instantiatedCardInHandBehaviour.cardData = cardSent;
        cardsInHand.Add(instantiatedCardInHandBehaviour);
        if (locallySelectedCard != null)
        {
            SetStateToNothingSelected();
        }

        return instantiatedCardInHandBehaviour;
    }


    private void DiscardCard()
    {
        cardsInHand[0].DiscardAnimation();
        RemoveCardFromHand(cardsInHand[0]);
    }

    public void DrawCardWithIndex(int indexOfCard)
    {
        if (cardsInDeck.Count <= 0)
        {
            return;
        }
        if (cardsInHand.Count >= maxHandSize)
        {
            return;
        }
        CardInHand cardAddingToHand = cardsInDeck[indexOfCard]; //todo this might cause problems when dealing with shuffling cards back into the deck
        cardsInDeck.RemoveAt(indexOfCard);

        GameObject instantiatedCardInHand = Instantiate(cardAddingToHand.gameObject, cardParent);
        CardInHand instantiatedCardInHandBehaviour = instantiatedCardInHand.GetComponent<CardInHand>();

        cardsInHand.Add(instantiatedCardInHandBehaviour);
        instantiatedCardInHandBehaviour.playerOwningCard = this;
        instantiatedCardInHandBehaviour.CheckToSeeIfPurchasable(resources);
    }

    void RemoveCardFromHand(CardInHand cardToRemove)
    {
        cardsInHand.Remove(cardToRemove);
        if (cardToRemove != null)
        {
            cardToRemove.DiscardAnimation();
            cardToRemove.transform.parent = null;
        }
    }


    int indexOfCardInHandSelected;
    public void SetToCreatureOnFieldSelected(Creature creatureSelectedSent)
    {
        if (state == State.SpellInHandSelected)
        {
            if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
            {
                if (!locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly)
                {
                    CastSpellOnTargetedCreature(creatureSelectedSent);
                    return;
                }
                if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly)
                {
                    if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature == null)
                    {
                        return;
                    }
                    if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature == this)
                    {
                        CastSpellOnTargetedCreature(creatureSelectedSent);
                        return;
                    }
                    if (locallySelectedCard.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature != this)
                    {
                        return;
                    }
                }
            }
            return;
        }
    }

    private void CastSpellOnTargetedCreature(Creature creatureSelectedSent)
    {
        Debug.Log(locallySelectedCard + " card selected send to casting spell on target creature");
        SpendManaToCast(locallySelectedCard.GetComponent<CardInHand>());
        GameObject instantiatedSpell = Instantiate(locallySelectedCard.GameObjectToInstantiate.gameObject, creatureSelectedSent.tileCurrentlyOn.tilePosition, Quaternion.identity);
        instantiatedSpell.GetComponent<TargetedSpell>().InjectDependencies(creatureSelectedSent, this);
        OnSpellCast();
        cardsInHand.Remove(locallySelectedCardInHandToTurnOff);
        Destroy(locallySelectedCardInHandToTurnOff.gameObject);
        RemoveCardFromHand(locallySelectedCard);
        SetStateToNothingSelected();
    }

    public void SetStateToNothingSelected()
    {
        if (selectedOnBoardFarmer != null)
        {
            foreach (Collider col in selectedOnBoardFarmer.GetComponents<Collider>())
            {
                col.enabled = true;
            }
            Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(selectedOnBoardFarmer.cardData.positionOnBoard);
            selectedOnBoardFarmer.transform.position = positionToSpawn;
        }
        if (selectedOnBoardCreature != null)
        {
            foreach (Collider col in selectedOnBoardCreature.GetComponents<Collider>())
            {
                col.enabled = true;
            }
            Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(selectedOnBoardCreature.cardData.positionOnBoard);
            selectedOnBoardCreature.transform.position = positionToSpawn;
        }
        selectedOnBoardFarmer = null;
        selectedOnBoardCreature = null;
        if (locallySelectedCard != null)
        {
            locallySelectedCard = null;
        }
        locallySelectedCard = null;

        state = State.NothingSelected;
    }
    protected void SetStateToWaiting()
    {
        locallySelectedCard = null;
        state = State.Waiting;
    }

    public virtual void ClearMana()
    {
        resources.greenMana = 0;
        resources.redMana = 0;
        resources.blackMana = 0;
        resources.whiteMana = 0;

        resourcesChanged.Invoke(resources);
    }
    public virtual void AddToMana()
    {
        for (int i = 0; i < harvestedTiles.Count; i++)
        {
            for (int j = 0; j < harvestedTiles[i].currentAmountOfManaProducing; j++)
            {
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Black)
                {
                    resources.blackMana++;
                    if (resources.blackMana > resources.blackManaCap)
                    {
                        resources.blackMana = resources.blackManaCap;
                    }
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Red)
                {
                    resources.redMana++;
                    if (resources.redMana > resources.redManaCap)
                    {
                        resources.redMana = resources.redManaCap;
                    }
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.White)
                {
                    resources.whiteMana++;
                    if (resources.whiteMana > resources.whiteManaCap)
                    {
                        resources.whiteMana = resources.whiteManaCap;
                    }
                }
                if (harvestedTiles[i].manaType == SpellSiegeData.ManaType.Green)
                {
                    resources.greenMana++;
                    if (resources.greenMana > resources.greenManaCap)
                    {
                        resources.greenMana = resources.greenManaCap;
                    }
                }
            }
        }

        resourcesChanged.Invoke(resources);
    }
    public void AddSpecificManaToPool(SpellSiegeData.ManaType manaTypeSent)
    {
        if (manaTypeSent == SpellSiegeData.ManaType.Black)
        {
            resources.blackMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.Red)
        {
            resources.redMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.Green)
        {
            resources.greenMana++;
        }
        if (manaTypeSent == SpellSiegeData.ManaType.White)
        {
            resources.whiteMana++;
        }

        resourcesChanged.Invoke(resources);
    }
    int totalMana;
    public virtual void UpdateHudForResourcesChanged(PlayerResources resources)
    {
        hudElements.UpdateHudElements(resources);
        CheckAffordableCards();
        totalMana = resources.blackMana + resources.whiteMana + resources.greenMana + resources.redMana;
    }


    public void CheckAffordableCards()
    {

        foreach (CardInHand cardInHand in cardsInHand)
        {
            cardInHand.CheckToSeeIfPurchasable(resources);
        }
    }




    //overridables
    public virtual void OnSpellCast()
    {

        spellCounter++;
        foreach (Creature creature in creaturesOwned)
        {
            creature.OnOwnerCastSpell();
        }

    }

    internal CardInHand GetRandomCreatureInHand()
    {
        List<int> numbersChosen = new List<int>();
        CardInHand creatureSelectedInHand = new CardInHand();
        while (creatureSelectedInHand == null)
        {
            int randomNumber = UnityEngine.Random.Range(0, cardsInHand.Count - 1);
            if (numbersChosen.Contains(randomNumber))
            {
                break;
            }
            numbersChosen.Add(randomNumber);
            if (cardsInHand[randomNumber].cardData.cardType == SpellSiegeData.CardType.Creature)
            {
                creatureSelectedInHand = cardsInHand[randomNumber];
            }
        }
        return creatureSelectedInHand;
    }


    public CardInHand cardToPurchase = null;
    internal void SetToPurchaseACard(CardInHand locallySelectedCardToPurchase)
    {
        cardToPurchase = locallySelectedCardToPurchase;
    }

    internal void SetToDontPurchaseCard()
    {
        cardToPurchase = null;
    }

    public void PurchaseCard()
    {
        if (goldAmount >= cardToPurchase.cardData.purchaseCost)
        {
            goldAmount -= cardToPurchase.cardData.purchaseCost;
        }
        else
        {
            SetToDontPurchaseCard();
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
            cardToPurchase = null;
            return;
        }

        if (cardToPurchase != null)
        {
            InstantiateCardInHand(cardToPurchase.cardData);
            if (cardToPurchase.gameObject != null)
            {
                Destroy(cardToPurchase.gameObject);
            }
            if (locallySelectedCardInHandToTurnOff.gameObject != null)
            {
                Destroy(locallySelectedCardInHandToTurnOff.gameObject);
            }
            SetStateToNothingSelected();
            SetVisualsToNothingSelectedLocally();
        }
        cardToPurchase = null;
    }


    public PlayerData playerData;
    public string currentGUIDForPlayer;
    public void SubmitPlayerData()
    {
        // Ensure playerData is not null
        if (playerData == null)
        {
            Debug.LogError("playerData is null. Initializing playerData object.");
            playerData = new PlayerData();
        }

        // Ensure playerRoundConfigurations is not null
        if (playerData.playerRoundConfigurations == null)
        {
            Debug.LogError("playerRoundConfigurations is null. Initializing playerRoundConfigurations list.");
            playerData.playerRoundConfigurations = new List<RoundConfiguration>();
        }
        SaveAllCardsInHandAndOnFieldIntoAllCardsData();

        RoundConfiguration roundConfiguration = new RoundConfiguration
        {
            allOwnedCards = allOwnedCardsInScene,
            round = playerData.currentRound
        };

        RoundConfiguration existingConfig = playerData.playerRoundConfigurations
            .Find(config => config != null && config.round == roundConfiguration.round);

        if (existingConfig != null)
        {
            int index = playerData.playerRoundConfigurations.IndexOf(existingConfig);
            playerData.playerRoundConfigurations[index] = roundConfiguration;
        }
        else
        {
            playerData.playerRoundConfigurations.Add(roundConfiguration);
        }

        SavePlayerConfigLocally(playerData, currentGUIDForPlayer);
    }

    public void SaveAllCardsInHandAndOnFieldIntoAllCardsData()
    {
        allOwnedCardsInScene = new List<CardData>();
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            allOwnedCardsInScene.Add(cardsInHand[i].cardData);
        }
        for (int i = 0; i < creaturesOwned.Count; i++)
        {
            allOwnedCardsInScene.Add(creaturesOwned[i].cardData);
        }
        for (int i = 0; i < farmersOwned.Count; i++)
        {
            allOwnedCardsInScene.Add(farmersOwned[i].cardData);
        }
    }

    protected virtual void SavePlayerConfigLocally(PlayerData playerData, string playerGuid)
    {
        string directoryPath = $"{Application.persistentDataPath}/playerData/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = $"{directoryPath}{playerGuid}.txt";
        string json = JsonUtility.ToJson(playerData);

        File.WriteAllText(filePath, json);
    }

    public PlayerData GrabPlayerDataByGuid(string playerGuid)
    {
        string filePath = $"{Application.persistentDataPath}/playerData/{playerGuid}.txt";

        // Check if the file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON string back to a PlayerData object
            PlayerData playerDataFromJSON = JsonUtility.FromJson<PlayerData>(json);
            return playerDataFromJSON;
        }
        else
        {
            Debug.Log($"File not found: {filePath}");
        }

        return null;
    }

    public string GenerateNewPlayerGuid()
    {
        return Guid.NewGuid().ToString();
    }

    public List<PlayerData> LoadAllPlayerData()
    {
        List<PlayerData> allPlayersData = new List<PlayerData>();
        string directoryPath = $"{Application.persistentDataPath}/playerData/";

        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath, "*.txt");

            foreach (string file in files)
            {
                string json = File.ReadAllText(file);
                PlayerData playerDataFromJSON = JsonUtility.FromJson<PlayerData>(json);
                allPlayersData.Add(playerDataFromJSON);
            }
        }
        else
        {
            Debug.Log($"Directory not found: {directoryPath}");
        }

        return allPlayersData;
    }

    public void InstantiateCardsBasedOnPlayerData(PlayerData playerDataSent)
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
                        CastCreatureOnTile(cardToImmediatelyPlay, cardData.positionOnBoard);
                    }
                    if (cardData.cardType == SpellSiegeData.CardType.Farmer)
                    {
                        CastFarmerOnTile(cardToImmediatelyPlay);
                        PurchaseHarvestTile(cardData.positionOnBoard);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No cards found in roundConfiguration.allOwnedCards.");
        }
    }
    private RoundConfiguration GrabBuildByCurrentRound(PlayerData playerDataSent, int currentRound)
    {
        if (playerDataSent.playerRoundConfigurations != null)
        {
            RoundConfiguration existingConfig = playerDataSent.playerRoundConfigurations
                .Find(config => config != null && config.round == currentRound);

            if (existingConfig != null)
            {
                return existingConfig;
            }
        }

        return new RoundConfiguration();
    }


}




public struct PlayerResources
{
    public int redMana;
    public int whiteMana;
    public int blackMana;
    public int greenMana;


    public int redManaCap;
    public int whiteManaCap;
    public int blackManaCap;
    public int greenManaCap;

}