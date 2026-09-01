using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] int baseScore = 1000;
    [SerializeField] float scoreDropPerSecond = 5f;
    [SerializeField] int minScore = 50;

    float elapsed = 0f;
    bool running = false;
    Room exitRoom;

    // HUD subscribes to these to update its timer/score display live.
    public event Action<float> OnTimerTick;
    public event Action<int, float> OnLevelWon; // (score, timeTaken)

    void Awake() => Instance = this;

    public void SetExit(Room exit) => exitRoom = exit;

    // Called by MazeCornerSelector the moment the player taps a starting corner.
    public void StartTimer()
    {
        elapsed = 0f;
        running = true;
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        OnTimerTick?.Invoke(elapsed);
    }

    public void CheckWin(Room current)
    {
        if (!running || exitRoom == null) return;
        if (current.Index != exitRoom.Index) return;

        running = false;
        int score = Mathf.Max(minScore, baseScore - Mathf.RoundToInt(elapsed * scoreDropPerSecond));
        OnLevelWon?.Invoke(score, elapsed);
    }
}