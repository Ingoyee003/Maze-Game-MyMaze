using UnityEngine;
using TMPro;

// Attach this to the HUDPanel object. It listens to ScoreManager's events
// and keeps the Timer/Score text on screen updated automatically.
public class HUDDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text scoreText;

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnTimerTick += UpdateTimer;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnTimerTick -= UpdateTimer;
    }

    void UpdateTimer(float elapsed)
    {
        if (timerText != null)
            timerText.text = $"Time: {elapsed:F1}";
    }

    // Call this from GameManager/WinPanel when a level is won, or leave
    // ScoreText showing the last value - your call.
    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}