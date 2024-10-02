using System;
using System.Collections;
using System.Collections.Generic;
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
        Waiting
    }

    //Could use a state machine if creature is selected change state to creature selected 
    //if card in hand is selected change state to placing card
    //if neither are selected change state to selecting
    //if environment is selected change state to environment selected
    //if environment card is selected change state to environment card selected

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

    protected int turnThreshold = 1100; //todo make this 1400
    protected int maxHandSize = 9;
    [SerializeField] protected List<CardInHand> dragonDeck = new List<CardInHand>();
    [SerializeField] protected List<CardInHand> demonDeck = new List<CardInHand>();
    public List<CardInHand> cardsInDeck = new List<CardInHand>();
    public List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardInHand cardSelected;
    public CardInHand locallySelectedCard;
    public List<Vector3> allVertextPointsInTilesOwned = new List<Vector3>();

    protected Transform instantiatedPlayerUI;
    protected Transform cardParent;

    protected Canvas canvasMain;


    protected PlayerResources resources;

    public delegate void ResourcesChanged(PlayerResources resources);
    public ResourcesChanged resourcesChanged;

    [SerializeField] protected Transform playerHud;
    protected HudElements hudElements;
    protected int numOfPurchasableHarvestTiles = 1;

    public List<BaseTile> harvestedTiles = new List<BaseTile>();


    public int spellCounter = 0;


    public int numberOfLandsYouCanPlayThisTurn = 1;
    public int numberOfLandsPlayedThisTurn = 0;

    [SerializeField] protected Color[] colorsToPickFrom;


    public List<Structure> structuresOwned = new List<Structure>();

    public enum ActionTaken
    {
        LeftClickBaseMap,
        SelectedCardInHand,
        RightClick,
        TilePurchased
    }


    public Controller opponent;

    private void Awake()
    {
        GameManager.singleton.playerList.Add(this);
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        GrabAllObjectsFromGameManager();
        mousePositionScript = GetComponent<MousePositionScript>();
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
    private void StartGame()
    {
        Debug.Log("Starting the game as there are now 2 players connected.");

        SpawnCastleForPlayer();
    }

    [SerializeField] GameObject castlePrefab;

    private void SpawnCastleForPlayer()
    {
    }
    public void StartGameCoroutine()
    {
        //cardsInDeck = new List<CardInHand>(demonDeck);
        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        SpawnHUDAndHideOnAllNonOwners();
        resources = new PlayerResources();
        resourcesChanged += UpdateHudForResourcesChanged;

        col.a = 1;
        transparentCol = col;
        transparentCol.a = .5f;
        locallySelectedCard = null;



    }

    private readonly Vector3 serverCastlePosition = new Vector3(-9, 0, 0);
    private readonly Vector3 clientCastlePosition = new Vector3(10, 0, 0);




    protected void GrabAllObjectsFromGameManager()
    {
        canvasMain = GameManager.singleton.canvasMain.GetComponent<Canvas>();
        highlightTile = GameManager.singleton.highlightTile;
        highlightMap = GameManager.singleton.highlightMap;// set these = to gamemanage.singleton.highlightmap TODO
        baseMap = GameManager.singleton.baseMap;
        environmentMap = GameManager.singleton.enviornmentMap;
        waterMap = GameManager.singleton.waterTileMap;
        grid = GameManager.singleton.grid;
        castle = GameManager.singleton.castleTransform;
        //GameManager.singleton.playerList.Add(this);
    }
    protected void SpawnHUDAndHideOnAllNonOwners()
    {
        instantiatedPlayerUI = Instantiate(playerHud, canvasMain.transform);
        cardParent.gameObject.GetComponent<Image>().color = transparentCol;
        hudElements = instantiatedPlayerUI.GetComponent<HudElements>();
        hudElements.UpdateHudVisuals(this, turnThreshold);
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
                HandleMana();
                HandleHarvestTiles();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.CreatureInHandSelected:
                HandleMana();
                HandleHarvestTiles();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.SpellInHandSelected:
                HandleMana();
                HandleHarvestTiles();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
            case State.StructureInHandSeleced:
                HandleMana();
                HandleHarvestTiles();
                HandleDrawCards();
                TriggerAllCreatureAbilities();
                break;
        }
    }

    public void HandleHarvestTiles()
    {
    }


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
        if (Input.GetMouseButtonDown(1))
        {
            SetVisualsToNothingSelectedLocally();
            SetStateToNothingSelected();
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (locallySelectedCard != null)
            {
                if (cellPositionSentToClients != null)
                {
                    if (cellPositionSentToClients != grid.WorldToCell(mousePosition))
                    {
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePositionWorldPoint;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, mousePositionScript.baseTileMap))
            {
                mousePositionWorldPoint = raycastHit.point;
                cellPositionSentToClients = grid.WorldToCell(mousePositionWorldPoint);
            }
            if (!CheckForRaycast())
            {
                LeftClickQueue(cellPositionSentToClients);
            }

            if (state == State.NothingSelected && cardSelected == null)
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
    private bool CheckToSeeIfClickedHarvestTileCanBePurchased(Vector3Int tilePositionSent)
    {
        if (!harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent)))
        {
            if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent))
            {
                if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(tilePositionSent).playerOwningTile == this && numberOfLandsYouCanPlayThisTurn > numberOfLandsPlayedThisTurn)
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
        numberOfLandsPlayedThisTurn++;
        AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(vector3Int));

        if (numberOfLandsPlayedThisTurn >= numberOfLandsYouCanPlayThisTurn)
        {
            HideHarvestedTiles();
        }
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
        Debug.Log("showing harvest tiles " + numberOfLandsYouCanPlayThisTurn + " " + numberOfLandsPlayedThisTurn);
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in tilesOwned)
        {
            if (harvestedTiles.Contains(bt.Value) || numberOfLandsYouCanPlayThisTurn > numberOfLandsPlayedThisTurn)
            {
                bt.Value.ShowHarvestIcon();
            }
            if (!harvestedTiles.Contains(bt.Value) && numberOfLandsYouCanPlayThisTurn > numberOfLandsPlayedThisTurn)
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
        Debug.Log("Local creature " + locallySelectedCard);
        //visual section for spawning creatures
        if (locallySelectedCard != null && locallySelectedCard.cardType == SpellSiegeData.CardType.Creature)
        {
            if (CheckToSeeIfCanSpawnCreature(positionSent))
            {
                SpawnVisualCreatureOnTile(positionSent);
                LocalLeftClick(positionSent);
            }
            return;
        }
        if (locallySelectedCard != null && locallySelectedCard.cardType == SpellSiegeData.CardType.Spell)
        {
            LocalLeftClick(positionSent);
            return;
        }
        if (state == State.PlacingCastle)
        {
            LocalLeftClick(positionSent);
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
                HandleTurn();
                break;
            case State.CreatureInHandSelected:
                HandleTurn();
                break;
            case State.SpellInHandSelected:
                HandleTurn();
                break;
            case State.StructureInHandSeleced:
                HandleTurn();
                break;
        }
    }


    private void HandleTurn()
    {
        if (hudElements != null)
        {
            hudElements.UpdateDrawSlider(GameManager.singleton.turnTimer);
        }
        if (GameManager.singleton.turnTimer > turnThreshold)
        {
            foreach (Controller controller in GameManager.singleton.playerList)
            {
            }
            GameManager.singleton.turnTimer = 0;
        }

    }

    protected void TriggerAllCreatureAbilities()
    {
        foreach (KeyValuePair<int, Creature> kp in creaturesOwned)
        {
            kp.Value.OnTurn();
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

    protected void HandleDrawCards()
    {
        DrawCard();
    }


    protected void HandleMana()
    {
        numberOfLandsYouCanPlayThisTurn = 1;
        numberOfLandsPlayedThisTurn = 0;
        //ClearMana();
        AddToMana();
    }



    void AddIndexOfCardInHandToTickQueueLocal(int index)
    {
    }
    void AddIndexOfCreatureOnBoard(int index)
    {
    }



    void LocalLeftClick(Vector3Int positionSent)
    {
        switch (state)
        {
            case State.PlacingCastle:
                break;
            case State.NothingSelected:
                break;
            case State.CreatureInHandSelected:
                Debug.Log("Local left click ddqwwd");
                HandleCreatureInHandSelected(positionSent);
                break;
            case State.SpellInHandSelected:
                HandleSpellInHandSelected(positionSent);
                break;
            case State.StructureInHandSeleced:
                HandleStructureInHandSelected(positionSent);
                break;
        }
    }

    void LocalPlaceCastle(Vector3Int positionSent)
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> bt in BaseMapTileState.singleton.baseTiles)
        {
            bt.Value.SetAllNeighborTiles();
        }

        placedCellPosition = positionSent;
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
        //instantiatedCaste = Instantiate(castle, positionSent, Quaternion.identity);
        //instantiatedCaste.GetComponent<MeshRenderer>().material.color = col;
        //AddStructureToTile(instantiatedCaste.GetComponent<Structure>(), positionSent);
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
            resources.greenManaCap = 9;
            resources.greenMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Black)
        {
            resources.blackManaCap = 9;
            resources.blackMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.White)
        {
            resources.whiteManaCap = 9;
            resources.whiteMana++;
        }
        if (baseTileSent.manaType == SpellSiegeData.ManaType.Red)
        {
            resources.redManaCap = 9;
            resources.redMana++;
        }
        if (!harvestedTiles.Contains(baseTileSent))
        {
            harvestedTiles.Add(baseTileSent);
            baseTileSent.SetBeingHarvested();
        }
        baseTileSent.ShowHarvestIcon();
        //baseTileSent.HighlightTile();
        //numberOfLandsPlayedThisTurn++;

        resourcesChanged.Invoke(resources);
    }




    CardInHand locallySelectedCardInHandToTurnOff;
    bool CheckForRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHitCardInHand, Mathf.Infinity))
        {
            if (raycastHitCardInHand.transform.GetComponent<CardInHand>() != null && state != State.CreatureInHandSelected)
            {
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                if (raycastHitCardInHand.transform.GetComponent<CardInHand>().isPurchasable)
                {
                    SetVisualsToNothingSelectedLocally();
                    //todo
                    locallySelectedCardInHandToTurnOff = raycastHitCardInHand.transform.GetComponent<CardInHand>();
                    locallySelectedCardInHandToTurnOff.TurnOffVisualCard();
                    locallySelectedCard = Instantiate(locallySelectedCardInHandToTurnOff.gameObject, canvasMain.transform).GetComponent<CardInHand>();
                    locallySelectedCard.transform.position = locallySelectedCardInHandToTurnOff.transform.position;
                    locallySelectedCard.transform.localEulerAngles = Vector3.zero;
                    locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);
                    AddIndexOfCardInHandToTickQueueLocal(locallySelectedCardInHandToTurnOff.indexOfCard);
                    ShowViablePlacableTiles(locallySelectedCardInHandToTurnOff);
                    return true;
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
                    if (cardSelected.GameObjectToInstantiate != null)
                    {
                        if (cardSelected.GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
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

    List<BaseTile> highlightedTiles = new List<BaseTile>();
    private void ShowViablePlacableTiles(CardInHand locallySelectedCardInHandToTurnOff)
    {
        Debug.Log("Showing viable placable tiles");
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Creature)
        {
            foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
            {
                kvp.Value.HighlightTile();
                highlightedTiles.Add(kvp.Value);
            }
        }
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Structure)
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
        /*
    if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Structure)
    {
        foreach (KeyValuePair<Vector3Int, BaseTile> kvp in tilesOwned)
        {
            foreach (BaseTile neighbor in kvp.Value.neighborTiles)
            {
                if (!tilesOwned.ContainsValue(neighbor))
                {
                    neighbor.HighlightTile();
                    highlightedTiles.Add(neighbor);
                }
            }
        }*/
        if (locallySelectedCardInHandToTurnOff.cardType == SpellSiegeData.CardType.Spell)
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


    protected void LocalSelectCardWithIndex(int indexOfCardSelected)
    {
        CardInHand cardToSelect;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].indexOfCard == indexOfCardSelected)
            {
                cardToSelect = cardsInHand[i];
                cardSelected = cardToSelect;
                if (cardSelected.cardType == SpellSiegeData.CardType.Creature)
                {
                    state = State.CreatureInHandSelected;
                }
                if (cardSelected.cardType == SpellSiegeData.CardType.Spell)
                {
                    state = State.SpellInHandSelected;
                }
                if (cardSelected.cardType == SpellSiegeData.CardType.Structure)
                {
                    state = State.StructureInHandSeleced;
                }
            }
        }

    }

    public Dictionary<int, Creature> creaturesOwned = new Dictionary<int, Creature>();


    void HandleCreatureInHandSelected(Vector3Int cellSent)
    {
        SpendManaToCast(cardSelected);
        CastCreatureOnTile(cardSelected, cellSent);
        SetStateToNothingSelected();
    }
    public virtual bool CheckToSeeIfCanSpawnCreature(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            //show error
            return false;
        }
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).traverseType == SpellSiegeData.traversableType.SwimmingAndFlying && locallySelectedCard.GameObjectToInstantiate.GetComponent<Creature>().thisTraversableType == SpellSiegeData.travType.Walking)
        {
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
        return false;
    }

    [SerializeField] Transform visualSpawnEffect; GameObject localVisualCreture;
    GameObject instantiatedSpawnPArticle;
    private void SpawnVisualCreatureOnTile(Vector3Int positionSent)
    {
        Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(positionSent);

        instantiatedSpawnPArticle = Instantiate(visualSpawnEffect, new Vector3(positionToSpawn.x, positionToSpawn.y + .2f, positionToSpawn.z), Quaternion.identity).gameObject;
        Destroy(locallySelectedCard.gameObject);

        locallySelectedCardInHandToTurnOff.gameObject.SetActive(false);
    }

    public void CastCreatureOnTile(CardInHand cardSelectedSent, Vector3Int cellSent)
    {
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

        instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>().creatureID, instantiatedCreature.GetComponent<Creature>());
        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent);
        instantiatedCreature.GetComponent<Creature>().OnETB();

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
        if (instantiatedSpawnPArticle != null)
        {
            Destroy(instantiatedSpawnPArticle);
        }

        if (!instantiatedCreature.GetComponent<Creature>().garrison)
        {
            instantiatedCreature.GetComponent<Creature>().SetStructureToFollow(opponent.instantiatedCaste, instantiatedCreature.GetComponent<Creature>().actualPosition);
        }
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

        instantiatedCreature.GetComponent<Creature>().SetToPlayerOwningCreature(this);
        creaturesOwned.Add(instantiatedCreature.GetComponent<Creature>().creatureID, instantiatedCreature.GetComponent<Creature>());
        instantiatedCreature.GetComponent<Creature>().SetOriginalCard(cardSelectedSent);
        instantiatedCreature.GetComponent<Creature>().OnETB();

        if (instantiatedSpawnPArticle != null)
        {
            Destroy(instantiatedSpawnPArticle);
        }
        instantiatedCreature.GetComponent<Creature>().SetStructureToFollow(opponent.instantiatedCaste, instantiatedCreature.GetComponent<Creature>().actualPosition);
    }

    private void HandleSpellInHandSelected(Vector3Int cellSent)
    {
        if (BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent) == null)
        {
            return;
        }
        if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>() != null)
        {
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range != 0)
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

                RemoveCardFromHand(cardSelected);
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && !cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                RemoveCardFromHand(cardSelected);
                SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
                instantiatedSpell.GetComponent<Spell>().InjectDependencies(cellSent, this);
                OnSpellCast();
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().range == 0 && cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<Spell>().SpellRequiresToBeCastOnAHarvestedTile)
            {
                if (harvestedTiles.Contains(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent)))
                {
                    Vector3 positionToSpawn = BaseMapTileState.singleton.GetWorldPositionOfCell(cellSent);

                    RemoveCardFromHand(cardSelected);
                    SpendManaToCast(cardSelected.GetComponent<CardInHand>());
                    GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, positionToSpawn, Quaternion.identity);
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
        if (cardSelected != null)
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

                SpendManaToCast(cardSelected.GetComponent<CardInHand>());

                foreach (BaseTile bt in BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent).neighborTiles)
                {
                    SetOwningTile(bt.tilePosition);
                }
                AddTileToHarvestedTilesList(BaseMapTileState.singleton.GetBaseTileAtCellPosition(cellSent));
                RemoveCardFromHand(cardSelected);
                SetVisualsToNothingSelectedLocally();
                SetStateToNothingSelected();
                return;
            }
        }

    }



    protected void SpendManaToCast(CardInHand cardSelectedSent)
    {
        resources.redMana -= cardSelectedSent.redManaCost;
        resources.whiteMana -= cardSelectedSent.whiteManaCost;
        resources.blackMana -= cardSelectedSent.blackManaCost;
        resources.greenMana -= cardSelectedSent.greenManaCost;
        SpendGenericMana(cardSelectedSent.genericManaCost);
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

    public void DrawCard()
    {

        if (cardsInDeck.Count <= 0)
        {
            return;
        }
        if (cardsInHand.Count >= maxHandSize)
        {
            return;
            //DiscardCard();
        }
        CardInHand cardAddingToHand = cardsInDeck[cardsInDeck.Count - 1]; //todo this might cause problems when dealing with shuffling cards back into the deck

        cardAddingToHand.indexOfCard = cardsInDeck.Count - 1;
        cardsInDeck.RemoveAt(cardsInDeck.Count - 1);

        GameObject instantiatedCardInHand = Instantiate(cardAddingToHand.gameObject, cardParent);
        CardInHand instantiatedCardInHandBehaviour = instantiatedCardInHand.GetComponent<CardInHand>();
        instantiatedCardInHandBehaviour.indexOfCard = cardAddingToHand.indexOfCard;

        cardsInHand.Add(instantiatedCardInHandBehaviour);
        instantiatedCardInHandBehaviour.playerOwningCard = this;
        instantiatedCardInHandBehaviour.CheckToSeeIfPurchasable(resources);
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
        cardAddingToHand.indexOfCard = cardsInDeck.Count - 1;
        cardsInDeck.RemoveAt(indexOfCard);

        GameObject instantiatedCardInHand = Instantiate(cardAddingToHand.gameObject, cardParent);
        CardInHand instantiatedCardInHandBehaviour = instantiatedCardInHand.GetComponent<CardInHand>();
        instantiatedCardInHandBehaviour.indexOfCard = cardAddingToHand.indexOfCard;

        cardsInHand.Add(instantiatedCardInHandBehaviour);
        instantiatedCardInHandBehaviour.playerOwningCard = this;
        instantiatedCardInHandBehaviour.CheckToSeeIfPurchasable(resources);
    }

    void RemoveCardFromHand(CardInHand cardToRemove)
    {
        cardsInHand.Remove(cardToRemove);
        if (cardToRemove != null)
        {
            if (cardToRemove.cardType != SpellSiegeData.CardType.Creature)
            {
                cardToRemove.DiscardAnimation();
                cardToRemove.transform.parent = null;
            }
        }
    }


    int indexOfCardInHandSelected;
    public void SetToCreatureOnFieldSelected(Creature creatureSelectedSent)
    {
        if (state == State.SpellInHandSelected)
        {
            if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>() != null)
            {
                if (!cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly)
                {
                    CastSpellOnTargetedCreature(creatureSelectedSent);
                    return;
                }
                if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly)
                {
                    if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature == null)
                    {
                        return;
                    }
                    if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature == this)
                    {
                        CastSpellOnTargetedCreature(creatureSelectedSent);
                        return;
                    }
                    if (cardSelected.GetComponent<CardInHand>().GameObjectToInstantiate.GetComponent<TargetedSpell>().requiresCreatureBeFriendly && creatureSelectedSent.playerOwningCreature != this)
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
        Debug.Log(cardSelected + " card selected send to casting spell on target creature");
        SpendManaToCast(cardSelected.GetComponent<CardInHand>());
        GameObject instantiatedSpell = Instantiate(cardSelected.GameObjectToInstantiate.gameObject, creatureSelectedSent.tileCurrentlyOn.tilePosition, Quaternion.identity);
        instantiatedSpell.GetComponent<TargetedSpell>().InjectDependencies(creatureSelectedSent, this);
        OnSpellCast();
        RemoveCardFromHand(cardSelected);
        SetStateToNothingSelected();
    }

    public void SetStateToNothingSelected()
    {
        if (locallySelectedCard != null)
        {
            locallySelectedCard = null;
            //SetVisualsToNothingSelectedLocally();
        }
        cardSelected = null;

        state = State.NothingSelected;
    }
    protected void SetStateToWaiting()
    {
        cardSelected = null;
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
        totalMana = resources.blackMana  + resources.whiteMana + resources.greenMana + resources.redMana;
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
        foreach (KeyValuePair<int, Creature> creature in creaturesOwned)
        {
            creature.Value.OnOwnerCastSpell();
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
            if (cardsInHand[randomNumber].cardType == SpellSiegeData.CardType.Creature)
            {
                creatureSelectedInHand = cardsInHand[randomNumber];
            }
        }
        return creatureSelectedInHand;
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