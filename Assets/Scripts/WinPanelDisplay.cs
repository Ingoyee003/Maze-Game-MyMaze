using UnityEngine;
using TMPro;
using System.Collections;

// Attach this to the WinPanel object. Shows the score/time once ScoreManager
// reports the player reached the exit, with a little punch-scale for flair.
public class WinPanelDisplay : MonoBehaviour
{
    TMP_Text timeValueText;
    TMP_Text scoreValueText;

    // Wired by UIBuilder in code - no manual Inspector dragging needed.
    public void SetReferences(TMP_Text timeValue, TMP_Text scoreValue)
    {
        timeValueText = timeValue;
        scoreValueText = scoreValue;
    }

    // Called DIRECTLY by GameManager right after it activates this panel -
    // NOT via the OnLevelWon event. The event fires GameManager's own
    // handler first, which is what activates this panel; a listener added
    // here via OnEnable() would subscribe too late to catch that same
    // firing (it'd only work from the second win onward). Calling this
    // directly sidesteps that entirely.
    public void ShowResult(int score, float time)
    {
        if (timeValueText != null) timeValueText.text = $"{time:F1}s";
        if (scoreValueText != null) scoreValueText.text = score.ToString();

        StopCoroutine(nameof(PunchScale));
        StartCoroutine(PunchScale());
    }

    IEnumerator PunchScale()
    {
        Transform t = transform;
        Vector3 baseScale = Vector3.one;
        t.localScale = baseScale * 0.5f;

        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            float scale = Mathf.Sin(progress * Mathf.PI * 0.5f) * 1.05f;
            t.localScale = baseScale * scale;
            yield return null;
        }

        t.localScale = baseScale;
    }
}