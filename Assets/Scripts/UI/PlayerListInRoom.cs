using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListInRoom : MonoBehaviourPunCallbacks
{
    public GameObject PlayerInRoomPrefab;
    public Transform PlayerListContentContainer;
    private Dictionary<int, GameObject> PlayerListDictionary = new Dictionary<int, GameObject>();

    void Start()
    {
    Player[] PlayerAlreadyInRoom = PhotonNetwork.PlayerList;
            
        for (int i = 0; i < PlayerAlreadyInRoom.Length; i++)
        {
            print(PlayerAlreadyInRoom[i].NickName + "is in the room.");
            GameObject CurrentPlayerInRoom = Instantiate(PlayerInRoomPrefab, Vector3.zero, Quaternion.identity, PlayerListContentContainer);
            CurrentPlayerInRoom.GetComponent<PlayerInRoom>().PlayerName.text = PlayerAlreadyInRoom[i].NickName;
            PlayerListDictionary.Add(PlayerAlreadyInRoom[i].ActorNumber, CurrentPlayerInRoom);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        print(newPlayer.NickName + " has joined.");
        GameObject CurrentPlayerInRoom = Instantiate(PlayerInRoomPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Content").transform); 
        CurrentPlayerInRoom.GetComponent<PlayerInRoom>().PlayerName.text = newPlayer.NickName;
        PlayerListDictionary.Add(newPlayer.ActorNumber, CurrentPlayerInRoom); 
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print(otherPlayer.NickName + " has left the room.");
        if(PlayerListDictionary.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(PlayerListDictionary[otherPlayer.ActorNumber]);
            PlayerListDictionary.Remove(otherPlayer.ActorNumber);
        }
    }
}
