using UnityEngine;
using UnityEngine.UI;

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

    [Header("FPS Buttons")]
    public Button fps60;
    public Button fps90;
    public Button fps120;

    private bool isPaused = false;

    private float deltaTime = 0.0f;
    private float fps;
    private float updateTimer = 0f;
    private const float updateInterval = 0.25f;

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

        // Connect buttons
        if (CloseMenuButton != null)
        {
            CloseMenuButton.onClick.AddListener(ResumeGame);
        }

        if (QuitGameButton != null)
        {
            QuitGameButton.onClick.AddListener(QuitGame);
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

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateTimer += Time.unscaledDeltaTime;

        if (updateTimer >= updateInterval)
        {
            fps = 1.0f / deltaTime;
            updateTimer = 0f;
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(true);
        }

        // Only unlock cursor when opening the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        if (PauseMenuObj != null)
        {
            PauseMenuObj.SetActive(false);
        }

        // Do NOT change cursor state here.
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
        SetButtonAlpha(fps60, TargetFrameRate == 60 ? 1f : 0f);
        SetButtonAlpha(fps90, TargetFrameRate == 90 ? 1f : 0f);
        SetButtonAlpha(fps120, TargetFrameRate == 120 ? 1f : 0f);
    }

    private void SetButtonAlpha(Button button, float alpha)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGUI()
    {
        if (!ShowFPS)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.normal.textColor = Color.cyan;

        Rect rect = new Rect(10, Screen.height - 30, 150, 30);

        GUI.Label(rect, $"FPS: {Mathf.RoundToInt(fps)}", style);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}