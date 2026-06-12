using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HostStartButton : MonoBehaviour
{
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("DodgeballMap1");
        }
    }
}
