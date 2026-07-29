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

    [Header("Loading UI")]
    public Image connectionLogo;

    [Header("Menu UI")]
    public GameObject backgroundImage;
    public GameObject menuContainer;

    private Animator logoAnimator;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            if (PlayerPrefs.GetString("Username") != "DefaultName")
            {
                input_Username.text = PlayerPrefs.GetString("Username");
            }
        }

        // Reset UI every time this scene loads
        if (backgroundImage != null)
            backgroundImage.SetActive(true);

        if (menuContainer != null)
            menuContainer.SetActive(true);

        if (connectionLogo != null)
        {
            connectionLogo.gameObject.SetActive(false);

            logoAnimator = connectionLogo.GetComponent<Animator>();

            if (logoAnimator == null)
            {
                Debug.LogWarning("No Animator found on the Connection Logo.");
            }
        }
    }

    public void CreateRoom()
    {
        SetUsername();

        ShowLoading();

        PhotonNetwork.CreateRoom(
            input_HostRoom.text,
            new RoomOptions()
            {
                MaxPlayers = 4,
                IsOpen = true
            },
            TypedLobby.Default,
            null
        );
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(input_JoinRoom.text))
        {
            return;
        }

        SetUsername();

        ShowLoading();

        PhotonNetwork.JoinRoom(input_JoinRoom.text);
    }

    private void ShowLoading()
    {
        if (backgroundImage != null)
            backgroundImage.SetActive(false);

        if (menuContainer != null)
            menuContainer.SetActive(false);

        if (connectionLogo != null)
        {
            connectionLogo.gameObject.SetActive(true);

            if (logoAnimator != null)
            {
                logoAnimator.SetBool("isConnecting", true);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        if (logoAnimator != null)
        {
            logoAnimator.SetBool("isConnecting", false);
        }

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