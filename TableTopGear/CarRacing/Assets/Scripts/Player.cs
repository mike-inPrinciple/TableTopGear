using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Player : MonoBehaviour {

    public Tile currentTile;
    public Tile nextTile;

    private bool firstMove;

    public int carID;
    private MeshRenderer mr;
    private PlayerMotor pm;
    public int laps;
    public int highestLap;
    public int racePosition;
    public int lolCount;

    public bool pariah;
    public int brokenDownTurns;
    public int refuelTurns;
    public int score;
    public bool goingForward;

    private Transform racePositionContainer;
    private TextMeshPro tm_RacePosition;

    private PlayerSiren ps;
    [SerializeField] private GameObject fxMoney;

    private Vector3 previousPosition;

    public void Setup(int id, int firstTurn) {
        laps = 0;
        highestLap = 0;
        lolCount = 0;
        carID = id;
        ps = transform.Find("Siren").GetComponent<PlayerSiren>();
        mr = transform.Find("Model").GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color", GameManager.script.playerColors[id]);
        pm = GetComponent<PlayerMotor>();
        pm.Setup(this);
        racePositionContainer = transform.Find("TextContainer");
        tm_RacePosition = racePositionContainer.Find("Text_Position").GetComponent<TextMeshPro>();
        firstMove = true;
        UpdateRacePosition(firstTurn);
        brokenDownTurns = 0;
        refuelTurns = 0;
        pariah = false;
        ps.Setup(carID);
    }

    void LateUpdate() {
        racePositionContainer.rotation = Quaternion.identity;
    }

    public void MoveToward(int dieRoll, bool spinning = false) {
        if (firstMove) {
            firstMove = false;
            currentTile = GameManager.script.tiles[0];
        }
        pm.MoveToward(dieRoll, spinning);
        goingForward = (dieRoll > 0);
    }

    public void CheckForOvertake() {
        GameManager.script.Overtake(this, nextTile);
    }

    public void SingleMove(int dist = 1) {
        pm.SingleMove(dist);
        goingForward = (dist < 0);
    }

    public void AddBreakDown(int turns = 1) {
        brokenDownTurns += turns;
    }

    public void AddRefuel(int turns = 1) {
        refuelTurns += turns;
    }

    public void IncrementLOL(int increment = 1) {
        lolCount += increment;
        lolCount = Mathf.Clamp(lolCount, 0, GameManager.script.maxLOLCount);
        if (lolCount == GameManager.script.maxLOLCount) {
            GoBankrupt();
        }
    }

    public bool CheckTurnAvailable() {
        if (brokenDownTurns > 0) {
            brokenDownTurns--;
            return false;
        }
        return true;
    }

    public bool CheckRefuelAvailable() {
        if (refuelTurns > 0) {
            refuelTurns--;
            return true;
        }
        return false;
    }

    public void GoBankrupt() {
        lolCount = 0;
        int halfScore = (score / 10) / 2;
        halfScore *= 10;
        GameManager.script.JokerStashes(carID, halfScore);
        ChangeScore(0, score - halfScore);
        if (transform.position != previousPosition) {
            Instantiate(fxMoney, transform.position, fxMoney.transform.rotation).GetComponent<PFX_SelfManager>().Setup(-1, carID, "LOSE HALF!!!");
        } else {
            Instantiate(fxMoney, transform.position, fxMoney.transform.rotation).GetComponent<PFX_SelfManager>().Setup(-1, carID, "LOSE HALF!!!", 1);
        }
        previousPosition = transform.position;
    }

    public void ChangeScore(int increment = 0, int specific = 0) {
        if (specific > 0) {
            score = specific;
        } else {
            if (increment < 0) {
                if (Mathf.Abs(increment) > score) {
                    increment = -score;
                }
            }
            score += increment;
            if (transform.position != previousPosition) {
                Instantiate(fxMoney, transform.position, fxMoney.transform.rotation).GetComponent<PFX_SelfManager>().Setup(-1, carID, (increment > 0 ? "+" : "") + increment.ToString() + "$");
            } else {
                Instantiate(fxMoney, transform.position, fxMoney.transform.rotation).GetComponent<PFX_SelfManager>().Setup(-1, carID, (increment > 0 ? "+" : "") + increment.ToString() + "$", 1);
            }
            previousPosition = transform.position;
        }
        score = Mathf.Clamp(score, 0, Mathf.Abs(score));
        GameManager.script.UpdateScoreTexts();
        if (score >= GameManager.script.targetScore) {
            if (!ps.isActive) {
                ps.TurnOnOff(true);
            }
        } else {
            if (ps.isActive) {
                ps.TurnOnOff(false);
            }
        }
    }

    public void UpdateRacePosition(int pos, bool visible = true) {
        if (visible) {
            racePosition = pos;
            if (currentTile == null) {
                tm_RacePosition.SetText(
                    (pariah ? "\n" + GameManager.script.cardTypeNames[(int)CARDTYPE.targetMe] : "") +
                    (brokenDownTurns > 0 ? "\n" + "Skip x" + brokenDownTurns.ToString() : "") +
                    (refuelTurns > 0 ? "\n" + "Bonus x" + refuelTurns.ToString() : "")
                );
            } else {
                tm_RacePosition.SetText(
                    GameManager.script.racePositionNames[pos] +
                    (pariah ? "\n" + GameManager.script.cardTypeNames[(int)CARDTYPE.targetMe] : "") +
                    (brokenDownTurns > 0 ? "\n" + "Skip x" + brokenDownTurns.ToString() : "") +
                    (refuelTurns > 0 ? "\n" + "Bonus x" + refuelTurns.ToString() : "")
                );
            }
        } else {
            tm_RacePosition.gameObject.SetActive(false);
        }
    }

    public void IncrementLap(int dir) {
        laps += dir;
        if (highestLap < laps) {
            highestLap = laps;
            PlaySFX(CARSFXTYPE.lap);
        }
    }

    public int GiveUpScore(int amount) {
        int returnable = Mathf.Min(score, amount);
        ChangeScore(-returnable);
        return returnable;
    }

    public void GoToGameOver() {
        UpdateRacePosition(racePosition, false);
        if (ps.isActive) {
            ps.TurnOnOff(false);
        }
    }

    public void PlaySFX(CARSFXTYPE t) {
        pm.PlaySFX(t);
    }

    public void PlayTileSFX(TILETYPE t) {
        pm.PlayTileSFX(t);
    }

    public void PlayDeckSFX(DECKSFXTYPE t) {
        pm.PlayDeckSFX(t);
    }

}
