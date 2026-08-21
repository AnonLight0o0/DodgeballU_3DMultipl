using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HostStartButton : MonoBehaviour
{
    [SerializeField] private string scene1 = "DodgeballMap1";
    [SerializeField] private string scene2 = "DodgeballMap2";
    [SerializeField] private string scene3 = "DodgeballMap3";
    [SerializeField] private string scene4 = "DodgeballMap4";

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(scene1);
        }
    }
}
