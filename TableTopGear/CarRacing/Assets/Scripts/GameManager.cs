using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*

Bugs

1 - Selection bug on spherecollider. Spherecollider is smaller than the one on each tile
3 - Refuel applies even if broken down if landing on it after card --- can't repro

Features

1 - Add a targeting system that displays where clicks are possible

/**/

public enum GAMESTATE {
    splash = 0, 
    title,
    menu,
    gameplay, 
    gameOver
}

public enum TILETYPE {
    _none = -1,
    start,
    standard,
    refuel,
    mine,
    noBrakes,
    breakDown
}

public enum SCORETYPE {
    crossStart = 0,
    landStandard,
    landRefuel,
    landMine,
    landNoBrakes,
    landBreakDown,
    crossOvertake,
    doubleDraw,
    collectStash
}

public enum CARDTYPE {
    _none = -1,
    swap,
    steal_500,
    steal_1000,
    steal_1500,
    skipTurn,
    targetMe,
    pushForward_1,
    pushForward_2,
    pushForward_3,
    pushBackward_1,
    pushBackward_2,
    pushBackward_3,
    lose_500,
    lose_1000,
    restartLap,
    closestBreakDown,
    deployMine,
    closestRefuel,
    gift_500,
    gift_1000,
    gift_1500,
    lolToken,
    gain_250,
    gain_500,
    gain_750,
    gainTurn,
    reverseTurn,
    closestMine,
    closestNoBrakes,
    giveLOL,
    stealLOL,
    clearLOL,
    deployStash
}

public enum TILEOBJECT {
    _none = -1, 
    mine,
    stash
}

public class GameManager : MonoBehaviour {

    public static GameManager script;

    [Header("--- DEBUG ------------------------------------------------------------------------------")]
    [SerializeField] private int debug_WantedDieValue = -1;
    [SerializeField] private CARDTYPE debug_WantedCard = CARDTYPE._none;
    private int[] debug_CalculatedDieValue;
    [SerializeField] private int debug_EndGameID = -1;

    [Header("--- STATE ------------------------------------------------------------------------------")]
    public GAMESTATE gameState;
    [HideInInspector] public bool hasRolled;
    private int activePlayer;
    private bool turnsReversed;
    [HideInInspector] public bool drawPhase;
    [HideInInspector] public bool playCardPhase;
    [HideInInspector] public bool selectVictimPhase;
    [HideInInspector] public bool selectTilePhase;
    [HideInInspector] public bool waitForCardPlayPhase;
    [HideInInspector] public bool discardPhase;
    [HideInInspector] public int cardsToDraw;
    private bool gameOver;

    [Header("--- UI ------------------------------------------------------------------------------")]
    [SerializeField] private GameObject[] screens;
    [SerializeField] private int splashTime = 3;
    [SerializeField] private TextMeshPro text_TitleInstructions;
    private Coroutine COR_Title;

    [Header("--- INFRASTRUCTURE ------------------------------------------------------------------------------")]
    [SerializeField] private int maxPlayers = 8;
    [SerializeField] private Transform board;
    [HideInInspector] public Tile[] startTiles;
    [HideInInspector] public Tile[] tiles;
    private List<Tile> list_UnoccupiedTiles;
    [SerializeField] private AudioSource as_Tiles;
    public Material[] tileBorderMaterials;
    public Material[] tileCoreMaterials;
    [HideInInspector] public Vector3[] controlPoints;
    public Transform lookTarget;
    [SerializeField] private float controlPointMinimumAngle = 15f;
    public AnimationCurve curve_ForwardLookTarget;
    public AnimationCurve curve_BackwardLookTarget;
    public float lookMinimumDistance = 0.05f;
    public int playerCount;
    [SerializeField] private GameObject playerTemplate;
    [HideInInspector] public List<Player> list_Players;
    private List<int> list_TurnSequence;
    private SortedDictionary<float, int> dict_RacePositions;
    private List<int> list_PositionSequence;
    public string[] racePositionNames;
    public GameObject tileObjectTemplate;
    public Mesh[] tileObjectMeshes;
    [SerializeField] private int[] tileObjectStashBoundaries = new int[2] { 1, 51 };
    private List<Player> list_Pariahs;
    private List<Player> list_LOLCounts;
    private List<int> list_LOLStashes;

    [Header("--- DICE ------------------------------------------------------------------------------")]
    public int diceNumber = 2;
    private int diceRolled;
    public int diceFaces = 6;
    private DieWheel[] dieWheels;
    private int dieRoll;
    [SerializeField] private Material dieWheelMaterial;
    public int[] diceRevolutions = new int[2] { 3, 5 };
    public float[] dice60DegreeDurations = new float[2] { 0.1f, 0.15f };
    public AnimationCurve[] curve_Dice;
    public float[] dicePitches = new float[2] { 0.5f, 1.5f };
    public AnimationCurve[] curve_DicePitches;
    public AnimationCurve[] curve_DiceVolumes;
    private int[] diceResults;
    private Coroutine COR_Overlay;

    [Header("--- DECK ------------------------------------------------------------------------------")]
    [SerializeField] private GameObject cardTemplate;
    [HideInInspector] public Transform deckAnchor;
    [HideInInspector] public Transform landingAnchor;
    [HideInInspector] public Transform discardAnchor;
    private List<CARDTYPE> list_DeckCards;
    private List<CARDTYPE> list_TempDeckCards;
    [SerializeField] private int[] maxCardTypeAmounts;
    public string[] cardTypeNames;
    public float cardSpeed = 3f;
    public AnimationCurve curve_CardSpeed;
    public float cardBounceHeight = 5f;
    public AnimationCurve curve_CardBounce;
    public Vector3 cardStartRotation;
    public Vector3 cardEndRotation;
    private TextMeshPro tm_CardStatus;
    private CARDTYPE appointCard;
    private Card card;

    [Header("--- RULES ------------------------------------------------------------------------------")]
    public float carMoveSpeed = 2f;
    public float overtakeJumpSpeed = 2f;
    public float overtakeJumpHeight = 5f;
    public AnimationCurve curve_OvertakeJump;
    public Color[] playerColors;
    public string[] playerColorHexes;
    [SerializeField] private string[] playerColorNames;
    [SerializeField] private int startScore = 500;
    public int targetScore = 5000;
    [SerializeField] private float[] lapScoreRatios;
    public int[] gameScores;
    [SerializeField] private float overlayDuration = 2f;
    [SerializeField] private AnimationCurve curve_OverlayDuration;
    [SerializeField] private string textLOLToken = "<sup>LOL</sup>";
    public int maxLOLCount = 3;

