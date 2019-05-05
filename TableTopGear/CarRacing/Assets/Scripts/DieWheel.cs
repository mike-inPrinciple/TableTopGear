using System.Collections;
using UnityEngine;

public class DieWheel : MonoBehaviour {

    private int rollValue;
    private int rollRevolutions;
    private float rollRotation;
    private float rollDuration;
    private AnimationCurve curve_Roll;

    private Coroutine COR_Animate;

    private AudioSource as_sfx;
    private AnimationCurve curve_Pitch;
    private AnimationCurve curve_Vol;

    void Start() {
        as_sfx = GetComponent<AudioSource>();
        as_sfx.enabled = false;
    }

    public int Roll(int wantedValue = -1) {
        CalculateValues(wantedValue);
        CalculateRotations();
        if (COR_Animate != null) {
            StopCoroutine(COR_Animate);
        }
        COR_Animate = StartCoroutine(AnimateRoll());
        return rollValue + 1;
    }

    IEnumerator AnimateRoll() {
        as_sfx.enabled = true;
        as_sfx.Play();
        float passingTime = 0f;
        while (passingTime < 1f) {
            passingTime += Time.deltaTime / rollDuration;
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, Vector3.right * rollRotation, curve_Roll.Evaluate(passingTime)));
            as_sfx.pitch = Mathf.Lerp(GameManager.script.dicePitches[0], GameManager.script.dicePitches[1], curve_Pitch.Evaluate(passingTime));
            as_sfx.volume = Mathf.Lerp(0f, 1f, curve_Vol.Evaluate(passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        GameManager.script.ReceiveCompletedRoll();
        as_sfx.enabled = false;
    }

    void CalculateValues(int wantedValue = -1) {
        if (wantedValue < 0) {
            rollValue = Random.Range(0, GameManager.script.diceFaces);
        } else {
            rollValue = wantedValue;
        }
    }

    void CalculateRotations() {
        rollRevolutions = Random.Range(GameManager.script.diceRevolutions[0], GameManager.script.diceRevolutions[1]);
        rollRotation = (float)rollValue * 60f + 360f * (float)rollRevolutions;
        rollDuration = rollRotation / 60f * Random.Range(GameManager.script.dice60DegreeDurations[0], GameManager.script.dice60DegreeDurations[1]);
        int rand = Random.Range(0, GameManager.script.curve_Dice.Length);
        curve_Roll = GameManager.script.curve_Dice[rand];
        curve_Pitch = GameManager.script.curve_DicePitches[rand];
        curve_Vol = GameManager.script.curve_DiceVolumes[rand];
    }

}
