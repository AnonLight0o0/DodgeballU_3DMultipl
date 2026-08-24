using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ReturnToLobbyOrRoom : MonoBehaviourPunCallbacks
{
    [Header("Scenes")]
    public string LobbyScene = "GameLobby";
    public string RoomScene = "WaitingRoom";

    // Keeps track of where the player wants to go after leaving Photon
    private bool returnToWaitingRoom = false;

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

        // Tell OnLeftRoom where we want to go
        returnToWaitingRoom = false;

        // Leave Photon room first
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

        // Tell OnLeftRoom where we want to go
        returnToWaitingRoom = true;

        // Leave Photon room first
        PhotonNetwork.LeaveRoom();
    }

    // ==========================================
    // AFTER LEAVING PHOTON ROOM
    // ==========================================
    public override void OnLeftRoom()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        if (returnToWaitingRoom)
        {
            Debug.Log(
                PhotonNetwork.NickName +
                " left the room. Returning to Waiting Room."
            );

            PhotonNetwork.LoadLevel(RoomScene);
        }
        else
        {
            Debug.Log(
                PhotonNetwork.NickName +
                " left the room. Returning to Game Lobby."
            );

            PhotonNetwork.LoadLevel(LobbyScene);
        }
    }
}