using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BUTTON_MENU {
    player2 = 0,
    player3,
    player4,
    player5,
    player6,
    player7,
    player8,
    quit
}

public class InputManager : MonoBehaviour {

    public static InputManager script;

    private Ray ray;
    private RaycastHit hit;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask buttonMask;
    [SerializeField] private Transform[] menuButtons;
    [SerializeField] private Transform gameplayQuitButton;
    [SerializeField] private Transform gameOverQuitButton;

    void Awake() {
        script = this;
    }

    void Update() {
        switch (GameManager.script.gameState) {
            case GAMESTATE.title:
                if (Input.GetMouseButtonDown(0)) {
                    GameManager.script.ChangeGameState(GAMESTATE.menu);
                }
                break;
            case GAMESTATE.menu:
                if (Input.GetMouseButtonDown(0)) {
                    ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, buttonMask)) {
                        for (int i = 0; i < menuButtons.Length; i++) {
                            if (hit.transform == menuButtons[i]) {
                                switch ((BUTTON_MENU)i) {
                                    case BUTTON_MENU.quit:
                                        Application.Quit();
                                        break;
                                    default:
                                        GameManager.script.playerCount = i + 2;
                                        GameManager.script.ChangeGameState(GAMESTATE.gameplay);
                                        break;
                                }
                                return;
                            }
                        }
                    }
                }
                break;
            case GAMESTATE.gameplay:
                if (Input.GetMouseButtonDown(0)) {
                    ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit)) {
                        //Debug.Log(hit.transform.name);
                        switch (hit.transform.tag) {
                            case "_Dice":
                                if (!GameManager.script.hasRolled && !GameManager.script.discardPhase) {
                                    GameManager.script.StartRoll();
                                }
                                break;
                            case "_Deck":
                                if (GameManager.script.drawPhase) {
                                    GameManager.script.DrawCard();
                                }
                                break;
                            case "_Card":
                                if (GameManager.script.discardPhase) {
                                    GameManager.script.Discard();
                                }
                                break;
                            case "_Player":
                                //Debug.Log("Hit " + hit.transform.GetComponent<Player>().carID);
                                if (GameManager.script.selectVictimPhase) {
                                    GameManager.script.AppointVictim(hit.transform.GetComponent<Player>());
                                }
                                if (GameManager.script.selectTilePhase) {
                                    GameManager.script.AppointTile(hit.transform.GetComponent<Player>().currentTile);
                                }
                                break;
                            case "_Tile":
                                if (GameManager.script.selectTilePhase) {
                                    GameManager.script.AppointTile(hit.transform.GetComponent<Tile>());
                                }
                                break;
                            case "_Button":
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity, buttonMask)) {
                                    if (hit.transform == gameplayQuitButton) {
                                        GameManager.script.QuitMatch();
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
            case GAMESTATE.gameOver:
                if (Input.GetMouseButtonDown(0)) {
                    ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit)) {
                        switch (hit.transform.tag) {
                            case "_Button":
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity, buttonMask)) {
                                    if (hit.transform == gameOverQuitButton) {
                                        GameManager.script.QuitMatch();
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
        }
        
    }

}
