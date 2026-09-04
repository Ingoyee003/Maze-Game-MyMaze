using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Builds the level-select grid entirely in code - no prefab authoring
// needed. Shows every level from 1 up to a bit past the player's furthest
// unlocked level (never all 500 at once - keeps it fast), and rebuilds
// itself every time the panel opens so it always reflects current progress.
public class LevelSelectUI : MonoBehaviour
{
    [Tooltip("The 'Content' object inside your Scroll View - needs a Grid Layout Group component")]
    [SerializeField] Transform gridContent;

    [Tooltip("How many locked levels to preview beyond the furthest one reached")]
    [SerializeField] int showAheadOfUnlocked = 15;

    public void SetGridContent(Transform content) => gridContent = content;

    void OnEnable()
    {
        Populate();
    }

    void Populate()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelSelectUI: LevelManager.Instance is null - make sure LevelSelectPanel starts INACTIVE (unchecked) in the Hierarchy so this only runs after the game has fully loaded.");
            return;
        }

        // Rebuilt fresh each time this panel opens, so it's never stale.
        foreach (Transform child in gridContent)
            Destroy(child.gameObject);

        int highest = LevelManager.Instance.HighestLevelReached;
        int totalToShow = highest + showAheadOfUnlocked + 1;

        for (int i = 0; i < totalToShow; i++)
            CreateLevelButton(i);
    }

    void CreateLevelButton(int levelIndex)
    {
        bool unlocked = LevelManager.Instance.IsUnlocked(levelIndex);

        GameObject btnObj = new GameObject($"Level_{levelIndex + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(gridContent, false);

        var img = btnObj.GetComponent<Image>();
        RoundedUI.ApplyRoundedStyle(img, 20);
        img.color = unlocked ? UITheme.Palette.CardWhite : UITheme.Palette.MutedGrey;

        var btn = btnObj.GetComponent<Button>();
        btn.interactable = unlocked;

        // Level number label (blank if locked - the lock icon takes its place)
        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = unlocked ? (levelIndex + 1).ToString() : "";
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = UITheme.Palette.TextDark;
        StretchFull(textObj.GetComponent<RectTransform>());

        if (!unlocked)
        {
            GameObject lockObj = new GameObject("Lock", typeof(RectTransform));
            lockObj.transform.SetParent(btnObj.transform, false);
            var lockTmp = lockObj.AddComponent<TextMeshProUGUI>();
            lockTmp.text = "LOCKED";
            lockTmp.fontSize = 16;
            lockTmp.alignment = TextAlignmentOptions.Center;
            lockTmp.color = UITheme.Palette.TextMuted;
            StretchFull(lockObj.GetComponent<RectTransform>());
        }

        btn.onClick.AddListener(() => GameManager.Instance.OnLevelSelected(levelIndex));
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}