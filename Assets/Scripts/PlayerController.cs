using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float moveSpeed = 8f;

    Room currentRoom;
    bool moving = false;
    bool canMove = false;
    PlayerVisual visual;

    void Awake()
    {
        Instance = this;
        visual = GetComponent<PlayerVisual>(); // optional - fine if not added

        // Hidden visually until spawned, but the GameObject itself stays
        // ACTIVE the whole time - this is required so Awake() (and therefore
        // Instance) is always set. An inactive GameObject never runs Awake().
        if (visual != null) visual.SetVisible(false);
    }

    public void SpawnAt(Room room)
    {
        currentRoom = room;
        transform.position = room.transform.position;
        if (visual != null) visual.SetVisible(true);
        canMove = true;
    }

    void Update()
    {
        if (!canMove || moving) return;

        // Keyboard, for editor/desktop testing.
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveUp();
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveDown();
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveLeft();
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveRight();
    }

    // Hook these directly to on-screen D-pad button OnClick events for mobile.
    // Using buttons instead of swipe gestures avoids fighting with the
    // camera's own drag-to-pan / pinch-to-zoom gestures.
    public void MoveUp() => TryMove(Room.Directions.TOP);
    public void MoveDown() => TryMove(Room.Directions.BOTTOM);
    public void MoveLeft() => TryMove(Room.Directions.LEFT);
    public void MoveRight() => TryMove(Room.Directions.RIGHT);

    void TryMove(Room.Directions dir)
    {
        if (!canMove || moving || currentRoom == null) return;

        if (currentRoom.HasWall(dir))
        {
            // Blocked by a wall - give the player feedback instead of silence,
            // so it's obvious the tap registered but that way is closed.
            if (CameraController.Instance != null)
                CameraController.Instance.Shake();
            if (visual != null)
                visual.PlayBumpEffect();
            return;
        }

        Vector2Int next = currentRoom.Index + Room.Delta(dir);
        Room nextRoom = GenerateMaze.Instance.GetRoom(next.x, next.y);
        if (nextRoom == null) return;

        if (visual != null)
            visual.PlayMoveEffect(Room.Delta(dir)); // sparkle burst + flame trail direction

        StartCoroutine(MoveTo(nextRoom));
    }

    IEnumerator MoveTo(Room target)
    {
        moving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = target.transform.position;
        float duration = Mathf.Max(0.01f, Vector3.Distance(startPos, endPos) / moveSpeed);
        float t = 0f;

        Vector3 baseScale = Vector3.one;
        Vector3 moveDir = (endPos - startPos).normalized;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            transform.position = Vector3.Lerp(startPos, endPos, progress);

            // Squash-stretch: elongate along the movement axis mid-step, settle at the end.
            // Peaks at the midpoint (progress 0.5) and eases back to normal by the end.
            float stretch = Mathf.Sin(progress * Mathf.PI) * 0.18f;
            Vector3 stretchScale = baseScale;
            stretchScale.x += Mathf.Abs(moveDir.x) * stretch;
            stretchScale.y += Mathf.Abs(moveDir.y) * stretch;
            transform.localScale = stretchScale;

            yield return null;
        }

        transform.position = endPos;
        transform.localScale = baseScale;
        currentRoom = target;
        moving = false;

        ScoreManager.Instance.CheckWin(currentRoom);
    }
}