    [Header("--- STASH ------------------------------------------------------------------------------")]
    public float[] stashSpeeds;
    public AnimationCurve[] curve_StashSpeeds;
    public float[] stashYDisplacements;
    public AnimationCurve curve_StashYDisplacements;
    public float[] stashIntervals;
    public Vector3 stashStartingSize;
    private Coroutine COR_JokerStashes;

    [Header("--- GAMEOVER ------------------------------------------------------------------------------")]
    [SerializeField] private int gameOverLapScoreMultiplier = 2000;
    [SerializeField] private int gameOverWinScore = 20000;
    private SortedDictionary<float, int> dict_GameOverPlayerOrder;
    public List<int> list_GameOverSortedIDs;
    public List<int> list_GameOverSortedScores;
    public List<int> list_GameOverSortedOrdinals;
    private int winningPlayer;

    [Header("--- COSMETICS ------------------------------------------------------------------------------")]
    [SerializeField] private GameObject[] pfx_Tiles;
    public string[] pfx_Names;

    [Header("--- TEXT ------------------------------------------------------------------------------")]
    [SerializeField] private TextMeshPro[] text_PlayerScores;
    private TextMeshPro text_Instructions;
    [SerializeField] private TextMeshPro text_Overlay;

#region Start

    void Awake () {
        script = this;
	}
	
	void Start () {
        // Debug
        debug_CalculatedDieValue = new int[diceNumber];
        // Regular
        list_TurnSequence = new List<int>();
        dict_RacePositions = new SortedDictionary<float, int>();
        dict_GameOverPlayerOrder = new SortedDictionary<float, int>();
        list_GameOverSortedIDs = new List<int>();
        list_GameOverSortedScores = new List<int>();
        list_GameOverSortedOrdinals = new List<int>();
        list_PositionSequence = new List<int>();
        list_Players = new List<Player>();
        startTiles = new Tile[board.Find("StartTiles").GetComponentsInChildren<Tile>().Length];
        for (int i = 0; i < startTiles.Length; i++) {
            startTiles[i] = board.Find("StartTiles/" + i.ToString()).GetComponent<Tile>();
        }
        tiles = new Tile[board.Find("Tiles").GetComponentsInChildren<Tile>().Length];
        controlPoints = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++) {
            tiles[i] = board.Find("Tiles/" + i.ToString()).GetComponent<Tile>();
        }
        list_UnoccupiedTiles = new List<Tile>();
        Vector3 dir;
        float dist;
        float angleTest;
        for (int i = 0; i < tiles.Length; i++) {
            if (i == 0) {
                dir = (tiles[i].transform.position - tiles[tiles.Length - 1].transform.position).normalized;
            } else {
                dir = (tiles[i].transform.position - tiles[i - 1].transform.position).normalized; 
            }
            if (i == tiles.Length - 2) {
                angleTest = Vector3.Angle(tiles[0].transform.position - tiles[i + 1].transform.position, tiles[i + 1].transform.position - tiles[i].transform.position);
            } else {
                if (i == tiles.Length - 1) {
                    angleTest = Vector3.Angle(tiles[1].transform.position - tiles[0].transform.position, tiles[0].transform.position - tiles[i].transform.position);
                } else {
                    angleTest = Vector3.Angle(tiles[i + 2].transform.position - tiles[i + 1].transform.position, tiles[i + 1].transform.position - tiles[i].transform.position); 
                }
            }
            if (angleTest < controlPointMinimumAngle) {
                if (i == tiles.Length - 1) {
                    dir = (tiles[0].transform.position - tiles[i].transform.position).normalized;
                } else {
                    dir = (tiles[i + 1].transform.position - tiles[i].transform.position).normalized;
                }
            }
            if (i == tiles.Length - 1) {
                dist = Vector3.Distance(tiles[i].transform.position, tiles[0].transform.position) * 0.5f;
            } else {
                dist = Vector3.Distance(tiles[i].transform.position, tiles[i + 1].transform.position) * 0.5f; 
            }
            controlPoints[i] = tiles[i].transform.position + dir * dist;
        }
        for (int i = 0; i < tiles.Length; i++) {
            if (i == 0) {
                tiles[i].Setup(i, controlPoints[i], controlPoints[controlPoints.Length - 1]);
            } else {
                tiles[i].Setup(i, controlPoints[i], controlPoints[i - 1]);
            }
        }
        dieWheels = new DieWheel[diceNumber];
        diceResults = new int[diceNumber];
        for (int i = 0; i < diceNumber; i++) {
            dieWheels[i] = board.Find("Dice/" + i.ToString() + "/Wheel").GetComponent<DieWheel>();
        }
        list_Pariahs = new List<Player>();
        list_LOLCounts = new List<Player>();
        list_LOLStashes = new List<int>();
        text_Instructions = board.Find("UI/Instructions").GetComponent<TextMeshPro>();
        SetupColorHexes();
        ChangeGameState(GAMESTATE.splash);
    }

#endregion


#region GameState

    public void ChangeGameState(GAMESTATE g) {
        for (int i = 0; i < screens.Length; i++) {
            screens[i].SetActive(false);
        }
        screens[(int)g].SetActive(true);
        gameState = g;
        switch (g) {
            case GAMESTATE.splash:
                StartCoroutine(SplashRoutine(splashTime));
                break;
            case GAMESTATE.title:
                if (COR_Title != null) {
                    StopCoroutine(COR_Title);
                }
                COR_Title = StartCoroutine(TitleRoutine());
                break;
            case GAMESTATE.gameplay:
                SetupMatch();
                break;
            case GAMESTATE.gameOver:
                GameOverManager.script.SetupGameOver();
                break;
        }
        
    }

    IEnumerator SplashRoutine(int wait) {
        yield return new WaitForSeconds(wait);
        ChangeGameState(GAMESTATE.title);
    }

    IEnumerator TitleRoutine() {
        while (gameState == GAMESTATE.title) {
            text_TitleInstructions.SetText("Click anywhere to play");
            yield return new WaitForSeconds(1);
            text_TitleInstructions.SetText("");
            yield return new WaitForSeconds(1);
        }
    }

    public void QuitMatch() {
        StopAllLocalCoroutines();
        if (card != null) {
            Destroy(card.gameObject);
        }
        DestroyPlayers();
        ChangeGameState(GAMESTATE.menu);
    }

    void StopAllLocalCoroutines() {
        StopAllCoroutines();
    }

