using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour {

    public TILEOBJECT objectType;
    public int playerID;
    public int value;

    private MeshFilter mf;
    private MeshRenderer mr;

    private Coroutine COR_Launch;

    private Tile parentTile;

    public void Setup(Tile t, TILEOBJECT o, int pID, int val = -1, bool launch = false) {
        parentTile = t;
        objectType = o;
        playerID = pID;
        mf = GetComponent<MeshFilter>();
        mf.mesh = GameManager.script.tileObjectMeshes[(int)o];
        mr = GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color", GameManager.script.playerColors[pID]);
        value = val;
        if (launch) {
            GameManager.script.list_Players[pID].PlaySFX(CARSFXTYPE.launch);
            if (COR_Launch != null) {
                StopCoroutine(COR_Launch);
            }
            COR_Launch = StartCoroutine(LaunchRoutine(pID));
        }
    }

    IEnumerator LaunchRoutine(int pID) {
        float passingTime = 0f;
        float speed = Random.Range(GameManager.script.stashSpeeds[0], GameManager.script.stashSpeeds[1]);
        AnimationCurve curve_Speed = GameManager.script.curve_StashSpeeds[Random.Range(0, GameManager.script.curve_StashSpeeds.Length)];
        float yDisplacement = Random.Range(GameManager.script.stashYDisplacements[0], GameManager.script.stashYDisplacements[1]);
        Vector3 startingSize = Vector3.zero;
        switch (objectType) {
            case TILEOBJECT.stash:
                startingSize = GameManager.script.stashStartingSize;
                break;
        }
        while (passingTime <= 1f) {
            passingTime += Time.deltaTime * speed;
            transform.position = 
                Vector3.Lerp(GameManager.script.list_Players[pID].transform.position, parentTile.transform.position, curve_Speed.Evaluate(passingTime)) +
                new Vector3(0f, Mathf.Lerp(0f, yDisplacement, GameManager.script.curve_StashYDisplacements.Evaluate(passingTime)));
            transform.localScale = Vector3.Lerp(startingSize, Vector3.one, curve_Speed.Evaluate(passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        switch (objectType) {
            case TILEOBJECT.stash:
                GameManager.script.RequestTileSound(TILEINTERACTIONSFXTYPE.coinDrop, parentTile.transform.position);  
                break;
        }
        
    }

}
