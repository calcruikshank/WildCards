using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Opponent : Controller
{
    protected override void Start()
    {
    }

    public void StartFromGameManager()
    {
        GrabAllObjectsFromGameManager();
        mousePositionScript = GetComponent<MousePositionScript>();

        GameManager.singleton.playerList.Add(this);
        StartGame();
        Debug.Log("Starting from game manager ");

        InstantiateCardsBasedOnRoundConfig(GrabRandomOpponentBuildByCurrentRound(GameManager.singleton.playerInScene.playerData.currentRound));
        goldAmount = playerData.currentRound + 1;
        if (goldAmount > 3)
        {
            goldAmount = 3;
        }
        CheckToSeeIfYouHaveEnoughManaForCreature();

    }

    private RoundConfiguration GrabRandomOpponentBuildByCurrentRound(int currentRound)
    {
        string roundConfigFilePath = $"{Application.persistentDataPath}/opponentData/round/{currentRound}/roundConfig.txt";

        if (File.Exists(roundConfigFilePath))
        {
            string roundConfigJson = File.ReadAllText(roundConfigFilePath);
            Debug.LogError(roundConfigJson);
            RoundConfiguration roundConfiguration = JsonUtility.FromJson<RoundConfiguration>(roundConfigJson);
            if (roundConfiguration.allOwnedCards.Count <= 0)
            {

            }
            return roundConfiguration;
        }
        else
        {
            Debug.LogWarning($"Round config for round {currentRound} not found.");
            return null;
        }
    }

    public override void TriggerCreatureMove()
    {
        foreach (Creature kp in creaturesOwned.OrderBy(kp => kp.currentCellPosition.x))
        {
            if (!kp.didAttack)
            {
                kp.OnTurnMoveIfNoCreatures();
            }
        }
    }
    protected override void TiggerCreatureTurn()
    {
        foreach (Creature kp in creaturesOwned.OrderBy(kp => kp.currentCellPosition.x))
        {
            if (kp.AttackIfCreature())
            {
                kp.didAttack = true;
                Debug.Log(kp + " attacking");
            }
            else
            {
                kp.didAttack = false;
            }
        }
    }



    public void InstantiateCardsBasedOnRoundConfig(RoundConfiguration roundConfiguration)
    {
        foreach (CardInHand cardInHand in cardsInHand)
        {
            Destroy(cardInHand.gameObject);
        }
        foreach (Creature creature in creaturesOwned)
        {
            Destroy(creature.gameObject);
        }
        foreach (Farmer farmer in farmersOwned)
        {
            Destroy(farmer.gameObject);
        }
        creaturesOwned.Clear();
        cardsInHand.Clear();
        farmersOwned.Clear();

        allOwnedCardsInScene.Clear();

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
                    SetStateToNothingSelected();
                }
                else
                {
                    CardInHand cardToImmediatelyPlay = InstantiateCardInHand(cardData);
                    if (cardData.cardType == SpellSiegeData.CardType.Farmer)
                    {
                        PurchaseHarvestTile(MirrorVector(cardToImmediatelyPlay.cardData.positionOnBoard));


                        if (selectedOnBoardFarmer != null)
                        {
                            Destroy(selectedOnBoardFarmer.gameObject);
                        }
                        SetStateToNothingSelected();
                    }
                    RemoveCardFromHand(cardToImmediatelyPlay);
                }
            }
            foreach (CardData cardData in roundConfiguration.allOwnedCards)
            {
                if (cardData != null && !cardData.isInHand)
                {
                    CardInHand cardToImmediatelyPlay = InstantiateCardInHand(cardData);

                    if (cardData.cardType == SpellSiegeData.CardType.Creature)
                    {
                        cardToImmediatelyPlay.cardData.positionOnBoard = cardData.positionOnBoard;
                        CastCreatureOnTile(cardToImmediatelyPlay, MirrorVector( cardToImmediatelyPlay.cardData.positionOnBoard ));
                        if (selectedOnBoardCreature != null)
                        {
                            Destroy(selectedOnBoardCreature.gameObject);
                        }

                        SetStateToNothingSelected();
                    }


                    RemoveCardFromHand(cardToImmediatelyPlay);
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

    public void StartRoundFromGameManager()
    {
        GrabAllObjectsFromGameManager();
        mousePositionScript = GetComponent<MousePositionScript>();

        GameManager.singleton.playerList.Add(this);
        StartGame();
        Debug.Log("Starting from game manager ");

        InstantiateCardsBasedOnRoundConfig(GrabRandomOpponentBuildByCurrentRound(GameManager.singleton.playerInScene.playerData.currentRound));
        goldAmount = playerData.currentRound + 1;
        if (goldAmount > 3)
        {
            goldAmount = 3;
        }
        CheckToSeeIfYouHaveEnoughManaForCreature();
    }
}
