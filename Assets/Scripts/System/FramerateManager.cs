using UnityEngine;

public class FramerateManager : MonoBehaviour
{
    public static FramerateManager Instance { get; private set; }

    [Header("Performance")]
    [Tooltip("-1 = Platform Default")]
    public int TargetFrameRate = 120;

    [Tooltip("0 = Off, 1 = Every VBlank, 2 = Every Second VBlank")]
    public int VSyncCount = 0;

    [Header("FPS Counter")]
    public bool ShowFPS = false;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ShowFPS = !ShowFPS;
        }

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        updateTimer += Time.unscaledDeltaTime;
        if (updateTimer >= updateInterval)
        {
            fps = 1.0f / deltaTime;
            updateTimer = 0f;
        }
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
}