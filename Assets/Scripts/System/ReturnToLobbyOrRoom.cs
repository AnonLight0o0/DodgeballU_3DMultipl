using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ReturnToLobbyOrRoom : MonoBehaviourPunCallbacks
{
    public string LobbyScene = "GameLobby";
    public string RoomScene = "WaitingRoom";
        public void ReturnToRoomScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Host returned to room.");
            PhotonNetwork.LoadLevel(RoomScene); 
        }
    }

    public void LeaveRoomAndReturnToLobby()
    {
        Debug.Log("Leaving Room and disbanding team...");
        PhotonNetwork.LeaveRoom(); 
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Returning to Lobby...");
        SceneManager.LoadScene(LobbyScene);
    }
}