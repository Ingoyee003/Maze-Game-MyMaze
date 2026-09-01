using UnityEngine;
using System.Collections.Generic;
using TMPro;

// Waits for a maze to finish generating, highlights the 4 corners as
// selectable, and on tap: spawns the player there, picks the farthest
// corner as the exit (via BFS through the maze's open walls), and starts
// the timer. This is what makes "player picks any corner" work correctly
// regardless of which corner they choose.
public class MazeCornerSelector : MonoBehaviour
{
    [Tooltip("Optional - a HUD text object shown while the player is choosing a starting corner")]
    [SerializeField] TMP_Text hintText;

    List<Room> corners = new List<Room>();
    bool selectionActive = false;

    // IMPORTANT: subscribing in Start() instead of OnEnable(). Unity does
    // NOT guarantee that GenerateMaze's Awake() has already run by the time
    // this object's OnEnable() runs - so GenerateMaze.Instance could still
    // be null there, silently skipping the subscription forever. Start()
    // is guaranteed to run only after every object's Awake() has finished,
    // so GenerateMaze.Instance is always ready by then.
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
        corners = GenerateMaze.Instance.GetCorners();
        selectionActive = true;

        if (hintText != null)
        {
            hintText.text = "Tap a glowing corner to start!";
            hintText.gameObject.SetActive(true);
        }

        foreach (var c in corners)
        {
            c.ShowAsCornerOption(true);
            c.OnClicked += OnCornerClicked;
        }
    }

    void OnCornerClicked(Room chosen)
    {
        if (!selectionActive || !corners.Contains(chosen)) return;
        selectionActive = false;

        foreach (var c in corners)
        {
            c.ShowAsCornerOption(false);
            c.OnClicked -= OnCornerClicked;
        }

        Room exit = PickFarthestCorner(chosen);
        exit.ShowAsExit(true);

        // Timer + hint update FIRST, so even if something goes wrong spawning
        // the player, you still see the exit marked and the timer running -
        // makes bugs visible/debuggable instead of silently freezing the UI.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetExit(exit);
            ScoreManager.Instance.StartTimer();
        }
        else
        {
            Debug.LogError("MazeCornerSelector: ScoreManager.Instance is null - is the ScoreManager component in the scene and enabled?");
        }

        if (hintText != null)
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

    Room PickFarthestCorner(Room start)
    {
        Room best = null;
        int bestDist = -1;

        foreach (var corner in corners)
        {
            if (corner == start) continue;
            int dist = BFSDistance(start, corner);
            if (dist > bestDist)
            {
                bestDist = dist;
                best = corner;
            }
        }

        return best;
    }

    int BFSDistance(Room from, Room to)
    {
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<(Room room, int dist)>();
        queue.Enqueue((from, 0));
        visited.Add(from.Index);

        while (queue.Count > 0)
        {
            var (room, dist) = queue.Dequeue();
            if (room.Index == to.Index) return dist;

            foreach (Room.Directions dir in System.Enum.GetValues(typeof(Room.Directions)))
            {
                if (dir == Room.Directions.NONE) continue;
                if (room.HasWall(dir)) continue;

                Vector2Int next = room.Index + Room.Delta(dir);
                Room nextRoom = GenerateMaze.Instance.GetRoom(next.x, next.y);
                if (nextRoom == null || visited.Contains(next)) continue;

                visited.Add(next);
                queue.Enqueue((nextRoom, dist + 1));
            }
        }

        return 0;
    }
}