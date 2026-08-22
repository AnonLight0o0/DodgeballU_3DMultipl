using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class HostAndJoinRoom : MonoBehaviourPunCallbacks
{
    [Header("Room / Mode")]
    public TMP_InputField input_RoomNumber;
    public TMP_InputField input_Username;

    public Button modeButton;
    public Button confirmButton;

    [Header("Button Text")]
    public TMP_Text modeButtonText;
    public TMP_Text confirmButtonText;

    [Header("Mode Animation")]
    public Animator modeAnimator;

    [Tooltip("Delay before the button text changes.")]
    public float textChangeDelay = 0.14f;

    [Tooltip("Total time the mode button remains disabled.")]
    public float modeButtonLockTime = 0.4f;

    [Header("Loading UI")]
    public Image connectionLogo;

    [Header("Error UI")]
    public GameObject errorImage;
    public TMP_Text errorText;
    public float errorDisplayTime = 4f;

    private Coroutine errorCoroutine;

    [Header("Menu UI")]
    public GameObject backgroundImage;
    public GameObject menuContainer;

    private bool isHostMode = false;
    private bool isSwitchingMode = false;

    private Animator logoAnimator;

    private void Start()
    {
        // Load saved username
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

        if (errorImage != null)
        {
            errorImage.SetActive(false);
        }

        if (connectionLogo != null)
        {
            connectionLogo.gameObject.SetActive(false);

            logoAnimator = connectionLogo.GetComponent<Animator>();

            if (logoAnimator == null)
            {
                Debug.LogWarning("No Animator found on the Connection Logo.");
            }
        }

        // Connect buttons
        if (modeButton != null)
        {
            modeButton.onClick.AddListener(SwitchHostJoinMode);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(StartRoomAction);
        }

        // Set initial button text
        UpdateButtonText();
    }

    // =========================================================
    // MODE SWITCHING
    // =========================================================

    public void SwitchHostJoinMode()
    {
        if (isSwitchingMode)
            return;

        StartCoroutine(SwitchModeCoroutine());
    }

    private IEnumerator SwitchModeCoroutine()
    {
        isSwitchingMode = true;

        // Disable mode button during animation
        if (modeButton != null)
            modeButton.interactable = false;

        // Change actual mode immediately
        isHostMode = !isHostMode;

        // Trigger animation
        if (modeAnimator != null)
        {
            modeAnimator.SetTrigger("FlipHostJoinMode");
        }

        // Wait before changing text
        yield return new WaitForSecondsRealtime(textChangeDelay);

        UpdateButtonText();

        // Wait remaining lock time
        float remainingTime = modeButtonLockTime - textChangeDelay;

        if (remainingTime > 0f)
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }

        // Re-enable mode button
        if (modeButton != null)
            modeButton.interactable = true;

        isSwitchingMode = false;

        Debug.Log(
            isHostMode
                ? "Switched to Host mode."
                : "Switched to Join mode."
        );
    }

    private void UpdateButtonText()
    {
        if (isHostMode)
        {
            if (modeButtonText != null)
                modeButtonText.text = "Side B: Host";

            if (confirmButtonText != null)
                confirmButtonText.text = "Host";
        }
        else
        {
            if (modeButtonText != null)
                modeButtonText.text = "Side A: Join";

            if (confirmButtonText != null)
                confirmButtonText.text = "Join";
        }
    }

    // =========================================================
    // HOST / JOIN
    // =========================================================

    public void StartRoomAction()
    {
        if (input_RoomNumber == null)
        {
            Debug.LogError("Room number input field is NOT assigned.");
            return;
        }

        if (string.IsNullOrEmpty(input_RoomNumber.text))
        {
            Debug.Log("Please enter a room number.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning(
                "Photon is not ready yet. Current state: " +
                PhotonNetwork.NetworkClientState
            );

            return;
        }

        SetUsername();

        if (isHostMode)
        {
            CreateRoom();
        }
        else
        {
            JoinRoom();
        }
    }

    private void CreateRoom()
    {
        LogPhotonConnectionInfo();

        ShowLoading();

        Debug.Log(
            "Attempting to CREATE room: " +
            input_RoomNumber.text
        );

        PhotonNetwork.CreateRoom(
            input_RoomNumber.text,
            new RoomOptions()
            {
                MaxPlayers = 4,
                IsOpen = true
            },
            TypedLobby.Default,
            null
        );
    }

    private void JoinRoom()
    {
        LogPhotonConnectionInfo();

        ShowLoading();

        Debug.Log(
            "Attempting to JOIN room: " +
            input_RoomNumber.text
        );

        PhotonNetwork.JoinRoom(input_RoomNumber.text);
    }

    // =========================================================
    // PHOTON CONNECTION DIAGNOSTICS
    // =========================================================

    private void LogPhotonConnectionInfo()
    {
        if (PhotonNetwork.PhotonServerSettings == null)
        {
            Debug.LogError("PhotonServerSettings is NULL.");
            return;
        }

        AppSettings appSettings =
            PhotonNetwork.PhotonServerSettings.AppSettings;

        Debug.Log(
            "========== PHOTON CONNECTION INFO ==========\n" +
            "Connected: " + PhotonNetwork.IsConnected + "\n" +
            "Ready: " + PhotonNetwork.IsConnectedAndReady + "\n" +
            "Server: " + PhotonNetwork.Server + "\n" +
            "State: " + PhotonNetwork.NetworkClientState + "\n" +
            "Region: " + PhotonNetwork.CloudRegion + "\n" +
            "App Version: " + PhotonNetwork.AppVersion + "\n" +
            "App ID: " + appSettings.AppIdRealtime + "\n" +
            "Room: " +
                (PhotonNetwork.CurrentRoom != null
                    ? PhotonNetwork.CurrentRoom.Name
                    : "None") +
            "\n" +
            "============================================"
        );
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(
            "========== CONNECTED TO PHOTON MASTER ==========\n" +
            "Region: " + PhotonNetwork.CloudRegion + "\n" +
            "App Version: " + PhotonNetwork.AppVersion + "\n" +
            "Server: " + PhotonNetwork.Server +
            "\n==============================================="
        );
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(
            "Successfully joined room: " +
            PhotonNetwork.CurrentRoom.Name
        );

        if (logoAnimator != null)
        {
            logoAnimator.SetBool("isConnecting", false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("WaitingRoom");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError(
            "========== JOIN ROOM FAILED ==========\n" +
            "Code: " + returnCode + "\n" +
            "Message: " + message + "\n" +
            "Region: " + PhotonNetwork.CloudRegion + "\n" +
            "App Version: " + PhotonNetwork.AppVersion +
            "\n======================================="
        );

        // Return the normal menu
        HideLoading();

        // Show the room-not-found message
        ShowRoomError(
            "No room found with ID " + input_RoomNumber.text
        );
    }

    private void ShowRoomError(string message)
    {
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
        }

        errorCoroutine = StartCoroutine(RoomErrorCoroutine(message));
    }

    private IEnumerator RoomErrorCoroutine(string message)
    {
        if (errorImage != null)
        {
            errorImage.SetActive(true);
        }

        if (errorText != null)
        {
            errorText.text = message;
        }

        yield return new WaitForSecondsRealtime(errorDisplayTime);

        if (errorImage != null)
        {
            errorImage.SetActive(false);
        }

        errorCoroutine = null;
    }

    private void HideLoading()
    {
        if (backgroundImage != null)
            backgroundImage.SetActive(true);

        if (menuContainer != null)
            menuContainer.SetActive(true);

        if (connectionLogo != null)
        {
            connectionLogo.gameObject.SetActive(false);

            if (logoAnimator != null)
            {
                logoAnimator.SetBool("isConnecting", false);
            }
        }
    }

    // =========================================================
    // LOADING UI
    // =========================================================

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

    // =========================================================
    // USERNAME
    // =========================================================

    private void SetUsername()
    {
        if (input_Username != null &&
            !string.IsNullOrEmpty(input_Username.text))
        {
            PhotonNetwork.NickName = input_Username.text;

            PlayerPrefs.SetString(
                "Username",
                input_Username.text
            );
        }
        else
        {
            PhotonNetwork.NickName = "DefaultName";

            PlayerPrefs.SetString(
                "Username",
                "DefaultName"
            );
        }

        PlayerPrefs.Save();
    }
}

/*using System.Collections;
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
}*/