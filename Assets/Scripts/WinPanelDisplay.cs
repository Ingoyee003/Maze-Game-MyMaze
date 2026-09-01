using UnityEngine;
using TMPro;
using System.Collections;

// Attach this to the WinPanel object. Shows the score/time once ScoreManager
// reports the player reached the exit, with a little punch-scale for flair.
public class WinPanelDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text resultText;

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnLevelWon += ShowResult;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnLevelWon -= ShowResult;
    }

    void ShowResult(int score, float time)
    {
        if (resultText == null) return;

        resultText.text = $"Score: {score}\nTime: {time:F1}s";
        StopCoroutine(nameof(PunchScale));
        StartCoroutine(PunchScale());
    }

    IEnumerator PunchScale()
    {
        Transform t = resultText.transform;
        Vector3 baseScale = Vector3.one;
        t.localScale = baseScale * 0.5f;

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            // Overshoot then settle - a classic "pop in" feel.
            float scale = Mathf.Sin(progress * Mathf.PI * 0.5f) * 1.15f;
            t.localScale = baseScale * scale;
            yield return null;
        }

        t.localScale = baseScale;
    }
}