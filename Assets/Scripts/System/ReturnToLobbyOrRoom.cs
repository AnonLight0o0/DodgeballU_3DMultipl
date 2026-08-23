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
    // Only the player who clicks this leaves.
    // Other players are completely unaffected.
    public void ReturnToLobby()
    {
        Debug.Log(
            PhotonNetwork.NickName +
            " chose to return to the Game Lobby."
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        // Only THIS client leaves the Photon room.
        PhotonNetwork.LeaveRoom();
    }

    // ==========================================
    // RETURN TO WAITING ROOM
    // ==========================================
    // Only the player who clicks this changes scene.
    // Other players are completely unaffected.
    public void ReturnToWaitingRoom()
    {
        Debug.Log(
            PhotonNetwork.NickName +
            " chose to return to the Waiting Room."
        );

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        // IMPORTANT:
        // Do NOT use PhotonNetwork.LoadLevel().
        // That would move everyone to the scene.
        SceneManager.LoadScene(RoomScene);
    }

    // ==========================================
    // AFTER LEAVING PHOTON ROOM
    // ==========================================
    // This only happens for the player who
    // clicked ReturnToLobby().
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