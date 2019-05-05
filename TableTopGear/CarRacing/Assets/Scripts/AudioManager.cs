using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXTYPE {
    sys_SkipTurn = 0,
    sys_NextTurn,
    sys_Click, 
    sys_Error,
    sys_BonusDraw
}

public enum CARSFXTYPE {
    skid = 0, 
    overtake,
    land,
    jump,
    lap,
    launch
}

public enum TILEINTERACTIONSFXTYPE {
    coinDrop,
    coinCollect
}

public enum DECKSFXTYPE {
    _none = -1,
    draw,
    land,
    play_Gain,
    play_Gift,
    play_LOL,
    play_Lose,
    play_Pariah,
    play_PushPull,
    play_Restart,
    play_Steal,
    play_Swap,
    play_Reverse
}

public class AudioManager : MonoBehaviour {

    public static AudioManager script;

    private AudioSource as_sfx;

    // System
    private AudioClip[] sfx_Sys_SkipTurn;
    [SerializeField] private float[] sfx_Sys_SkipTurn_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Sys_NextTurn;
    [SerializeField] private float[] sfx_Sys_NextTurn_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Sys_Click;
    [SerializeField] private float[] sfx_Sys_Click_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Sys_Error;
    [SerializeField] private float[] sfx_Sys_Error_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Sys_BonusDraw;
    [SerializeField] private float[] sfx_Sys_BonusDraw_vol = new float[2] { 0.5f, 0.7f };
    // Tiles
    private AudioClip[] sfx_Tile_Refuel;
    [SerializeField] private float[] sfx_Tile_Refuel_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Tile_Breakdown;
    [SerializeField] private float[] sfx_Tile_Breakdown_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Tile_NoBrakes;
    [SerializeField] private float[] sfx_Tile_NoBrakes_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Tile_Mine;
    [SerializeField] private float[] sfx_Tile_Mine_vol = new float[2] { 0.5f, 0.7f };
    // Tile Interactions
    private AudioClip[] sfx_Tile_CoinDrop;
    [SerializeField] private float[] sfx_Tile_CoinDrop_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Tile_CoinCollect;
    [SerializeField] private float[] sfx_Tile_CoinCollect_vol = new float[2] { 0.5f, 0.7f };
    // Cars
    private AudioClip[] sfx_Car_Skid;
    [SerializeField] private float[] sfx_Car_Skid_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Car_Overtake;
    [SerializeField] private float[] sfx_Car_Overtake_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Car_Land;
    [SerializeField] private float[] sfx_Car_Land_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Car_Jump;
    [SerializeField] private float[] sfx_Car_Jump_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Car_Lap;
    [SerializeField] private float[] sfx_Car_Lap_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Car_Launch;
    [SerializeField] private float[] sfx_Car_Launch_vol = new float[2] { 0.5f, 0.7f };
    // Draw
    private AudioClip[] sfx_Deck_Draw;
    [SerializeField] private float[] sfx_Deck_Draw_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_Land;
    [SerializeField] private float[] sfx_Deck_Land_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayGain;
    [SerializeField] private float[] sfx_Deck_PlayGain_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayGift;
    [SerializeField] private float[] sfx_Deck_PlayGift_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayLOL;
    [SerializeField] private float[] sfx_Deck_PlayLOL_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayLose;
    [SerializeField] private float[] sfx_Deck_PlayLose_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayPariah;
    [SerializeField] private float[] sfx_Deck_PlayPariah_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayPushPull;
    [SerializeField] private float[] sfx_Deck_PlayPushPull_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayRestart;
    [SerializeField] private float[] sfx_Deck_PlayRestart_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlaySteal;
    [SerializeField] private float[] sfx_Deck_PlaySteal_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlaySwap;
    [SerializeField] private float[] sfx_Deck_PlaySwap_vol = new float[2] { 0.5f, 0.7f };
    private AudioClip[] sfx_Deck_PlayReverse;
    [SerializeField] private float[] sfx_Deck_PlayReverse_vol = new float[2] { 0.5f, 0.7f };

    [System.Serializable]
    public class SFXRequest {

        public AudioClip clip;
        public float vol;