#endregion


#region Setup

    void SetupColorHexes() {
        playerColorHexes = new string[maxPlayers];
        for (int i = 0; i < maxPlayers; i++) {
            playerColorHexes[i] = ColorUtility.ToHtmlStringRGB(playerColors[i]);
        }
    }

    void SetupMatch() {
        gameOver = false;
        DestroyPlayers();
        SetupPlayers();
        SetupTurns();
        for (int i = 0; i < text_PlayerScores.Length; i++) {
            if (i < playerCount) {
                text_PlayerScores[i].color = playerColors[list_TurnSequence[i]];
            } else {
                text_PlayerScores[i].SetText("");
            }
        }
        ResetScores();
        SetupDeck();
        ResetBoard();
    }

    void SetupTurns() {
        List<int> turns = new List<int>();
        for (int i = 0; i < playerCount; i++) {
            turns.Add(i);
        }
        int rand;
        list_TurnSequence.Clear();
        for (int i = 0; i < playerCount; i++) {
            rand = Random.Range(0, turns.Count);
            list_TurnSequence.Add(turns[rand]);
            turns.RemoveAt(rand);
        }
        ChangeTurn(0);
    }

    void SetupPlayers() {
        for (int i = 0; i < startTiles.Length; i++) {
            startTiles[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < playerCount; i++) {
            startTiles[i].gameObject.SetActive(true);
            list_Players.Add(Instantiate(playerTemplate, startTiles[i].transform.position, startTiles[i].transform.rotation).GetComponent<Player>());
            list_Players[i].Setup(i, -list_TurnSequence.IndexOf(i) - 1);
        }
    }

    void DestroyPlayers() {
        for (int i = 0; i < list_Players.Count; i++) {
            Destroy(list_Players[i].gameObject);
        }
        list_Players.Clear();
    }

    void SetupDeck() {
        deckAnchor = board.Find("Cards/Deck/SpawnPoint");
        landingAnchor = board.Find("Cards/Deck/LandingPoint");
        discardAnchor = board.Find("Cards/Deck/EndPoint");
        tm_CardStatus = board.Find("Cards/Deck/Text_Status").GetComponent<TextMeshPro>();
        tm_CardStatus.SetText("");
        list_DeckCards = new List<CARDTYPE>();
        list_TempDeckCards = new List<CARDTYPE>();
        for (int ta = 0; ta < maxCardTypeAmounts.Length; ta++) {
            for (int t = 0; t < maxCardTypeAmounts[ta]; t++) {
                list_TempDeckCards.Add((CARDTYPE)ta);
            }
        }
        int rand;
        while (list_TempDeckCards.Count > 0) {
            rand = Random.Range(0, list_TempDeckCards.Count);
            list_DeckCards.Add(list_TempDeckCards[rand]);
            list_TempDeckCards.RemoveAt(rand);
        }
    }

    void ResetBoard() {
        for (int i = 0; i < tiles.Length; i++) {
            tiles[i].DestroyTileObject();
        }
    }

#endregion


#region Update

    void Update(){
        if (gameState == GAMESTATE.gameplay) {
            if (debug_EndGameID > -1) {
                list_Players[debug_EndGameID].ChangeScore(targetScore);
                TestEndGame(debug_EndGameID);
                debug_EndGameID = -1;
            }
        }
    }

    public void ReceiveCompletedRoll() {
        diceRolled++;
        if (diceRolled == diceNumber) {
            list_Players[list_TurnSequence[activePlayer]].MoveToward(dieRoll);
            switch (dieRoll) {
                case 8:
                case 11:
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color> rolled an " + dieRoll.ToString() + "!");
                    break;
                default:
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color> rolled a " + dieRoll.ToString() + "!");
                    break;
            }
            if ((float)dieRoll / (float)diceNumber == (float)diceResults[0]) {
                cardsToDraw += 2;
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(gameScores[(int)SCORETYPE.doubleDraw]);
                Instantiate(
                   pfx_Tiles[(int)SCORETYPE.doubleDraw],
                   board.Find("Dice").position,
                   board.Find("Dice").rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.doubleDraw, list_Players[list_TurnSequence[activePlayer]].carID);
                AudioManager.script.PlaySound(SFXTYPE.sys_BonusDraw);
            } else {
                cardsToDraw++;
            }
            tm_CardStatus.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">Cards to draw: " + cardsToDraw.ToString() + "</color>");
        }
    }

    public void ReceiveCompletedMove(Player p) {
        EvaluateLandingTile(p);
        UpdateRacePositions();
    }

    IEnumerator ChangeTurnSequence(bool turnAvailable) {
        if (turnAvailable) {
            AudioManager.script.PlaySound(SFXTYPE.sys_NextTurn);
        } else {
            AudioManager.script.PlaySound(SFXTYPE.sys_SkipTurn);
        }
        text_Overlay.SetText(
            "<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>\n" + 
            (turnAvailable ? "You're next!" : "No turn for you...")
        );
        text_Overlay.text = text_Overlay.text.Replace("\\n", "\n");
        Color sColor = new Color(1f, 1f, 1f, 0f);
        Color eColor = new Color(1f, 1f, 1f, 1f);
        text_Overlay.color = sColor;
        float passingTime = 0f;
        while (passingTime <= 1f) {
            passingTime += Time.deltaTime / overlayDuration;
            text_Overlay.color = Color.Lerp(sColor, eColor, curve_OverlayDuration.Evaluate(passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (turnAvailable) {
            cardsToDraw = 0;
            dieWheelMaterial.SetColor("_Color", playerColors[list_TurnSequence[activePlayer]]);
            hasRolled = false;
            text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>'s turn! Click on the DICE WHEELS");
        } else {
            ChangeTurn();
        }
        UpdateRacePositions();
    }

#endregion


#region Interactions

    public void StartRoll() {
        AudioManager.script.PlaySound(SFXTYPE.sys_Click);
        StartTurn();
    }

    public void DrawCard() {
        AudioManager.script.PlaySound(SFXTYPE.sys_Click);
        drawPhase = false;
        playCardPhase = true;
        cardsToDraw--;
        tm_CardStatus.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">Cards to draw: " + cardsToDraw.ToString() + "</color>");
        card = Instantiate(cardTemplate, Vector3.zero, Quaternion.identity).GetComponent<Card>();
        if (debug_WantedCard != CARDTYPE._none) {
            card.Setup(debug_WantedCard);
        } else {
            card.Setup(list_DeckCards[0]);
        }
        list_DeckCards.Add(list_DeckCards[0]);
        list_DeckCards.RemoveAt(0);
    }

    public void PlayCard(CARDTYPE ct) {
        switch (ct) {
            case CARDTYPE.lose_500:
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(-500);
                DiscardPhase();
                break;
            case CARDTYPE.lose_1000:
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(-1000);
                DiscardPhase();
                break;
            case CARDTYPE.gain_250:
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(250);
                DiscardPhase();
                break;
            case CARDTYPE.gain_500:
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(500);
                DiscardPhase();
                break;
            case CARDTYPE.gain_750:
                list_Players[list_TurnSequence[activePlayer]].ChangeScore(750);
                DiscardPhase();
                break;
            case CARDTYPE.skipTurn:
                list_Players[list_TurnSequence[activePlayer]].AddBreakDown();
                Instantiate(
                    pfx_Tiles[(int)SCORETYPE.landBreakDown],
                    list_Players[list_TurnSequence[activePlayer]].currentTile.transform.position,
                    pfx_Tiles[(int)SCORETYPE.landBreakDown].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landBreakDown, list_TurnSequence[activePlayer]);
                DiscardPhase();
                break;
            case CARDTYPE.gainTurn:
                list_Players[list_TurnSequence[activePlayer]].AddRefuel();
                Instantiate(
                    pfx_Tiles[(int)SCORETYPE.landRefuel],
                    list_Players[list_TurnSequence[activePlayer]].currentTile.transform.position,
                    pfx_Tiles[(int)SCORETYPE.landRefuel].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landRefuel, list_TurnSequence[activePlayer]);
                DiscardPhase();
                break;
            case CARDTYPE.restartLap:
                list_Players[list_TurnSequence[activePlayer]].SingleMove(list_Players[list_TurnSequence[activePlayer]].currentTile.tileID);
                waitForCardPlayPhase = true;
                break;
            case CARDTYPE.closestBreakDown:
                list_Players[list_TurnSequence[activePlayer]].SingleMove(-GetClosestTileDirection(list_Players[list_TurnSequence[activePlayer]].currentTile.tileID, TILETYPE.breakDown));
                waitForCardPlayPhase = true;
                break;
            case CARDTYPE.closestRefuel:
                list_Players[list_TurnSequence[activePlayer]].SingleMove(-GetClosestTileDirection(list_Players[list_TurnSequence[activePlayer]].currentTile.tileID, TILETYPE.refuel));
                waitForCardPlayPhase = true;
                break;
            case CARDTYPE.closestMine:
                list_Players[list_TurnSequence[activePlayer]].SingleMove(-GetClosestTileDirection(list_Players[list_TurnSequence[activePlayer]].currentTile.tileID, TILETYPE.mine));
                waitForCardPlayPhase = true;
                break;
            case CARDTYPE.closestNoBrakes:
                list_Players[list_TurnSequence[activePlayer]].SingleMove(-GetClosestTileDirection(list_Players[list_TurnSequence[activePlayer]].currentTile.tileID, TILETYPE.noBrakes));
                waitForCardPlayPhase = true;
                break;
            case CARDTYPE.targetMe:
                list_Players[list_TurnSequence[activePlayer]].pariah = true;
                DiscardPhase();
                break;
            case CARDTYPE.pushBackward_1:
            case CARDTYPE.pushBackward_2:
            case CARDTYPE.pushBackward_3:
            case CARDTYPE.pushForward_1:
            case CARDTYPE.pushForward_2:
            case CARDTYPE.pushForward_3:
                appointCard = ct;
                VictimPhase();
                break;
            case CARDTYPE.steal_500:
            case CARDTYPE.steal_1000:
            case CARDTYPE.steal_1500:
            case CARDTYPE.gift_500:
            case CARDTYPE.gift_1000:
            case CARDTYPE.gift_1500:
                CheckForPariah();
                if (list_Pariahs.Count == 1 && list_Pariahs.Contains(list_Players[list_TurnSequence[activePlayer]])) {
                    DiscardPhase();
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, you're the only pariah! Discard...");
                } else {
                    appointCard = ct;
                    VictimPhase();
                }
                break;
            case CARDTYPE.swap:
                if (OpponentsOnTrack()) {
                    CheckForPariah();
                    if (list_Pariahs.Count == 1 && list_Pariahs.Contains(list_Players[list_TurnSequence[activePlayer]])) {
                        DiscardPhase();
                        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, you're the only pariah! Discard...");
                    } else {
                        appointCard = ct;
                        VictimPhase();
                    }
                } else {
                    DiscardPhase();
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, nobody to swap with! Discard...");
                }
                break;
            case CARDTYPE.deployMine:
            case CARDTYPE.deployStash:
                appointCard = ct;
                TilePhase();
                break;
            case CARDTYPE.lolToken:
                list_Players[list_TurnSequence[activePlayer]].IncrementLOL();
                DiscardPhase();
                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, the Joker draws near... Discard...");
                break;
            case CARDTYPE.clearLOL:
                list_Players[list_TurnSequence[activePlayer]].IncrementLOL(-1);
                DiscardPhase();
                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, the Joker is repelled... Discard...");
                break;
            case CARDTYPE.giveLOL:
                appointCard = ct;
                VictimPhase();
                break;
            case CARDTYPE.stealLOL:
                if (CheckForLOLCount()) {
                    CheckForPariah();
                    if (list_Pariahs.Count == 1) {
                        if (list_Pariahs.Contains(list_Players[list_TurnSequence[activePlayer]])) {
                            DiscardPhase();
                            text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, you're the only pariah! Discard...");
                        } else {
                            if (list_Pariahs[0].lolCount == 0) {
                                DiscardPhase();
                                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, the pariah doesn't have jokers! Discard...");
                            } else {
                                appointCard = ct;
                                VictimPhase();
                            }
                        }     
                    } else {
                        if (list_Pariahs.Count > 0) {
                            int test = 0;
                            for (int i = 0; i < list_Pariahs.Count; i++) {
                                if (list_Pariahs[i] != list_Players[list_TurnSequence[activePlayer]]) {
                                    test += list_Pariahs[i].lolCount;
                                }
                            }
                            if (test > 0) {
                                appointCard = ct;
                                VictimPhase();
                            } else {
                                DiscardPhase();
                                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, the pariahs don't have jokers! Discard...");
                            }
                        } else {
                            appointCard = ct;
                            VictimPhase();
                        }
                    }
                } else {
                    DiscardPhase();
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, no Jokers to steal... Discard...");
                }
                break;
            case CARDTYPE.reverseTurn:
                ReverseTurns();
                DiscardPhase();
                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, turns reversed! Discard...");
                break;
            default:
                Debug.Log("card type not supported yet " + ct);
                DiscardPhase();
                break;
        }
        playCardPhase = false;
        UpdateRacePositions();
    }

    public void AppointVictim(Player p) {
        if (CheckForPariah()) {
            if (!p.pariah) {
                AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's a pariah!");
                return;
            }
        } 
        switch (appointCard) {
            case CARDTYPE.pushBackward_1:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(1);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.pushBackward_2:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(2);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.pushBackward_3:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(3);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.pushForward_1:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(-1);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.pushForward_2:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(-2);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.pushForward_3:
                if (p.nextTile != null) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.SingleMove(-3);
                    selectVictimPhase = false;
                    waitForCardPlayPhase = true;
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_PushPull);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                    return;
                }
                break;
            case CARDTYPE.steal_500:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(p.GiveUpScore(500));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Gain);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't steal from yourself!");
                    return;
                }
                break;
            case CARDTYPE.steal_1000:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(p.GiveUpScore(1000));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Gain);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't steal from yourself!");
                    return;
                }
                break;
            case CARDTYPE.steal_1500:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(p.GiveUpScore(1500));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Gain);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't steal from yourself!");
                    return;
                }
                break;
            case CARDTYPE.gift_500:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.ChangeScore(Mathf.Min(500, list_Players[list_TurnSequence[activePlayer]].score));
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(-Mathf.Min(500, list_Players[list_TurnSequence[activePlayer]].score));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Lose);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't gift to yourself!");
                    return;
                }
                break;
            case CARDTYPE.gift_1000:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.ChangeScore(Mathf.Min(1000, list_Players[list_TurnSequence[activePlayer]].score));
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(-Mathf.Min(1000, list_Players[list_TurnSequence[activePlayer]].score));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Lose);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't gift to yourself!");
                    return;
                }
                break;
            case CARDTYPE.gift_1500:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                    p.ChangeScore(Mathf.Min(1500, list_Players[list_TurnSequence[activePlayer]].score));
                    list_Players[list_TurnSequence[activePlayer]].ChangeScore(-Mathf.Min(1500, list_Players[list_TurnSequence[activePlayer]].score));
                    DiscardPhase();
                    RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Lose);
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't gift to yourself!");
                    return;
                }
                break;
            case CARDTYPE.swap:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    if (p.nextTile != null) {
                        AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                        int tempDist = GetOpponentRelativeDistance(list_Players[list_TurnSequence[activePlayer]], p);
                        p.SingleMove(tempDist);
                        list_Players[list_TurnSequence[activePlayer]].SingleMove(-tempDist);
                        selectVictimPhase = false;
                        waitForCardPlayPhase = true;
                        RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_Swap);
                    } else {
                        AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a player that's on the track!");
                        return;
                    }
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't swap places with yourself!");
                    return;
                }
                break;
            case CARDTYPE.stealLOL:
                if (p != list_Players[list_TurnSequence[activePlayer]]) {
                    if (p.lolCount == 0) {
                        AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, no Jokers here!!!");
                        return;
                    } else {
                        AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                        p.IncrementLOL(-1);
                        list_Players[list_TurnSequence[activePlayer]].IncrementLOL();
                        DiscardPhase();
                        RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_LOL);
                    }
                } else {
                    AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                    text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, can't steal from yourself!");
                    return;
                }
                break;
            case CARDTYPE.giveLOL:
                AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                p.IncrementLOL();
                DiscardPhase();
                RequestCardPlayerSound(p, TILETYPE._none, DECKSFXTYPE.play_LOL);
                break;

        }
        if (CheckForPariah()) {
            p.pariah = false;
            UpdateRacePositions();
        }

    }

    public void AppointTile(Tile t) {
        switch (appointCard) {
            case CARDTYPE.deployMine:
                switch (t.tileType) {
                    case TILETYPE.standard:
                        if (CheckTileOccupied(t)) {
                            AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                            text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select an unoccupied tile!");
                        } else {
                            AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                            t.AddTileObject(TILEOBJECT.mine, list_TurnSequence[activePlayer]);
                            DiscardPhase();
                        }
                        break;
                    default:
                        AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, you must select a normal tile!");
                        break;
                }
                break;
            case CARDTYPE.deployStash:
                switch (t.tileType) {
                    case TILETYPE.standard:
                        if (CheckTileOccupied(t)) {
                            AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                            text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select an unoccupied tile!");
                        } else {
                            AudioManager.script.PlaySound(SFXTYPE.sys_Click);
                            t.AddTileObject(TILEOBJECT.stash, list_TurnSequence[activePlayer], Random.Range(tileObjectStashBoundaries[0], tileObjectStashBoundaries[1]) * 10);
                            DiscardPhase();
                        }
                        break;
                    default:
                        AudioManager.script.PlaySound(SFXTYPE.sys_Error);
                        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, you must select a normal tile!");
                        break;
                }
                break;
        }
    }

    public void Discard() {
        AudioManager.script.PlaySound(SFXTYPE.sys_Click);
        card.Discard();
        discardPhase = false;
        if (cardsToDraw > 0) {
            DrawPhase();
        } else {
            if (hasRolled) {
                ChangeTurn();
                UpdateRacePositions();
            }
        }
    }

