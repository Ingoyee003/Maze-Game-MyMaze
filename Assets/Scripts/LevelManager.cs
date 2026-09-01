using UnityEngine;

// Endless mode: no need to hand-author a LevelData asset for every level.
// Maze size and difficulty are computed from a formula based on the level
// index (0-based), so the game can keep going forever, getting harder.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Difficulty curve")]
    [Tooltip("Maze width/height at Level 1")]
    [SerializeField] int startSize = 5;

    [Tooltip("Maze stops growing past this size (keeps performance sane)")]
    [SerializeField] int maxSize = 25;

    [Tooltip("Every N levels, maze grows by 1 in each dimension")]
    [SerializeField] int levelsPerSizeStep = 2;

    [Tooltip("Chance (0-1) of an extra loop/opening per cell, once maxed out")]
    [SerializeField] float loopChanceMax = 0.15f;

    [Tooltip("By this level index, loop chance has ramped up to loopChanceMax")]
    [SerializeField] int levelsToMaxLoopChance = 30;

    const string HighestLevelKey = "HighestLevelReached"; // 0-based, highest level beaten
    const string ScoreKeyPrefix = "BestScore_";

    void Awake() => Instance = this;

    // Highest level index the player has successfully finished (0 if none yet).
    public int HighestLevelReached => PlayerPrefs.GetInt(HighestLevelKey, 0);

    // Level 0 is always open; anything up to (and including) the highest
    // reached level is playable/replayable.
    public bool IsUnlocked(int index) => index <= HighestLevelReached;

    public int GetBestScore(int index) => PlayerPrefs.GetInt(ScoreKeyPrefix + index, 0);

    public void ReportResult(int index, int score)
    {
        int best = GetBestScore(index);
        if (score > best)
            PlayerPrefs.SetInt(ScoreKeyPrefix + index, score);

        if (index + 1 > HighestLevelReached)
            PlayerPrefs.SetInt(HighestLevelKey, index + 1);

        PlayerPrefs.Save();
    }

    // Computes maze width, height, and loop chance for ANY level index on demand.
    // This is what replaces the fixed LevelData list - call this instead of
    // pulling from a hand-made array.
    public (int width, int height, float loopChance) GetLevelParams(int index)
    {
        int growthSteps = index / levelsPerSizeStep;
        int size = Mathf.Min(maxSize, startSize + growthSteps);

        float loopT = Mathf.Clamp01((float)index / levelsToMaxLoopChance);
        float loopChance = Mathf.Lerp(0f, loopChanceMax, loopT);

        return (size, size, loopChance);
    }
}