        public SFXRequest(AudioClip _clip, float _vol) {
            clip = _clip;
            vol = _vol;
        }

    }

    void Awake() {
        script = this;
        as_sfx = transform.Find("AS_SFX").GetComponent<AudioSource>();
        // System
        sfx_Sys_SkipTurn = Resources.LoadAll<AudioClip>("SFX/Systems/NoTurn");
        sfx_Sys_NextTurn = Resources.LoadAll<AudioClip>("SFX/Systems/Turn");
        sfx_Sys_Click = Resources.LoadAll<AudioClip>("SFX/Systems/Clicks/Click");
        sfx_Sys_Error = Resources.LoadAll<AudioClip>("SFX/Systems/Clicks/Error");
        sfx_Sys_BonusDraw = Resources.LoadAll<AudioClip>("SFX/Systems/Dice/BonusDraw");
        // Tiles
        sfx_Tile_Refuel = Resources.LoadAll<AudioClip>("SFX/Tiles/BonusTurn");
        sfx_Tile_Breakdown = Resources.LoadAll<AudioClip>("SFX/Tiles/Breakdown");
        sfx_Tile_NoBrakes = Resources.LoadAll<AudioClip>("SFX/Tiles/NoBrakes");
        sfx_Tile_Mine = Resources.LoadAll<AudioClip>("SFX/Tiles/Mine");
        // Tile Interactions
        sfx_Tile_CoinDrop = Resources.LoadAll<AudioClip>("SFX/Tiles/CoinDrop");
        sfx_Tile_CoinCollect = Resources.LoadAll<AudioClip>("SFX/Tiles/CoinCollect");
        // Car
        sfx_Car_Skid = Resources.LoadAll<AudioClip>("SFX/Cars/CarStart");
        sfx_Car_Overtake = Resources.LoadAll<AudioClip>("SFX/Cars/Overtake");
        sfx_Car_Land = Resources.LoadAll<AudioClip>("SFX/Cars/CarLand");
        sfx_Car_Jump = Resources.LoadAll<AudioClip>("SFX/Cars/CarJump");
        sfx_Car_Lap = Resources.LoadAll<AudioClip>("SFX/Cars/Lap");
        sfx_Car_Launch = Resources.LoadAll<AudioClip>("SFX/Cars/Whooshes");
        // Draw
        sfx_Deck_Draw = Resources.LoadAll<AudioClip>("SFX/Deck/CardDraw");
        sfx_Deck_Land = Resources.LoadAll<AudioClip>("SFX/Deck/CardLand");
        sfx_Deck_PlayGain = Resources.LoadAll<AudioClip>("SFX/Deck/CardGain");
        sfx_Deck_PlayGift = Resources.LoadAll<AudioClip>("SFX/Deck/CardGift");
        sfx_Deck_PlayLOL = Resources.LoadAll<AudioClip>("SFX/Deck/CardLOL");
        sfx_Deck_PlayLose = Resources.LoadAll<AudioClip>("SFX/Deck/CardLose");
        sfx_Deck_PlayPariah = Resources.LoadAll<AudioClip>("SFX/Deck/CardPariah");
        sfx_Deck_PlayPushPull = Resources.LoadAll<AudioClip>("SFX/Deck/CardPushPull");
        sfx_Deck_PlayRestart = Resources.LoadAll<AudioClip>("SFX/Deck/CardRestart");
        sfx_Deck_PlaySteal = Resources.LoadAll<AudioClip>("SFX/Deck/CardSteal");
        sfx_Deck_PlaySwap = Resources.LoadAll<AudioClip>("SFX/Deck/CardSwap");
        sfx_Deck_PlayReverse = Resources.LoadAll<AudioClip>("SFX/Deck/CardReverse");
    }

