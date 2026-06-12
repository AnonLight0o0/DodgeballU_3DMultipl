using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class HostButtonEnabler : MonoBehaviourPunCallbacks
{
    public GameObject startGameButton;
    public GameObject waitingForHostText;

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
            startGameButton.SetActive(true);
            waitingForHostText.SetActive(false);
        }
        else
        {
            startGameButton.SetActive(false);
            waitingForHostText.SetActive(true);
        }
    }
}
