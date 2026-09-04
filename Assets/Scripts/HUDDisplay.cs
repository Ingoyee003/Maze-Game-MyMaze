using UnityEngine;
using TMPro;

// Attach this to the HUDPanel object. It listens to ScoreManager's events
// and keeps the Timer/Score/Level text on screen updated automatically.
public class HUDDisplay : MonoBehaviour
{
    TMP_Text timerText;
    TMP_Text scoreText;
    TMP_Text levelText;

    // Wired by UIBuilder in code - no manual Inspector dragging needed.
    public void SetReferences(TMP_Text timer, TMP_Text score, TMP_Text level)
    {
        timerText = timer;
        scoreText = score;
        levelText = level;
    }

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnTimerTick += UpdateTimer;
        if (GenerateMaze.Instance != null)
            GenerateMaze.Instance.OnMazeGenerated += UpdateLevel;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnTimerTick -= UpdateTimer;
        if (GenerateMaze.Instance != null)
            GenerateMaze.Instance.OnMazeGenerated -= UpdateLevel;
    }

    void UpdateTimer(float elapsed)
    {
        if (timerText != null)
            timerText.text = $"Time: {elapsed:F1}";
    }

    void UpdateLevel()
    {
        if (levelText != null && GameManager.Instance != null)
            levelText.text = $"Level {GameManager.Instance.CurrentLevelIndex + 1}";
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}