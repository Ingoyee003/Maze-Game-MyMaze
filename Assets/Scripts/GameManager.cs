using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { MainMenu, LevelSelect, Playing, Paused, Won }
    public State CurrentState { get; private set; }

    [Tooltip("Drag the Canvas here - panels are found automatically by name, no need to wire each one")]
    [SerializeField] Transform canvasRoot;

    GameObject mainMenuPanel;
    GameObject levelSelectPanel;
    GameObject pausePanel;
    GameObject hudPanel;
    GameObject winPanel;
    GameObject settingsPanel;
    GameObject howToPlayPanel;

    int currentLevelIndex = -1;
    public int CurrentLevelIndex => currentLevelIndex;

    void Awake() => Instance = this;

    void Start()
    {
        // Resolved here (not Awake) so UIBuilder has already constructed
        // every panel by the time we go looking for them - Start() always
        // runs after every object's Awake() has finished.
        mainMenuPanel = Find("MainMenuPanel");
        levelSelectPanel = Find("LevelSelectPanel");
        pausePanel = Find("PausePanel");
        hudPanel = Find("HUDPanel");
        winPanel = Find("WinPanel");
        settingsPanel = Find("SettingsPanel");
        howToPlayPanel = Find("HowToPlayPanel");

        ScoreManager.Instance.OnLevelWon += HandleLevelWon;
        Time.timeScale = 1f;
        ShowState(State.MainMenu);
    }

    GameObject Find(string name)
    {
        Transform t = FindDeep(canvasRoot, name);
        if (t == null)
        {
            Debug.LogError($"GameManager: couldn't find a panel named '{name}' under the Canvas. Did UIBuilder run?");
            return null;
        }
        return t.gameObject;
    }

    Transform FindDeep(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName) return child;
            Transform result = FindDeep(child, objectName);
            if (result != null) return result;
        }
        return null;
    }

    public void ShowState(State state)
    {
        CurrentState = state;

        mainMenuPanel.SetActive(state == State.MainMenu);
        levelSelectPanel.SetActive(state == State.LevelSelect);
        pausePanel.SetActive(state == State.Paused);
        hudPanel.SetActive(state == State.Playing || state == State.Paused);
        winPanel.SetActive(state == State.Won);

        settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // ---- Main Menu buttons ----

    public void OnPlayPressed()
    {
        StartLevel(LevelManager.Instance.HighestLevelReached);
    }

    public void OnOpenLevelSelectPressed() => ShowState(State.LevelSelect);
    public void OnCloseLevelSelectPressed() => ShowState(State.MainMenu);

    public void OnOpenHowToPlayPressed()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }
    public void OnCloseHowToPlayPressed()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void OnSettingsPressed()
    {
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
        hudPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsPressed()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(CurrentState == State.MainMenu);
        pausePanel.SetActive(CurrentState == State.Paused);
        hudPanel.SetActive(CurrentState == State.Playing || CurrentState == State.Paused);
    }

    public void OnQuitPressed()
    {
        // Application.Quit() does nothing while testing inside the Unity
        // Editor (only works in a real build) - this makes Exit actually
        // stop Play mode so it "works" during testing too.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnLevelSelected(int levelIndex)
    {
        if (!LevelManager.Instance.IsUnlocked(levelIndex)) return;
        StartLevel(levelIndex);
    }

    void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        var (width, height, loopChance) = LevelManager.Instance.GetLevelParams(levelIndex);

        ShowState(State.Playing);

        // Reset the player BEFORE generating the new maze - otherwise it
        // stays visible/movable from the previous level (stale position,
        // stale walls) until a fresh corner is picked.
        if (PlayerController.Instance != null)
            PlayerController.Instance.ResetForNewLevel();

        GenerateMaze.Instance.Initialize(width, height);
        CameraController.Instance.FitToMaze(
            width, height,
            GenerateMaze.Instance.RoomWidth, GenerateMaze.Instance.RoomHeight
        );

        if (loopChance > 0f)
            GenerateMaze.Instance.AddLoops(loopChance);
    }

    void HandleLevelWon(int score, float time)
    {
        LevelManager.Instance.ReportResult(currentLevelIndex, score);
        ShowState(State.Won);

        // Push the result straight into the Win panel - see the comment on
        // WinPanelDisplay.ShowResult for why this can't go through the
        // OnLevelWon event itself.
        var winDisplay = winPanel.GetComponent<WinPanelDisplay>();
        if (winDisplay != null) winDisplay.ShowResult(score, time);
    }

    public void OnNextLevelPressed() => StartLevel(currentLevelIndex + 1);
    public void OnRestartLevelPressed() => StartLevel(currentLevelIndex);

    // ---- Pause menu ----

    public void OnPausePressed()
    {
        if (CurrentState != State.Playing) return;
        ShowState(State.Paused);
        Time.timeScale = 0f;
    }

    public void OnResumePressed()
    {
        Time.timeScale = 1f;
        ShowState(State.Playing);
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        ShowState(State.MainMenu);
    }
}