using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour {

    public static GameOverManager script;

    private Transform podiumContainer;
    [SerializeField] private GameObject podiumColumnTemplate;
    private List<Transform> list_PodiumColumns;
    private List<Transform> list_PodiumModelContainers;
    private List<Transform> list_PodiumAnchors;
    private List<TextMeshPro> list_PodiumCaptions;
    [SerializeField] private float podiumColumnMaxSize = 20f;
    [SerializeField] private float podiumColumnSizeDelta = 10f;
    [SerializeField] private float podiumColumnSpacing = 2f;
    [SerializeField] private float endGameCarScale = 0.5f;
    [SerializeField] private TextMeshPro text_Winner;

    void Awake() {
        script = this;
    }

    void Start() {
        podiumContainer = transform.Find("Board/PodiumContainer");
        list_PodiumColumns = new List<Transform>();
        list_PodiumModelContainers = new List<Transform>();
        list_PodiumAnchors = new List<Transform>();
        list_PodiumCaptions = new List<TextMeshPro>();
    }

    public void SetupGameOver() {
        ResetGameOver();
        float startX = (float)(GameManager.script.playerCount / 2) * -podiumColumnSpacing + podiumColumnSpacing / 2f - (float)(GameManager.script.playerCount % 2) * podiumColumnSpacing / 2f;
        for (int i = 0; i < GameManager.script.playerCount; i++) {
            list_PodiumColumns.Add(Instantiate(podiumColumnTemplate, Vector3.zero, podiumContainer.rotation).transform);
            list_PodiumModelContainers.Add(list_PodiumColumns[i].Find("ModelContainer"));
            list_PodiumAnchors.Add(list_PodiumColumns[i].Find("ModelContainer/Model/Anchor"));
            list_PodiumCaptions.Add(list_PodiumColumns[i].Find("TextContainer/Text_Position").GetComponent<TextMeshPro>());
            list_PodiumColumns[i].parent = podiumContainer;
            list_PodiumColumns[i].localPosition = Vector3.right * (startX + (float)i * podiumColumnSpacing);
            list_PodiumColumns[i].localRotation = Quaternion.identity;
            list_PodiumModelContainers[i].localScale = new Vector3(1f, 1f, podiumColumnMaxSize - (float)GameManager.script.list_GameOverSortedOrdinals[i] / (float)(GameManager.script.playerCount - 1) * podiumColumnSizeDelta);
            GameManager.script.list_Players[GameManager.script.list_GameOverSortedIDs[i]].GoToGameOver();
            GameManager.script.list_Players[GameManager.script.list_GameOverSortedIDs[i]].transform.position = list_PodiumAnchors[i].position;
            GameManager.script.list_Players[GameManager.script.list_GameOverSortedIDs[i]].transform.rotation = Quaternion.Euler(list_PodiumAnchors[i].rotation.eulerAngles + Vector3.up * 180f);
            GameManager.script.list_Players[GameManager.script.list_GameOverSortedIDs[i]].transform.localScale = Vector3.one * endGameCarScale;
            list_PodiumCaptions[i].SetText((GameManager.script.list_GameOverSortedOrdinals[i] + 1).ToString());
        }
        text_Winner.SetText(GameManager.script.GetWinText(GameManager.script.list_GameOverSortedIDs[0]));
    }

    void ResetGameOver() {
        for (int i = 0; i < list_PodiumColumns.Count; i++) {
            Destroy(list_PodiumColumns[i].gameObject);
        }
        list_PodiumColumns.Clear();
        list_PodiumModelContainers.Clear();
        list_PodiumAnchors.Clear();
        list_PodiumCaptions.Clear();
    }

}
