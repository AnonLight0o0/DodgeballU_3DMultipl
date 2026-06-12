using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomButtonEnabler : MonoBehaviourPunCallbacks
{
    public GameObject ReturnToRoomButton;
    public GameObject ReturnToLobbyButton;

    private void Start()
    {
        CheckButtonUpdate();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        CheckButtonUpdate();
    }

    private void CheckButtonUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ReturnToRoomButton.SetActive(true);
            ReturnToLobbyButton.SetActive(true);
        }
        else
        {
            ReturnToRoomButton.SetActive(false);
            ReturnToLobbyButton.SetActive(false);
        }
    }
}
