using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;


public class HostAndJoinRoom : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_HostRoom;
    public TMP_InputField input_JoinRoom;  
    public TMP_InputField input_Username; 

    private void Start()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            if (PlayerPrefs.GetString("Username") != "DefaultName")
            {
                input_Username.text = PlayerPrefs.GetString("Username");
            }
        }
    }

    public void CreateRoom()
    {
        SetUsername();

        PhotonNetwork.CreateRoom(input_HostRoom.text , new RoomOptions()
        {MaxPlayers = 4, IsOpen = true}, TypedLobby.Default, null);
    }

    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(input_JoinRoom.text))
        {
            return;
        }
        
        else
        {
            SetUsername();

            PhotonNetwork.JoinRoom(input_JoinRoom.text);
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("WaitingRoom");
        }
    }

    private void SetUsername()
    {
        if (!string.IsNullOrEmpty(input_Username.text))
        {
            PhotonNetwork.NickName = input_Username.text;
            PlayerPrefs.SetString("Username", input_Username.text);
        }
        else
        {
            PhotonNetwork.NickName = "DefaultName";
            PlayerPrefs.SetString("Username", "DefaultName");
        }
    }
}