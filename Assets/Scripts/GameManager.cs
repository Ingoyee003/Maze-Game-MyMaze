using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { MainMenu, Playing, Paused, Won }
    public State CurrentState { get; private set; }

    [Header("Panels - assign in Inspector")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject levelSelectPanel; // now used as the PAUSE panel - keep this field name so your existing Inspector wiring doesn't break
    [SerializeField] GameObject hudPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject settingsPanel;

    int currentLevelIndex = -1;
    public int CurrentLevelIndex => currentLevelIndex;

    void Awake() => Instance = this;

    void Start()
    {
        ScoreManager.Instance.OnLevelWon += HandleLevelWon;
        Time.timeScale = 1f;
        ShowState(State.MainMenu);
    }

    public void ShowState(State state)
    {
        CurrentState = state;

        mainMenuPanel.SetActive(state == State.MainMenu);
        levelSelectPanel.SetActive(state == State.Paused); // pause panel
        hudPanel.SetActive(state == State.Playing || state == State.Paused);
        winPanel.SetActive(state == State.Won);

        // Settings is ALWAYS force-closed on every state change. It only
        // opens when OnSettingsPressed() is explicitly called. This is the
        // fix for it getting "stuck" open.
        settingsPanel.SetActive(false);
    }

    // ---- Hook these to UI Button OnClick events ----

    // Play now goes straight into the maze - no separate level-select screen.
    public void OnPlayPressed()
    {
        StartLevel(LevelManager.Instance.HighestLevelReached);
    }

    public void OnSettingsPressed() => settingsPanel.SetActive(true);
    public void OnCloseSettingsPressed() => settingsPanel.SetActive(false);

    public void OnQuitPressed() => Application.Quit();

    void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        var (width, height, loopChance) = LevelManager.Instance.GetLevelParams(levelIndex);

        ShowState(State.Playing);

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
    }

    // Endless mode: there's no final level, so this always advances.
    public void OnNextLevelPressed()
    {
        StartLevel(currentLevelIndex + 1);
    }

    // ---- Pause menu (mid-game) - wire these to the Pause panel's buttons ----

    // Wire to a new Pause button on the HUD.
    public void OnPausePressed()
    {
        if (CurrentState != State.Playing) return;
        ShowState(State.Paused);
        Time.timeScale = 0f; // freezes movement AND the timer while paused
    }

    // Wire the pause panel's "Resume"/"Continue" button to this.
    public void OnResumePressed()
    {
        Time.timeScale = 1f;
        ShowState(State.Playing);
    }

    // Wire the pause panel's "Quit"/"Back" button to this.
    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        ShowState(State.MainMenu);
    }

    public void OnBackToMainMenu()
    {
        Time.timeScale = 1f;
        ShowState(State.MainMenu);
    }
}