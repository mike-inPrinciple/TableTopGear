using System.Collections;
using UnityEngine;

public class PlayerMotor : MonoBehaviour {

    private Player player;
    private Coroutine COR_Move;

    [SerializeField] private AnimationCurve[] curve_Pitches;
    [SerializeField] private float[] pitchDurations;
    private int currentGear;
    [SerializeField] private int maxGears = 5;
    [SerializeField] private float[] pitchStartingPoints;
    [SerializeField] private float[] pitchEndingPoints;
    [SerializeField] private float[] skidPitches;

    private AudioSource as_Engine;
    private Coroutine COR_Engine;
    private AudioSource as_Skids;
    private AudioSource as_sfx;

    public void Setup(Player p) {
        player = p;
        as_Engine = transform.Find("Audio/AS_Engine").GetComponent<AudioSource>();
        as_Skids = transform.Find("Audio/AS_Skids").GetComponent<AudioSource>();
        as_sfx = transform.Find("Audio/AS_SFX").GetComponent<AudioSource>();
    }

    public void MoveToward(int dieRoll, bool spinning = false) {
        if (COR_Move != null) {
            StopCoroutine(COR_Move);
        }
        COR_Move = StartCoroutine(ExecuteMove(dieRoll, spinning));
    }

    IEnumerator ExecuteMove(int moves, bool spinning = false) {
        StartEngine();
        float passingTime;
        int moveSign = Mathf.RoundToInt(Mathf.Sign(moves));
        Vector3 transformRotation;
        while (Mathf.Abs(moves) > 0) {
            player.nextTile = GameManager.script.GetTile(player.carID, moveSign, player.currentTile);
            player.CheckForOvertake();
            passingTime = 0f;
            transformRotation = transform.rotation.eulerAngles;
            while (passingTime < 1f) {
                passingTime += Time.deltaTime * GameManager.script.carMoveSpeed;
                if (!spinning) {
                    if (Vector3.Distance(transform.position, GameManager.script.lookTarget.position) > GameManager.script.lookMinimumDistance) {
                        transform.LookAt(GameManager.script.lookTarget);
                    }
                } else {
                    transform.rotation = Quaternion.Euler(new Vector3(transformRotation.x, transformRotation.y + Mathf.Lerp(0f, -360f, passingTime), transformRotation.z));
                }
                //transform.position = Vector3.Lerp(currentTile.transform.position, nextTile.transform.position, passingTime);
                if (moveSign > 0) {
                    transform.position = GameManager.script.QuadraticBezier(
                        player.currentTile.transform.position,
                        player.currentTile.forwardControlPoint,
                        player.nextTile.transform.position,
                        passingTime
                    );
                    GameManager.script.lookTarget.position = GameManager.script.QuadraticBezier(
                        player.currentTile.transform.position,
                        player.currentTile.forwardControlPoint,
                        player.nextTile.transform.position,
                        GameManager.script.curve_ForwardLookTarget.Evaluate(passingTime)
                    );
                } else {
                    transform.position = GameManager.script.QuadraticBezier(
                        player.currentTile.transform.position,
                        player.currentTile.backwardControlPoint,
                        player.nextTile.transform.position,
                        passingTime
                    );
                    GameManager.script.lookTarget.position = GameManager.script.QuadraticBezier(
                        player.currentTile.transform.position,
                        player.currentTile.backwardControlPoint,
                        player.nextTile.transform.position,
                        GameManager.script.curve_BackwardLookTarget.Evaluate(passingTime)
                    );
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            player.currentTile = player.nextTile;
            moves = moveSign * (Mathf.Abs(moves) - 1);
            GameManager.script.UpdateRacePositions();
        }
        StopEngine();
        GameManager.script.ReceiveCompletedMove(player);
    }

    IEnumerator ManageEngine() {
        float passingTime = 0f;
        currentGear = 0;
        while (true) {
            passingTime += Time.deltaTime;
            as_Engine.pitch = Mathf.Lerp(pitchStartingPoints[currentGear], pitchEndingPoints[currentGear], curve_Pitches[currentGear].Evaluate(passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
            if (passingTime >= pitchDurations[currentGear]) {
                passingTime = 0f;
                currentGear++;
                currentGear = Mathf.Clamp(currentGear, 0, maxGears);
            }
        }
    }

    public void SingleMove(int dist = 1) {
        if (COR_Move != null) {
            StopCoroutine(COR_Move);
        }
        COR_Move = StartCoroutine(ExecuteSingleMove(dist));
    }

    IEnumerator ExecuteSingleMove(int dist = 1) {
        float passingTime = 0f;
        player.nextTile = GameManager.script.GetTile(player.carID, -dist, player.currentTile);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        PlaySFX(CARSFXTYPE.jump);
        while (passingTime < 1f) {
            passingTime += Time.deltaTime * GameManager.script.overtakeJumpSpeed;
            transform.position = Vector3.Lerp(player.currentTile.transform.position, player.nextTile.transform.position, passingTime) + Vector3.up * Mathf.Lerp(0f, GameManager.script.overtakeJumpHeight, GameManager.script.curve_OvertakeJump.Evaluate(passingTime));
            transform.rotation = Quaternion.Euler(currentRotation + Vector3.right * Mathf.Lerp(0f, -360f, passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        player.CheckForOvertake();
        player.currentTile = player.nextTile;
        GameManager.script.ReceiveCompletedMove(player);
        PlaySFX(CARSFXTYPE.land);
    }

    void StartEngine() {
        PlaySkid(true);
        StopEngine();
        as_Engine.Play();
        COR_Engine = StartCoroutine(ManageEngine());
    }

    void StopEngine() {
        PlaySkid(false);
        as_Engine.Stop();
        if (COR_Engine != null) {
            StopCoroutine(COR_Engine);
        }
    }

    void PlaySkid(bool start) {
        AudioManager.SFXRequest sfx = AudioManager.script.RequestCarSound(CARSFXTYPE.skid);
        as_Skids.pitch = skidPitches[start ? 0 : 1];
        if (sfx != null) {
            as_Skids.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

    public void PlaySFX(CARSFXTYPE t) {
        AudioManager.SFXRequest sfx = AudioManager.script.RequestCarSound(t);
        if (sfx != null) {
            as_sfx.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

    public void PlayTileSFX(TILETYPE t) {
        AudioManager.SFXRequest sfx = AudioManager.script.RequestTileSound(t);
        if (sfx != null) {
            as_sfx.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

    public void PlayDeckSFX(DECKSFXTYPE t) {
        AudioManager.SFXRequest sfx = AudioManager.script.RequestDeckSound(t);
        if (sfx != null) {
            as_sfx.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

}
