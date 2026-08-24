using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ReturnToLobbyOrRoom : MonoBehaviourPunCallbacks
{
    [Header("Scenes")]
    public string LobbyScene = "GameLobby";
    public string RoomScene = "WaitingRoom";

    // ==========================================
    // RETURN TO GAME LOBBY
    // ==========================================
    public void ReturnToLobby()
    {
        Debug.Log(
            PhotonNetwork.NickName +
            " chose to return to the Game Lobby."
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        PhotonNetwork.LeaveRoom();
    }

    // ==========================================
    // RETURN TO WAITING ROOM
    // ==========================================
    public void ReturnToWaitingRoom()
    {
        Debug.Log(
            PhotonNetwork.NickName +
            " chose to return to the Waiting Room."
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        // Only this player's scene changes.
        // The player stays inside the Photon room.
        SceneManager.LoadScene(RoomScene);
    }

    // ==========================================
    // AFTER LEAVING PHOTON ROOM
    // ==========================================
    public override void OnLeftRoom()
    {
        Debug.Log(
            PhotonNetwork.NickName +
            " left the room. Returning to Game Lobby."
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(LobbyScene);
    }
}