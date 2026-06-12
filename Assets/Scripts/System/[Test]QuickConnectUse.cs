using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class QuickConnectUse : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("正在连接 Photon 服务器...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("连接主服务器成功！正在强行创建或加入测试房间...");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("TestRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("成功进入房间！你现在可以尽情丢躲避球了！");
    }
}