using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HostStartButton : MonoBehaviour
{
    [Header("Map Buttons")]
    [SerializeField] private Button mapButton1;
    [SerializeField] private Button mapButton2;
    [SerializeField] private Button mapButton3;
    [SerializeField] private Button mapButton4;
    [SerializeField] private Button randomButton;

    [Header("Map Highlights")]
    [SerializeField] private GameObject mapHighlight1;
    [SerializeField] private GameObject mapHighlight2;
    [SerializeField] private GameObject mapHighlight3;
    [SerializeField] private GameObject mapHighlight4;
    [SerializeField] private GameObject randomHighlight;

    [Header("Map Scenes")]
    [SerializeField] private string scene1 = "DodgeballMap1";
    [SerializeField] private string scene2 = "DodgeballMap2";
    [SerializeField] private string scene3 = "DodgeballMap3";
    [SerializeField] private string scene4 = "DodgeballMap4";

    private string selectedScene = "";
    private bool randomSelected = false;

    private void Start()
    {
        if (mapButton1 != null)
            mapButton1.onClick.AddListener(() => SelectMap(scene1, 1));

        if (mapButton2 != null)
            mapButton2.onClick.AddListener(() => SelectMap(scene2, 2));

        if (mapButton3 != null)
            mapButton3.onClick.AddListener(() => SelectMap(scene3, 3));

        if (mapButton4 != null)
            mapButton4.onClick.AddListener(() => SelectMap(scene4, 4));

        if (randomButton != null)
            randomButton.onClick.AddListener(SelectRandom);

        ClearHighlights();
    }

    private void SelectMap(string sceneName, int mapNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        selectedScene = sceneName;
        randomSelected = false;

        ClearHighlights();

        switch (mapNumber)
        {
            case 1:
                SetImageAlpha(mapHighlight1, 1f);
                break;

            case 2:
                SetImageAlpha(mapHighlight2, 1f);
                break;

            case 3:
                SetImageAlpha(mapHighlight3, 1f);
                break;

            case 4:
                SetImageAlpha(mapHighlight4, 1f);
                break;
        }
    }

    private void SelectRandom()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        selectedScene = "";
        randomSelected = true;

        ClearHighlights();

        SetImageAlpha(randomHighlight, 1f);
    }

    private void ClearHighlights()
    {
        SetImageAlpha(mapHighlight1, 0f);
        SetImageAlpha(mapHighlight2, 0f);
        SetImageAlpha(mapHighlight3, 0f);
        SetImageAlpha(mapHighlight4, 0f);
        SetImageAlpha(randomHighlight, 0f);
    }

    private void SetImageAlpha(GameObject highlight, float alpha)
    {
        if (highlight == null)
            return;

        Image image = highlight.GetComponent<Image>();

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (string.IsNullOrEmpty(selectedScene) && !randomSelected)
        {
            Debug.Log("Cannot start game: No map has been selected.");
            return;
        }

        if (randomSelected)
        {
            string[] maps =
            {
                scene1,
                scene2,
                scene3,
                scene4
            };

            selectedScene = maps[Random.Range(0, maps.Length)];
        }

        Debug.Log("Starting game on map: " + selectedScene);

        PhotonNetwork.LoadLevel(selectedScene);
    }
}