using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

    private CARDTYPE cardType;
    private TMPro.TextMeshPro tm;

    private AudioSource as_sfx;

    public void Setup(CARDTYPE ct) {
        as_sfx = transform.Find("AS_SFX").GetComponent<AudioSource>();
        cardType = ct;
        tm = transform.Find("Title").GetComponent<TMPro.TextMeshPro>();
        tm.SetText(GameManager.script.cardTypeNames[(int)cardType]);
        //tm.text = tm.text.Replace("\\n", "\n");
        transform.position = GameManager.script.deckAnchor.position;
        transform.rotation = Quaternion.Euler(GameManager.script.cardStartRotation);
        StartCoroutine(Uncover(ct));
    }

    IEnumerator Uncover(CARDTYPE ct) {
        as_sfx = transform.Find("AS_SFX").GetComponent<AudioSource>();
        PlaySFX(DECKSFXTYPE.draw);
        float passingTime = 0f;
        while (passingTime <= 1f) {
            passingTime += Time.deltaTime * GameManager.script.cardSpeed;
            transform.position = Vector3.Lerp(GameManager.script.deckAnchor.position, GameManager.script.landingAnchor.position, GameManager.script.curve_CardSpeed.Evaluate(passingTime)) +
                new Vector3(0f, Mathf.Lerp(0f, GameManager.script.cardBounceHeight, GameManager.script.curve_CardBounce.Evaluate(passingTime)), 0f);
            transform.rotation = Quaternion.Euler(Vector3.Lerp(GameManager.script.cardStartRotation, GameManager.script.cardEndRotation, GameManager.script.curve_CardSpeed.Evaluate(passingTime)));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        GameManager.script.PlayCard(cardType);
        PlaySFX(DECKSFXTYPE.land);
        switch (ct) {
            case CARDTYPE.gainTurn: GameManager.script.RequestCardPlayerSound(GameManager.script.GetActivePlayer(), TILETYPE.refuel); break;
            case CARDTYPE.skipTurn: GameManager.script.RequestCardPlayerSound(GameManager.script.GetActivePlayer(), TILETYPE.breakDown); break;
            case CARDTYPE.gain_250: case CARDTYPE.gain_500: case CARDTYPE.gain_750: PlaySFX(DECKSFXTYPE.play_Gain); break;
            case CARDTYPE.lose_500: case CARDTYPE.lose_1000: PlaySFX(DECKSFXTYPE.play_Lose); break;
            case CARDTYPE.gift_500: case CARDTYPE.gift_1000: case CARDTYPE.gift_1500: PlaySFX(DECKSFXTYPE.play_Gift); break;
            case CARDTYPE.steal_500: case CARDTYPE.steal_1000: case CARDTYPE.steal_1500: PlaySFX(DECKSFXTYPE.play_Steal); break;
            case CARDTYPE.lolToken: case CARDTYPE.clearLOL: case CARDTYPE.stealLOL: case CARDTYPE.giveLOL: PlaySFX(DECKSFXTYPE.play_LOL); break;
            case CARDTYPE.restartLap: PlaySFX(DECKSFXTYPE.play_Restart); break;
            case CARDTYPE.reverseTurn: PlaySFX(DECKSFXTYPE.play_Reverse); break;
            case CARDTYPE.targetMe: PlaySFX(DECKSFXTYPE.play_Pariah); break;
        }
    }

    public void Discard() {
        StartCoroutine(DiscardNow());
        PlaySFX(DECKSFXTYPE.draw);
    }

    IEnumerator DiscardNow() {
        float passingTime = 0f;
        while (passingTime <= 1f) {
            passingTime += Time.deltaTime * GameManager.script.cardSpeed;
            transform.position = Vector3.Lerp(GameManager.script.landingAnchor.position, GameManager.script.discardAnchor.position, GameManager.script.curve_CardSpeed.Evaluate(passingTime));
            //transform.rotation = Quaternion.Euler(Vector3.Lerp(GameManager.script.cardEndRotation, GameManager.script.cardStartRotation, GameManager.script.curve_CardSpeed.Evaluate(passingTime)));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Destroy(gameObject);
    }

    void PlaySFX(DECKSFXTYPE t) {
        AudioManager.SFXRequest sfx = AudioManager.script.RequestDeckSound(t);
        if (sfx != null) {
            as_sfx.PlayOneShot(sfx.clip, sfx.vol);
        }
    }

}