#endregion


#region Events

    void StartTurn() {
        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color> is rolling...");
        dieRoll = 0;
        diceRolled = 0;
        if (debug_WantedDieValue > -1) {
            GetDebugDieValue();
        } else {
            for (int i = 0; i < diceNumber; i++) {
                debug_CalculatedDieValue[i] = 0;
            }
        }
        for (int i = 0; i < diceNumber; i++) {
            diceResults[i] = dieWheels[i].Roll(debug_CalculatedDieValue[i] - 1);
            dieRoll += diceResults[i];
        }
        hasRolled = true;
    }

    void DrawPhase() {
        drawPhase = true;
        playCardPhase = false;
        discardPhase = false;
        waitForCardPlayPhase = false;
        selectVictimPhase = false;
        selectTilePhase = false;
        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, draw a card!");
    }

    void VictimPhase() {
        drawPhase = false;
        playCardPhase = false;
        discardPhase = false;
        waitForCardPlayPhase = false;
        selectVictimPhase = true;
        selectTilePhase = false;
        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a victim!");
    }

    void TilePhase() {
        drawPhase = false;
        playCardPhase = false;
        discardPhase = false;
        waitForCardPlayPhase = false;
        selectVictimPhase = false;
        selectTilePhase = true;
        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, select a valid tile!");
    }

    void DiscardPhase() {
        drawPhase = false;
        playCardPhase = false;
        discardPhase = true;
        waitForCardPlayPhase = false;
        selectVictimPhase = false;
        selectTilePhase = false;
        text_Instructions.SetText("<color=#" + playerColorHexes[list_TurnSequence[activePlayer]] + ">" + playerColorNames[list_TurnSequence[activePlayer]] + "</color>, discard...");
        UpdateScoreTexts();
        UpdateRacePositions();
    }

    void ChangeTurn(int specific = -1) {
        drawPhase = false;
        playCardPhase = false;
        discardPhase = false;
        waitForCardPlayPhase = false;
        selectVictimPhase = false;
        selectTilePhase = false;
        if (specific > -1) {
            activePlayer = specific;
        } else {
            if (turnsReversed) {
                activePlayer--;
            } else {
                activePlayer++;
            }
            if (activePlayer >= playerCount) {
                activePlayer = 0;
            } else {
                if (activePlayer < 0) {
                    activePlayer = playerCount - 1;
                }
            }
        }
        if (COR_Overlay != null) {
            StopCoroutine(COR_Overlay);
        }
        COR_Overlay = StartCoroutine(ChangeTurnSequence(list_Players[list_TurnSequence[activePlayer]].CheckTurnAvailable()));
        if (tm_CardStatus != null) {
            tm_CardStatus.SetText("");
        }
    }

    public bool Overtake(Player p, Tile t) {
        for (int i = 0; i < playerCount; i++) {
            if (i != p.carID) {
                if (list_Players[i].nextTile == t) {
                    p.ChangeScore(gameScores[(int)SCORETYPE.crossOvertake]);
                    list_Players[i].SingleMove();
                    Instantiate(
                        pfx_Tiles[(int)SCORETYPE.crossOvertake],
                        p.nextTile.transform.position,
                        pfx_Tiles[(int)SCORETYPE.crossOvertake].transform.rotation
                    ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.crossOvertake, p.carID);
                    p.PlaySFX(CARSFXTYPE.overtake);
                    return true;
                }
            }
        }
        return false;
    }

    void EvaluateLandingTile(Player p) {
        if (gameOver) {
            ChangeGameState(GAMESTATE.gameOver);
            return;
        }
        p.PlayTileSFX(p.nextTile.tileType);
        if (p.currentTile.tileObject != null) {
            switch (p.currentTile.tileObject.objectType) {
                case TILEOBJECT.stash:
                    RequestTileSound(TILEINTERACTIONSFXTYPE.coinCollect, p.transform.position);
                    p.ChangeScore(p.currentTile.tileObject.value);
                    Instantiate(
                        pfx_Tiles[(int)SCORETYPE.collectStash],
                        list_Players[p.carID].nextTile.transform.position,
                        pfx_Tiles[(int)SCORETYPE.collectStash].transform.rotation
                    ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.collectStash, p.carID);
                    p.currentTile.DestroyTileObject();
                    break;
            }
        }
        switch (p.currentTile.tileType) {
            case TILETYPE.start:
                p.ChangeScore(gameScores[(int)SCORETYPE.landStandard]);
                break;
            case TILETYPE.standard:
                if (p.currentTile.tileObject != null) {
                    switch (p.currentTile.tileObject.objectType) {
                        case TILEOBJECT.mine:
                            if (p.currentTile.tileObject.playerID != p.carID) {
                                p.PlayTileSFX(TILETYPE.mine);
                                p.ChangeScore(gameScores[(int)SCORETYPE.landMine]);
                                p.SingleMove(3);
                                Instantiate(
                                    pfx_Tiles[(int)SCORETYPE.landMine],
                                    p.currentTile.transform.position,
                                    pfx_Tiles[(int)SCORETYPE.landMine].transform.rotation
                                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landMine, p.carID);
                                p.currentTile.DestroyTileObject();
                            } else {
                                p.ChangeScore(gameScores[(int)SCORETYPE.landStandard]);
                            }
                            break;
                    }
                } else {
                    p.ChangeScore(gameScores[(int)SCORETYPE.landStandard]);
                }
                break;
            case TILETYPE.refuel:
                p.ChangeScore(gameScores[(int)SCORETYPE.landRefuel]);
                p.AddRefuel();
                Instantiate(
                   pfx_Tiles[(int)SCORETYPE.landRefuel],
                   p.currentTile.transform.position,
                   pfx_Tiles[(int)SCORETYPE.landRefuel].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landRefuel, p.carID);
                break;
            case TILETYPE.mine:
                p.ChangeScore(gameScores[(int)SCORETYPE.landMine]);
                p.SingleMove(3);
                Instantiate(
                    pfx_Tiles[(int)SCORETYPE.landMine],
                    p.currentTile.transform.position,
                    pfx_Tiles[(int)SCORETYPE.landMine].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landMine, p.carID);
                return;
            case TILETYPE.noBrakes:
                p.ChangeScore(gameScores[(int)SCORETYPE.landNoBrakes]);
                Instantiate(
                    pfx_Tiles[(int)SCORETYPE.landNoBrakes],
                    p.currentTile.transform.position + Vector3.up * 0.2f,
                    pfx_Tiles[(int)SCORETYPE.landNoBrakes].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landNoBrakes, p.carID);
                p.MoveToward(p.goingForward ? 1 : -1, true);
                return;
            case TILETYPE.breakDown:
                p.ChangeScore(gameScores[(int)SCORETYPE.landBreakDown]);
                p.AddBreakDown();
                Instantiate(
                    pfx_Tiles[(int)SCORETYPE.landBreakDown],
                    p.currentTile.transform.position,
                    pfx_Tiles[(int)SCORETYPE.landBreakDown].transform.rotation
                ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.landBreakDown, p.carID);
                break;
        }
        if (p == list_Players[list_TurnSequence[activePlayer]]) {
            if (p.CheckRefuelAvailable()) {
                if (waitForCardPlayPhase) {
                    if (p.brokenDownTurns > 0) {
                        DiscardPhase();
                        text_Instructions.SetText("<color=#" + playerColorHexes[p.carID] + ">" + playerColorNames[p.carID] + "</color>, more fuel for next turn! Discard...");
                        //Debug.Log("player had just broken down, give back a refuel");
                        p.AddRefuel();
                    } else {
                        hasRolled = false;
                        DiscardPhase();
                        text_Instructions.SetText("<color=#" + playerColorHexes[p.carID] + ">" + playerColorNames[p.carID] + "</color>, discard then roll!");
                        //Debug.Log("active player must discard first");
                    }
                } else {
                    if (p.brokenDownTurns > 0) {
                        p.AddRefuel();
                        if (!drawPhase) {
                            if (!waitForCardPlayPhase && !discardPhase) {
                                DrawPhase();
                            } else {
                                DiscardPhase();
                                return;
                            }
                        }
                        text_Instructions.SetText("<color=#" + playerColorHexes[p.carID] + ">" + playerColorNames[p.carID] + "</color>, keep your fuel for next turn! Draw a card...");
                        //Debug.Log("player just broke down during his first part of the turn, give back a refuel");
                    } else {
                        hasRolled = false;
                        text_Instructions.SetText("<color=#" + playerColorHexes[p.carID] + ">" + playerColorNames[p.carID] + "</color>, roll again!");
                        //Debug.Log("active player didn't draw");
                    }
                    return;
                }
            }
            switch (p.currentTile.tileType) {
                case TILETYPE.start:
                case TILETYPE.standard:
                case TILETYPE.breakDown:
                    if (!drawPhase) {
                        if (!waitForCardPlayPhase && !discardPhase) { 
                            DrawPhase();
                        } else {
                            DiscardPhase();
                        }
                    }
                    break;
            }
        } else {
            if (waitForCardPlayPhase) {
                switch (p.currentTile.tileType) {
                    case TILETYPE.start:
                    case TILETYPE.standard:
                    case TILETYPE.breakDown:
                    case TILETYPE.refuel:
                        DiscardPhase();
                        break;
                }
            }
        }
    }

    void ReverseTurns() {
        turnsReversed = !turnsReversed;
    }

    public void RequestCardPlayerSound(Player p, TILETYPE t = TILETYPE._none, DECKSFXTYPE d = DECKSFXTYPE._none) {
        if (t != TILETYPE._none) {
            p.PlayTileSFX(t);
        }
        if (d != DECKSFXTYPE._none) {
            p.PlayDeckSFX(d);
        }
    }

    public void JokerStashes(int pID, int total) {
        list_LOLStashes.Clear();
        int rand;
        while (total > 0) {
            rand = Random.Range(tileObjectStashBoundaries[0], tileObjectStashBoundaries[1]) * 10;
            rand = Mathf.Clamp(rand, 10, total);
            list_LOLStashes.Add(rand);
            total -= rand;
        }
        GetUnoccupiedTileCount();
        while (list_LOLStashes.Count > list_UnoccupiedTiles.Count) {
            list_LOLStashes.Sort();
            list_LOLStashes[1] += list_LOLStashes[0];
            list_LOLStashes.RemoveAt(0);
        }
        if (COR_JokerStashes != null) {
            StopCoroutine(COR_JokerStashes);
        }
        COR_JokerStashes = StartCoroutine(LaunchJokerStashes(pID));
    }

    IEnumerator LaunchJokerStashes(int pID) {
        int rand;
        int otherRand;
        while (list_LOLStashes.Count > 0) {
            rand = Random.Range(0, list_LOLStashes.Count);
            otherRand = Random.Range(0, list_UnoccupiedTiles.Count);
            list_UnoccupiedTiles[otherRand].AddTileObject(TILEOBJECT.stash, pID, list_LOLStashes[rand], true);
            list_LOLStashes.RemoveAt(rand);
            list_UnoccupiedTiles.RemoveAt(otherRand);
            yield return new WaitForSeconds(Random.Range(stashIntervals[0], stashIntervals[1]));
        }
    }

    public void RequestTileSound(TILEINTERACTIONSFXTYPE t, Vector3 pos) {
        as_Tiles.transform.position = pos;
        AudioManager.SFXRequest sfx = AudioManager.script.RequestTileInteractionSound(t);
        if (sfx != null) {
            as_Tiles.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

#endregion


#region Calculations

    public Tile GetTile(int playerID, int direction, Tile t) {
        int returnable;
        returnable = t.tileID + direction;
        if (returnable < 0) {
            list_Players[playerID].IncrementLap(-1);
            returnable += tiles.Length;
        } else {
            if (returnable >= tiles.Length) {
                if (list_Players[playerID].laps == list_Players[playerID].highestLap) {
                    TestEndGame(playerID);
                    list_Players[playerID].ChangeScore(Mathf.RoundToInt((float)gameScores[(int)SCORETYPE.crossStart] * lapScoreRatios[list_Players[playerID].racePosition]));
                    Instantiate(
                        pfx_Tiles[(int)SCORETYPE.crossStart],
                        list_Players[playerID].nextTile.transform.position,
                        pfx_Tiles[(int)SCORETYPE.crossStart].transform.rotation
                    ).GetComponent<PFX_SelfManager>().Setup((int)SCORETYPE.crossStart, playerID);
                }
                list_Players[playerID].IncrementLap(1);
                returnable -= tiles.Length;
            }
        }
        return tiles[returnable];
    }

    public Vector3 QuadraticBezier(Vector3 sp, Vector3 cp, Vector3 ep, float c) {
        Vector3 p0 = Vector3.Lerp(sp, cp, c);
        Vector3 p1 = Vector3.Lerp(cp, ep, c);
        return Vector3.Lerp(p0, p1, c);
    }

    public void UpdateRacePositions() {
        dict_RacePositions.Clear();
        float score;
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[i].nextTile == null) {
                score = (float)list_Players[i].racePosition;
            } else {
                score = (float)(list_Players[i].laps * tiles.Length + list_Players[i].nextTile.tileID);
            }
            while (dict_RacePositions.ContainsKey(score)) {
                score += 0.001f;
            }
            dict_RacePositions.Add(score, i);
        }
        list_PositionSequence.Clear();
        foreach (KeyValuePair<float, int> p in dict_RacePositions) {
            list_PositionSequence.Add(p.Value);
        }
        list_PositionSequence.Reverse();
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[list_PositionSequence[i]].nextTile != null) {
                list_Players[list_PositionSequence[i]].UpdateRacePosition(i);
            }
        }
    }

    int GetClosestTileDirection(int tileID, TILETYPE tt) {
        int closestForward = 0;
        int currentID = tileID;
        while (true) {
            closestForward++;
            currentID++;
            if (currentID >= tiles.Length) {
                closestForward = 100;
                break;
            }
            if (tiles[currentID].tileType == tt) {
                break;
            }
        }
        int closestBackward = 0;
        currentID = tileID;
        while (true) {
            closestBackward++;
            currentID--;
            if (currentID < 0) {
                closestBackward = 100;
                break;
            }
            if (tiles[currentID].tileType == tt) {
                break;
            }
        }
        if (closestForward > closestBackward) {
            //Debug.Log("back " + closestBackward);
            return -closestBackward;
        } else {
            if (closestForward < closestBackward) {
                //Debug.Log("forward " + closestForward);
                return closestForward;
            } else {
                //Debug.Log("equal therefore back " + closestBackward);
                return -closestBackward;
            }
        }
    }

    int GetOpponentRelativeDistance(Player p, Player o) {
        return o.currentTile.tileID - p.currentTile.tileID;
    }

    bool OpponentsOnTrack() {
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[list_TurnSequence[activePlayer]] != list_Players[i]) {
                if (list_Players[i].nextTile != null) {
                    return true;
                }
            }
        }
        return false;
    }

    bool CheckForPariah() {
        list_Pariahs.Clear();
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[i].pariah) {
                list_Pariahs.Add(list_Players[i]);
            }
        }
        return list_Pariahs.Count > 0;
    }

    bool CheckForLOLCount() {
        list_LOLCounts.Clear();
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[i].lolCount > 0 && i != list_TurnSequence[activePlayer]) {
                list_LOLCounts.Add(list_Players[i]);
            }
        }
        return list_LOLCounts.Count > 0;
    }

    bool CheckTileOccupied(Tile t) {
        for (int i = 0; i < playerCount; i++) {
            if (list_Players[i].currentTile == t) {
                return true;
            }
            if (t.tileObject != null) {
                return true;
            }
        }
        return false;
    }

    public Player GetActivePlayer() {
        return list_Players[list_TurnSequence[activePlayer]];
    }

    void TestEndGame(int playerID) {
        if (list_Players[playerID].score >= targetScore) {
            winningPlayer = playerID;
            CalculateGameOverScore();
            gameOver = true;
            //Debug.Log(playerColorNames[playerID] + " has won");
        }
    }

    void GetUnoccupiedTileCount() {
        list_UnoccupiedTiles.Clear();
        for (int i = 0; i < tiles.Length; i++) {
            if (!CheckTileOccupied(tiles[i])) {
                list_UnoccupiedTiles.Add(tiles[i]);
            }
        }
    }

    void CalculateGameOverScore() {
        dict_GameOverPlayerOrder.Clear();
        list_GameOverSortedIDs.Clear();
        list_GameOverSortedScores.Clear();
        list_GameOverSortedOrdinals.Clear();
        float tempScore;
        for (int i = 0; i < playerCount; i++) {
            tempScore = (float)GetGameOverScore(i);
            while (dict_GameOverPlayerOrder.ContainsKey(tempScore)) {
                tempScore += 0.0001f;
            }
            dict_GameOverPlayerOrder.Add(tempScore, i);
        }
        foreach (KeyValuePair<float, int> k in dict_GameOverPlayerOrder) {
            list_GameOverSortedIDs.Add(k.Value);
            list_GameOverSortedScores.Add(Mathf.RoundToInt(k.Key));
        }
        list_GameOverSortedIDs.Reverse();
        list_GameOverSortedScores.Reverse();
        int previousValue = -1;
        int currentValue = -1;
        int counter = -1;
        for (int i = 0; i < list_GameOverSortedIDs.Count; i++) {
            if (currentValue < 0) {
                counter++;
                currentValue = list_GameOverSortedScores[i];
                previousValue = currentValue;
                list_GameOverSortedOrdinals.Add(counter);
            } else {
                currentValue = list_GameOverSortedScores[i];
                if (currentValue != previousValue) {
                    counter++;
                    previousValue = currentValue;
                    list_GameOverSortedOrdinals.Add(counter);
                } else {
                    list_GameOverSortedOrdinals.Add(counter);
                }
            }
        }
    }

    int GetGameOverScore(int pID) {
        int returnable = 0;
        returnable += list_Players[pID].score;
        returnable += list_Players[pID].highestLap * gameOverLapScoreMultiplier;
        if (pID == winningPlayer) {
            returnable += gameOverWinScore;
        }
        return returnable;
    }

