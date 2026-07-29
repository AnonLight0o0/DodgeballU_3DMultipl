using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Image connectionLogo;   // Assign your UI Image in the Inspector

    private Animator logoAnimator;

    void Start()
    {
        if (connectionLogo != null)
        {
            logoAnimator = connectionLogo.GetComponent<Animator>();

            if (logoAnimator != null)
            {
                logoAnimator.SetBool("isConnecting", true);
            }
            else
            {
                Debug.LogWarning("No Animator found on the Connection Logo.");
            }
        }
        else
        {
            Debug.LogWarning("Connection Logo Image has not been assigned.");
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (logoAnimator != null)
        {
            logoAnimator.SetBool("isConnecting", false);
        }

        SceneManager.LoadScene("GameLobby");
    }
}