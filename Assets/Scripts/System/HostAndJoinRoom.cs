using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

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

    [Header("Connection")]
    [Tooltip("Maximum time to wait before restoring the menu.")]
    public float connectionTimeout = 4f;

    [Header("Loading UI")]
    public Image connectionLogo;

    [Header("Room Not Found UI")]
    public GameObject roomNotFoundImage;
    public TMP_Text roomNotFoundText;
    public float roomNotFoundDisplayTime = 3f;

    private Coroutine roomNotFoundCoroutine;


    [Header("Menu UI")]
    public GameObject backgroundImage;
    public GameObject menuContainer;

    private bool isHostMode = false;
    private bool isSwitchingMode = false;
    private bool isConnecting = false;

    private Animator logoAnimator;

    // Used when Photon needs to reconnect before performing the action.
    private bool pendingRoomAction = false;

    private void Start()
    {
        // Load saved username
        if (PlayerPrefs.HasKey("Username"))
        {
            string savedUsername = PlayerPrefs.GetString("Username");

            if (savedUsername != "DefaultName")
            {
                input_Username.text = savedUsername;
            }
        }

        // Reset UI
        if (backgroundImage != null)
            backgroundImage.SetActive(true);

        if (menuContainer != null)
            menuContainer.SetActive(true);

        if (roomNotFoundImage != null)
        {
            roomNotFoundImage.SetActive(false);
        }

        // Connection logo
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

        // Initial text
        UpdateButtonText();
    }

    private void Update()
    {
        if (roomNotFoundImage != null && roomNotFoundImage.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HideRoomNotFoundMessage();
            }
        }
    }

    // =========================================================
    // HOST / JOIN MODE
    // =========================================================

    public void SwitchHostJoinMode()
    {
        if (isSwitchingMode || isConnecting)
            return;

        StartCoroutine(SwitchModeCoroutine());
    }

    private IEnumerator SwitchModeCoroutine()
    {
        isSwitchingMode = true;

        if (modeButton != null)
            modeButton.interactable = false;

        // Change mode immediately
        isHostMode = !isHostMode;

        // Play flip animation
        if (modeAnimator != null)
        {
            modeAnimator.SetTrigger("FlipHostJoinMode");
        }

        // Delay text change
        yield return new WaitForSecondsRealtime(textChangeDelay);

        UpdateButtonText();

        // Wait until total lock time has passed
        float remainingTime = modeButtonLockTime - textChangeDelay;

        if (remainingTime > 0f)
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }

        if (modeButton != null)
            modeButton.interactable = true;

        isSwitchingMode = false;

        Debug.Log(isHostMode ? "Switched to Host mode." : "Switched to Join mode.");
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
    // HOST / JOIN ACTION
    // =========================================================

    public void StartRoomAction()
    {
        if (isConnecting || isSwitchingMode)
            return;

        if (roomNotFoundImage != null)
        {
            roomNotFoundImage.SetActive(false);
        }

        if (input_RoomNumber == null || string.IsNullOrWhiteSpace(input_RoomNumber.text))
        {
            Debug.LogWarning("Please enter a room number.");
            return;
        }

        SetUsername();

        // Photon isn't currently ready.
        // Reconnect first instead of calling CreateRoom/JoinRoom
        // while Photon is disconnected.
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon is not ready. Reconnecting...");

            pendingRoomAction = true;

            DisableMenu();
            StartConnectionTimeout();

            PhotonNetwork.ConnectUsingSettings();

            return;
        }

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
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Cannot create room because Photon is not ready.");
            RestoreMenu();
            return;
        }

        ShowLoading();

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

        StartConnectionTimeout();
    }

    private void JoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Cannot join room because Photon is not ready.");
            RestoreMenu();
            return;
        }

        ShowLoading();

        PhotonNetwork.JoinRoom(input_RoomNumber.text);

        StartConnectionTimeout();
    }

    // =========================================================
    // PHOTON CALLBACKS
    // =========================================================

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");

        if (pendingRoomAction)
        {
            pendingRoomAction = false;

            CancelConnectionTimeout();

            if (isHostMode)
            {
                CreateRoom();
            }
            else
            {
                JoinRoom();
            }
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined room.");

        CancelConnectionTimeout();

        isConnecting = false;
        pendingRoomAction = false;

        if (logoAnimator != null)
        {
            logoAnimator.SetBool("isConnecting", false);
        }

        // Both Host and Join players should enter WaitingRoom.
        PhotonNetwork.LoadLevel("WaitingRoom");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Failed to join room: " + message);

        CancelConnectionTimeout();
        RestoreMenu();

        ShowRoomNotFoundMessage();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning(
            "Failed to create room. Return Code: "
            + returnCode
            + " | Message: "
            + message
        );

        CancelConnectionTimeout();
        RestoreMenu();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning(
            "Photon disconnected. Cause: "
            + cause
        );

        CancelConnectionTimeout();
        RestoreMenu();
    }

    // =========================================================
    // LOADING / MENU
    // =========================================================

    private void ShowLoading()
    {
        isConnecting = true;

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

        if (modeButton != null)
            modeButton.interactable = false;

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private void DisableMenu()
    {
        isConnecting = true;

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

        if (modeButton != null)
            modeButton.interactable = false;

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private void RestoreMenu()
    {
        isConnecting = false;
        pendingRoomAction = false;

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

        if (modeButton != null)
            modeButton.interactable = true;

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    // =========================================================
    // CONNECTION TIMEOUT
    // =========================================================

    private Coroutine connectionTimeoutCoroutine;

    private void StartConnectionTimeout()
    {
        CancelConnectionTimeout();

        connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeoutCoroutine());
    }

    private void CancelConnectionTimeout()
    {
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
    }

    private IEnumerator ConnectionTimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(connectionTimeout);

        connectionTimeoutCoroutine = null;

        if (isConnecting)
        {
            Debug.LogWarning(
                "Connection attempt timed out after "
                + connectionTimeout
                + " seconds."
            );

            RestoreMenu();

            // If Photon has become disconnected, reconnect so the
            // next Host/Join attempt can work normally.
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    private void ShowRoomNotFoundMessage()
    {
        if (roomNotFoundImage != null)
            roomNotFoundImage.SetActive(true);

        if (roomNotFoundText != null)
            roomNotFoundText.text = "No room found with ID " + input_RoomNumber.text;

        if (roomNotFoundCoroutine != null)
            StopCoroutine(roomNotFoundCoroutine);

        roomNotFoundCoroutine = StartCoroutine(HideRoomNotFoundAfterDelay());
    }

    private IEnumerator HideRoomNotFoundAfterDelay()
    {
        yield return new WaitForSecondsRealtime(roomNotFoundDisplayTime);

        HideRoomNotFoundMessage();
    }

    private void HideRoomNotFoundMessage()
    {
        if (roomNotFoundCoroutine != null)
        {
            StopCoroutine(roomNotFoundCoroutine);
            roomNotFoundCoroutine = null;
        }

        if (roomNotFoundImage != null)
            roomNotFoundImage.SetActive(false);
    }

    // =========================================================
    // USERNAME
    // =========================================================

    private void SetUsername()
    {
        if (input_Username != null &&
            !string.IsNullOrWhiteSpace(input_Username.text))
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