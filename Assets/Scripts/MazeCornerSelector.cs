using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

// Picks the EXIT corner FIRST (before the player chooses anything) and
// marks it immediately - it never changes afterward. The other 3 corners
// are shown as selectable starting points; the exit corner is excluded
// from that list entirely, so it can never accidentally be picked as start.
public class MazeCornerSelector : MonoBehaviour
{
    [Tooltip("Optional - a HUD text object shown while the player is choosing a starting corner")]
    [SerializeField] TMP_Text hintText;

    public void SetHintText(TMP_Text text) => hintText = text;

    List<Room> startOptions = new List<Room>();
    Room exitRoom;
    bool selectionActive = false;

    void Start()
    {
        if (GenerateMaze.Instance != null)
            GenerateMaze.Instance.OnMazeGenerated += BeginSelection;
        else
            Debug.LogError("MazeCornerSelector: GenerateMaze.Instance is still null in Start(). Check that a GenerateMaze component exists and is enabled in the scene.");
    }

    void OnDisable()
    {
        if (GenerateMaze.Instance != null)
            GenerateMaze.Instance.OnMazeGenerated -= BeginSelection;
    }

    void BeginSelection()
    {
        var allCorners = GenerateMaze.Instance.GetCorners();

        // Fix the exit FIRST and mark it immediately - it never gets
        // recomputed or reassigned after this point.
        exitRoom = allCorners[Random.Range(0, allCorners.Count)];
        exitRoom.ShowAsExit(true);

        // The remaining 3 corners are the only selectable starting points -
        // the exit corner is never offered, so it can't be picked as start.
        startOptions = allCorners.Where(c => c != exitRoom).ToList();
        selectionActive = true;

        bool hintsEnabled = PlayerPrefs.GetInt("TutorialHintsEnabled", 1) == 1;
        if (hintText != null && hintsEnabled)
        {
            hintText.text = "Tap a glowing corner to start!";
            hintText.gameObject.SetActive(true);
        }

        foreach (var c in startOptions)
        {
            c.ShowAsCornerOption(true);
            c.OnClicked += OnCornerClicked;
        }
    }

    void OnCornerClicked(Room chosen)
    {
        if (!selectionActive || !startOptions.Contains(chosen)) return;
        selectionActive = false;

        foreach (var c in startOptions)
        {
            c.ShowAsCornerOption(false);
            c.OnClicked -= OnCornerClicked;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetExit(exitRoom);
            ScoreManager.Instance.StartTimer();
        }
        else
        {
            Debug.LogError("MazeCornerSelector: ScoreManager.Instance is null - is the ScoreManager component in the scene and enabled?");
        }

        bool hintsEnabled = PlayerPrefs.GetInt("TutorialHintsEnabled", 1) == 1;
        if (hintText != null && hintsEnabled)
        {
            hintText.text = "Find the flagged exit - go!";
            StartCoroutine(HideHintAfterDelay(2f));
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SpawnAt(chosen);
        }
        else
        {
            Debug.LogError("MazeCornerSelector: PlayerController.Instance is null - is the Player object's PlayerController component enabled?");
        }
    }

    System.Collections.IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hintText != null) hintText.gameObject.SetActive(false);
    }
}