#endregion


#region Score

    void ResetScores() {
        for (int i = 0; i < playerCount; i++) {
            list_Players[i].ChangeScore(0, startScore);
        }
        UpdateScoreTexts();
    }

    public void UpdateScoreTexts() {
        string temp;
        for (int i = 0; i < playerCount; i++) {
            temp = "Lap " + (Mathf.Clamp(list_Players[list_TurnSequence[i]].highestLap, 0, Mathf.Abs(list_Players[list_TurnSequence[i]].highestLap)) + 1).ToString() + "\n$" +
                string.Format("{0:n0}", list_Players[list_TurnSequence[i]].score) + "\n";
            for (int j = 0; j < list_Players[list_TurnSequence[i]].lolCount; j++) {
                if (j > 0) {
                    temp += " ";
                }
                temp += textLOLToken;
            }
            text_PlayerScores[i].SetText(temp);
            text_PlayerScores[i].text = text_PlayerScores[i].text.Replace("\\n", "\n");
        }
    }

    public string GetWinText(int pID) {
        return "<color=#" + playerColorHexes[pID] + ">" + playerColorNames[pID] + "</color> has won!";
    }

#endregion

    
    // debug
    void GetDebugDieValue() {
        for (int i = 0; i < diceNumber; i++) {
            debug_CalculatedDieValue[i] = debug_WantedDieValue / diceNumber;
        }
        if (debug_WantedDieValue % diceNumber > 0) {
            debug_CalculatedDieValue[diceNumber - 1]++;
        }
    }

    void DebugGetNextCard() {
        
    }

}