    public void PlaySound(SFXTYPE t) {
        switch (t) {
            case SFXTYPE.sys_SkipTurn:
                as_sfx.PlayOneShot(sfx_Sys_SkipTurn[Random.Range(0, sfx_Sys_SkipTurn.Length)], Random.Range(sfx_Sys_SkipTurn_vol[0], sfx_Sys_SkipTurn_vol[1]));
                break;
            case SFXTYPE.sys_NextTurn:
                as_sfx.PlayOneShot(sfx_Sys_NextTurn[Random.Range(0, sfx_Sys_NextTurn.Length)], Random.Range(sfx_Sys_NextTurn_vol[0], sfx_Sys_NextTurn_vol[1]));
                break;
            case SFXTYPE.sys_Click:
                as_sfx.PlayOneShot(sfx_Sys_Click[Random.Range(0, sfx_Sys_Click.Length)], Random.Range(sfx_Sys_Click_vol[0], sfx_Sys_Click_vol[1]));
                break;
            case SFXTYPE.sys_Error:
                as_sfx.PlayOneShot(sfx_Sys_Error[Random.Range(0, sfx_Sys_Error.Length)], Random.Range(sfx_Sys_Error_vol[0], sfx_Sys_Error_vol[1]));
                break;
            case SFXTYPE.sys_BonusDraw:
                as_sfx.PlayOneShot(sfx_Sys_BonusDraw[Random.Range(0, sfx_Sys_BonusDraw.Length)], Random.Range(sfx_Sys_BonusDraw_vol[0], sfx_Sys_BonusDraw_vol[1]));
                break;
        }
    }

    public SFXRequest RequestTileSound(TILETYPE t) {
        switch (t) {
            case TILETYPE.refuel: return new SFXRequest(sfx_Tile_Refuel[Random.Range(0, sfx_Tile_Refuel.Length)], Random.Range(sfx_Tile_Refuel_vol[0], sfx_Tile_Refuel_vol[1]));
            case TILETYPE.breakDown: return new SFXRequest(sfx_Tile_Breakdown[Random.Range(0, sfx_Tile_Breakdown.Length)], Random.Range(sfx_Tile_Breakdown_vol[0], sfx_Tile_Breakdown_vol[1]));
            case TILETYPE.noBrakes: return new SFXRequest(sfx_Tile_NoBrakes[Random.Range(0, sfx_Tile_NoBrakes.Length)], Random.Range(sfx_Tile_NoBrakes_vol[0], sfx_Tile_NoBrakes_vol[1]));
            case TILETYPE.mine: return new SFXRequest(sfx_Tile_Mine[Random.Range(0, sfx_Tile_Mine.Length)], Random.Range(sfx_Tile_Mine_vol[0], sfx_Tile_Mine_vol[1]));
        }
        return null;
    }

    public SFXRequest RequestTileInteractionSound(TILEINTERACTIONSFXTYPE t) {
        switch (t) {
            case TILEINTERACTIONSFXTYPE.coinDrop: return new SFXRequest(sfx_Tile_CoinDrop[Random.Range(0, sfx_Tile_CoinDrop.Length)], Random.Range(sfx_Tile_CoinDrop_vol[0], sfx_Tile_CoinDrop_vol[1]));
            case TILEINTERACTIONSFXTYPE.coinCollect: return new SFXRequest(sfx_Tile_CoinCollect[Random.Range(0, sfx_Tile_CoinCollect.Length)], Random.Range(sfx_Tile_CoinCollect_vol[0], sfx_Tile_CoinCollect_vol[1]));
        }
        return null;
    }

    public SFXRequest RequestCarSound(CARSFXTYPE t) {
        switch (t) {
            case CARSFXTYPE.skid: return new SFXRequest(sfx_Car_Skid[Random.Range(0, sfx_Car_Skid.Length)], Random.Range(sfx_Car_Skid_vol[0], sfx_Car_Skid_vol[1]));
            case CARSFXTYPE.overtake: return new SFXRequest(sfx_Car_Overtake[Random.Range(0, sfx_Car_Overtake.Length)], Random.Range(sfx_Car_Overtake_vol[0], sfx_Car_Overtake_vol[1]));
            case CARSFXTYPE.land: return new SFXRequest(sfx_Car_Land[Random.Range(0, sfx_Car_Land.Length)], Random.Range(sfx_Car_Land_vol[0], sfx_Car_Land_vol[1]));
            case CARSFXTYPE.jump: return new SFXRequest(sfx_Car_Jump[Random.Range(0, sfx_Car_Jump.Length)], Random.Range(sfx_Car_Jump_vol[0], sfx_Car_Jump_vol[1]));
            case CARSFXTYPE.lap: return new SFXRequest(sfx_Car_Lap[Random.Range(0, sfx_Car_Lap.Length)], Random.Range(sfx_Car_Lap_vol[0], sfx_Car_Lap_vol[1]));
            case CARSFXTYPE.launch: return new SFXRequest(sfx_Car_Launch[Random.Range(0, sfx_Car_Launch.Length)], Random.Range(sfx_Car_Launch_vol[0], sfx_Car_Launch_vol[1]));
        }
        return null;
    }

