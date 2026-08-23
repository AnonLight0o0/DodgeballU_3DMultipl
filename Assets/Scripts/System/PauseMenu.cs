using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("Performance")]
    [Tooltip("-1 = Platform Default")]
    public int TargetFrameRate = 120;

    [Tooltip("0 = Off, 1 = Every VBlank, 2 = Every Second VBlank")]
    public int VSyncCount = 0;

    [Header("FPS Counter")]
    public bool ShowFPS = false;

    [Header("Pause Menu")]
    public GameObject PauseMenuObj;
    public Button CloseMenuButton;
    public Button QuitGameButton;

    [Header("Lobby Button")]
    public Button ReturnToLobbyButton;
    public string WaitingRoomScene = "WaitingRoom";

    [Header("FPS Buttons")]
    public Button fps60;
    public Button fps90;
    public Button fps120;

    private bool isPaused = false;

    private float deltaTime = 0.0f;
    private float fps;
    private float updateTimer = 0f;
    private const float updateInterval = 0.25f;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private PlayerCamera playerCamera;
    private PlayerController playerController;
    private PlayerDodgeballInteraction dodgeballInteraction;

    private void Awake()
    {
        // Destroy duplicate instances
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Apply performance settings once
        QualitySettings.vSyncCount = VSyncCount;
        Application.targetFrameRate = TargetFrameRate;

        // Make sure pause menu starts hidden
        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(false);
        }

        // Connect FPS buttons
        if (fps60 != null)
        {
            fps60.onClick.AddListener(() => SetFrameRate(60));
        }

        if (fps90 != null)
        {
            fps90.onClick.AddListener(() => SetFrameRate(90));
        }

        if (fps120 != null)
        {
            fps120.onClick.AddListener(() => SetFrameRate(120));
        }

        UpdateFPSButtonVisuals();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowFPS = !ShowFPS;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        deltaTime +=
            (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateTimer += Time.unscaledDeltaTime;

        if (updateTimer >= updateInterval)
        {
            fps = 1.0f / deltaTime;
            updateTimer = 0f;
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateReturnToLobbyButton();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        // Remember cursor state before opening menu
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        // IMPORTANT:
        // Do NOT pause Time.timeScale.
        // The multiplayer game continues running normally.

        // Find the local player's components
        FindLocalPlayerComponents();

        // Disable only this player's controls
        DisablePlayerControls();

        // Show pause menu
        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(true);
        }

        // Unlock cursor for the pause menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        // Hide pause menu
        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(false);
        }

        // Re-enable only this player's controls
        EnablePlayerControls();

        // Restore cursor
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }

    public void ReturnToLobby()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        // Resume normal game time
        Time.timeScale = 1f;

        // Close the pause menu
        isPaused = false;

        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(false);
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Only Master Client changes the scene for everyone
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(WaitingRoomScene);
        }
        else
        {
            Debug.Log("Only the Master Client can return everyone to the Waiting Room.");
        }
    }

    private void UpdateReturnToLobbyButton()
    {
        if (ReturnToLobbyButton == null)
            return;

        bool isWaitingRoom =
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene().name == WaitingRoomScene;

        bool hasPlayers =
            PhotonNetwork.IsConnected &&
            PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.PlayerCount > 0;

        bool isMasterClient =
            PhotonNetwork.IsConnected &&
            PhotonNetwork.IsMasterClient;

        ReturnToLobbyButton.gameObject.SetActive(
            !isWaitingRoom &&
            hasPlayers &&
            isMasterClient
        );
    }

    private void FindLocalPlayerComponents()
    {
        playerCamera = null;
        playerController = null;
        dodgeballInteraction = null;

        // Find all player objects
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PhotonView view =
                player.GetComponent<PhotonView>();

            if (view != null && view.IsMine)
            {
                playerController =
                    player.GetComponent<PlayerController>();

                dodgeballInteraction =
                    player.GetComponent<PlayerDodgeballInteraction>();

                break;
            }
        }

        // Find the camera belonging to the local player
        PlayerCamera[] cameras =
            FindObjectsOfType<PlayerCamera>();

        foreach (PlayerCamera camera in cameras)
        {
            if (camera.target != null &&
                playerController != null)
            {
                if (camera.target.gameObject ==
                    playerController.gameObject)
                {
                    playerCamera = camera;
                    break;
                }
            }
        }
    }

    private void DisablePlayerControls()
    {
        if (playerCamera != null)
        {
            playerCamera.ControlsEnabled = false;
        }

        if (playerController != null)
        {
            playerController.ControlsEnabled = false;
        }

        if (dodgeballInteraction != null)
        {
            dodgeballInteraction.ControlsEnabled = false;
        }
    }

    private void EnablePlayerControls()
    {
        if (playerCamera != null)
        {
            playerCamera.ControlsEnabled = true;
        }

        if (playerController != null)
        {
            playerController.ControlsEnabled = true;
        }

        if (dodgeballInteraction != null)
        {
            dodgeballInteraction.ControlsEnabled = true;
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetFrameRate(int frameRate)
    {
        TargetFrameRate = frameRate;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;

        UpdateFPSButtonVisuals();
    }

    private void UpdateFPSButtonVisuals()
    {
        SetButtonAlpha(
            fps60,
            TargetFrameRate == 60 ? 1f : 0f
        );

        SetButtonAlpha(
            fps90,
            TargetFrameRate == 90 ? 1f : 0f
        );

        SetButtonAlpha(
            fps120,
            TargetFrameRate == 120 ? 1f : 0f
        );
    }

    private void SetButtonAlpha(
        Button button,
        float alpha)
    {
        if (button == null)
            return;

        Image image =
            button.GetComponent<Image>();

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private void OnGUI()
    {
        if (!ShowFPS)
            return;

        GUIStyle style =
            new GUIStyle(GUI.skin.label);

        style.fontSize = 18;
        style.normal.textColor = Color.cyan;

        Rect rect =
            new Rect(
                10,
                Screen.height - 30,
                150,
                30
            );

        GUI.Label(
            rect,
            $"FPS: {Mathf.RoundToInt(fps)}",
            style
        );
    }

    private void OnDestroy()
    {
        // Just in case another system had changed it.
        Time.timeScale = 1f;
    }
}