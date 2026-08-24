using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostStartButton : MonoBehaviourPunCallbacks
{
    [Header("Map Selector")]
    [SerializeField] private MapSelector mapSelector;

    [Header("Map Scenes")]
    [SerializeField] private string scene1 = "DodgeballMap1";
    [SerializeField] private string scene2 = "DodgeballMap2";
    [SerializeField] private string scene3 = "DodgeballMap3";
    [SerializeField] private string scene4 = "DodgeballMap4";

    [Header("Lobby")]
    [SerializeField] private string gameLobbyScene = "GameLobby";

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

    public void ReturnToGameLobby()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        // Make sure the game isn't paused
        Time.timeScale = 1f;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Only the Master Client should change the scene
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Returning to Game Lobby.");

            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.Log("Only the Master Client can return to the Game Lobby.");
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Successfully left the room. Loading Game Lobby.");

        SceneManager.LoadScene(gameLobbyScene);
    }
}