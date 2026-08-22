using UnityEngine;
using Photon.Pun;

public class HostStartButton : MonoBehaviour
{
    [Header("Map Selector")]
    [SerializeField] private MapSelector mapSelector;

    [Header("Map Scenes")]
    [SerializeField] private string scene1 = "DodgeballMap1";
    [SerializeField] private string scene2 = "DodgeballMap2";
    [SerializeField] private string scene3 = "DodgeballMap3";
    [SerializeField] private string scene4 = "DodgeballMap4";

    public void StartGame()
    {
        // Only the Master Client can start the game
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (mapSelector == null)
        {
            Debug.LogError("MapSelector is not assigned.");
            return;
        }

        string selectedScene = mapSelector.GetSelectedScene();

        // Random was selected
        if (selectedScene == "Random")
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

        if (string.IsNullOrEmpty(selectedScene))
        {
            Debug.LogWarning("No map selected.");
            return;
        }

        Debug.Log("Starting game on map: " + selectedScene);

        PhotonNetwork.LoadLevel(selectedScene);
    }
}