    public SFXRequest RequestDeckSound(DECKSFXTYPE t) {
        switch (t) {
            case DECKSFXTYPE.draw: return new SFXRequest(sfx_Deck_Draw[Random.Range(0, sfx_Deck_Draw.Length)], Random.Range(sfx_Deck_Draw_vol[0], sfx_Deck_Draw_vol[1]));
            case DECKSFXTYPE.land: return new SFXRequest(sfx_Deck_Land[Random.Range(0, sfx_Deck_Land.Length)], Random.Range(sfx_Deck_Land_vol[0], sfx_Deck_Land_vol[1]));
            case DECKSFXTYPE.play_Gain: return new SFXRequest(sfx_Deck_PlayGain[Random.Range(0, sfx_Deck_PlayGain.Length)], Random.Range(sfx_Deck_PlayGain_vol[0], sfx_Deck_PlayGain_vol[1]));
            case DECKSFXTYPE.play_Gift: return new SFXRequest(sfx_Deck_PlayGift[Random.Range(0, sfx_Deck_PlayGift.Length)], Random.Range(sfx_Deck_PlayGift_vol[0], sfx_Deck_PlayGift_vol[1]));
            case DECKSFXTYPE.play_LOL: return new SFXRequest(sfx_Deck_PlayLOL[Random.Range(0, sfx_Deck_PlayLOL.Length)], Random.Range(sfx_Deck_PlayLOL_vol[0], sfx_Deck_PlayLOL_vol[1]));
            case DECKSFXTYPE.play_Lose: return new SFXRequest(sfx_Deck_PlayLose[Random.Range(0, sfx_Deck_PlayLose.Length)], Random.Range(sfx_Deck_PlayLose_vol[0], sfx_Deck_PlayLose_vol[1]));
            case DECKSFXTYPE.play_Pariah: return new SFXRequest(sfx_Deck_PlayPariah[Random.Range(0, sfx_Deck_PlayPariah.Length)], Random.Range(sfx_Deck_PlayPariah_vol[0], sfx_Deck_PlayPariah_vol[1]));
            case DECKSFXTYPE.play_PushPull: return new SFXRequest(sfx_Deck_PlayPushPull[Random.Range(0, sfx_Deck_PlayPushPull.Length)], Random.Range(sfx_Deck_PlayPushPull_vol[0], sfx_Deck_PlayPushPull_vol[1]));
            case DECKSFXTYPE.play_Restart: return new SFXRequest(sfx_Deck_PlayRestart[Random.Range(0, sfx_Deck_PlayRestart.Length)], Random.Range(sfx_Deck_PlayRestart_vol[0], sfx_Deck_PlayRestart_vol[1]));
            case DECKSFXTYPE.play_Steal: return new SFXRequest(sfx_Deck_PlaySteal[Random.Range(0, sfx_Deck_PlaySteal.Length)], Random.Range(sfx_Deck_PlaySteal_vol[0], sfx_Deck_PlaySteal_vol[1]));
            case DECKSFXTYPE.play_Swap: return new SFXRequest(sfx_Deck_PlaySwap[Random.Range(0, sfx_Deck_PlaySwap.Length)], Random.Range(sfx_Deck_PlaySwap_vol[0], sfx_Deck_PlaySwap_vol[1]));
            case DECKSFXTYPE.play_Reverse: return new SFXRequest(sfx_Deck_PlayReverse[Random.Range(0, sfx_Deck_PlayReverse.Length)], Random.Range(sfx_Deck_PlayReverse_vol[0], sfx_Deck_PlayReverse_vol[1]));
        }
        return null;
    }

}
