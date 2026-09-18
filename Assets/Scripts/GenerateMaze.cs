using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GenerateMaze : MonoBehaviour
{
    public static GenerateMaze Instance { get; private set; }

    [SerializeField] GameObject roomPrefab;

    Room[,] rooms;

    public int NumX { get; private set; } = 10;
    public int NumY { get; private set; } = 10;

    [SerializeField] float roomWidth;
    float roomHeight;

    [Header("Maze visuals")]
    [SerializeField] Color wallColor = new Color(0.85f, 0.95f, 1f);     // cool light blue-white, catches the glow
    [SerializeField] Color floorColor = new Color(0.08f, 0.1f, 0.18f); // deeper navy, more contrast for the glow to read against

    public float RoomWidth => roomWidth;
    public float RoomHeight => roomHeight;

    Stack<Room> stack = new Stack<Room>();
    bool generating = false;

    // Subscribed to by MazeCornerSelector once a maze finishes building
    public event Action OnMazeGenerated;

    void Awake()
    {
        Instance = this;
    }

    void GetRoomSize()
    {
        SpriteRenderer[] renderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        foreach (var r in renderers)
        {
            min = Vector3.Min(min, r.bounds.min);
            max = Vector3.Max(max, r.bounds.max);
        }

        roomWidth = max.x - min.x;
        roomHeight = max.y - min.y;
    }

    // Call this from GameManager whenever a level is (re)loaded.
    public void Initialize(int width, int height)
    {
        if (generating) return;

        NumX = width;
        NumY = height;

        if (rooms != null)
        {
            foreach (var room in rooms)
                if (room != null) Destroy(room.gameObject);
        }

        GetRoomSize();
        BuildGrid();
        BuildBackgroundGlow();
        CreateMaze();
    }

    GameObject backgroundGlow;

    // A big soft radial glow behind the whole maze - dark navy floor with a
    // gentle lighter/tinted glow near the center, instead of a flat color.
    // This is the main thing that gives the "premium/atmospheric" feel.
    void BuildBackgroundGlow()
    {
        if (backgroundGlow != null) Destroy(backgroundGlow);

        backgroundGlow = new GameObject("BackgroundGlow");
        backgroundGlow.transform.SetParent(transform, false);

        float centerX = (NumX * roomWidth - roomWidth) / 2f;
        float centerY = (NumY * roomHeight - roomHeight) / 2f;
        backgroundGlow.transform.position = new Vector3(centerX, centerY, 0.1f); // slightly behind everything

        var sr = backgroundGlow.AddComponent<SpriteRenderer>();
        sr.sprite = RoundedUI.CreateGlowSprite(256, new Color(0.35f, 0.55f, 0.75f, 0.35f));
        sr.sortingOrder = -10;

        float span = Mathf.Max(NumX * roomWidth, NumY * roomHeight);
        backgroundGlow.transform.localScale = Vector3.one * (span * 1.4f / 2.56f); // glow sprite is 256px @ 100 PPU = 2.56 world units wide
    }

    void BuildGrid()
    {
        rooms = new Room[NumX, NumY];

        for (int i = 0; i < NumX; i++)
        {
            for (int j = 0; j < NumY; j++)
            {
                GameObject room = Instantiate(
                    roomPrefab,
                    new Vector3(i * roomWidth, j * roomHeight, 0f),
                    Quaternion.identity,
                    transform
                );

                room.name = $"Room_{i}_{j}";
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);

                // Visual polish: colored walls/floor instead of flat default grey.
                rooms[i, j].ApplyTheme(wallColor, floorColor);
            }
        }
    }

    public Room GetRoom(int x, int y)
    {
        if (x < 0 || x >= NumX || y < 0 || y >= NumY) return null;
        return rooms[x, y];
    }

    // The 4 corner rooms - used by MazeCornerSelector for start/exit picking.
    public List<Room> GetCorners()
    {
        return new List<Room>
        {
            rooms[0, 0],
            rooms[0, NumY - 1],
            rooms[NumX - 1, 0],
            rooms[NumX - 1, NumY - 1],
        };
    }

    void RemoveRoomWall(int x, int y, Room.Directions dir)
    {
        int newX = x, newY = y;

        switch (dir)
        {
            case Room.Directions.TOP:
                if (y >= NumY - 1) return;
                newY++;
                break;
            case Room.Directions.RIGHT:
                if (x >= NumX - 1) return;
                newX++;
                break;
            case Room.Directions.BOTTOM:
                if (y <= 0) return;
                newY--;
                break;
            case Room.Directions.LEFT:
                if (x <= 0) return;
                newX--;
                break;
            default:
                return;
        }

        rooms[x, y].SetDirFlag(dir, false);
        rooms[newX, newY].SetDirFlag(Room.Opposite(dir), false);
    }

    List<Tuple<Room.Directions, Room>> GetUnvisitedNeighbors(int cx, int cy)
    {
        var neighbors = new List<Tuple<Room.Directions, Room>>();

        void Check(Room.Directions dir, int nx, int ny)
        {
            if (nx < 0 || nx >= NumX || ny < 0 || ny >= NumY) return;
            if (!rooms[nx, ny].visited)
                neighbors.Add(new Tuple<Room.Directions, Room>(dir, rooms[nx, ny]));
        }

        Check(Room.Directions.TOP, cx, cy + 1);
        Check(Room.Directions.RIGHT, cx + 1, cy);
        Check(Room.Directions.BOTTOM, cx, cy - 1);
        Check(Room.Directions.LEFT, cx - 1, cy);

        return neighbors;
    }

    bool GenerateStep()
    {
        if (stack.Count == 0) return true;

        Room current = stack.Peek();
        var neighbors = GetUnvisitedNeighbors(current.Index.x, current.Index.y);

        if (neighbors.Count != 0)
        {
            var item = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
            Room next = item.Item2;
            next.visited = true;
            RemoveRoomWall(current.Index.x, current.Index.y, item.Item1);
            stack.Push(next);
        }
        else
        {
            stack.Pop();
        }

        return false;
    }

    // Optional: punch extra openings to create loops (a "braid" maze).
    // Call after generation on harder levels so there's more than one solution path.
    public void AddLoops(float extraConnectionChance)
    {
        for (int i = 0; i < NumX; i++)
        {
            for (int j = 0; j < NumY; j++)
            {
                if (UnityEngine.Random.value > extraConnectionChance) continue;

                var dirs = new[] { Room.Directions.TOP, Room.Directions.RIGHT };
                var dir = dirs[UnityEngine.Random.Range(0, dirs.Length)];
                RemoveRoomWall(i, j, dir);
            }
        }
    }

    void CreateMaze()
    {
        ResetWalls();

        stack.Clear();
        rooms[0, 0].visited = true;
        stack.Push(rooms[0, 0]);

        StartCoroutine(Coroutine_Generate());
    }

    IEnumerator Coroutine_Generate()
    {
        generating = true;

        // Instant generation - a visible per-cell delay isn't practical once
        // mazes get into the hundreds of cells. If you want a "building" animation
        // for polish later, do it as a purely visual reveal after this completes.
        bool done = false;
        while (!done)
            done = GenerateStep();

        yield return null;

        generating = false;
        EnsureCornersReachable();
        HideOuterBoundary();
        OnMazeGenerated?.Invoke();
    }

    // Cosmetic only - the player still can't walk past the edge (GetRoom
    // returns null beyond the grid), this just removes the "boxed in" look
    // so the maze feels open/endless instead of walled off on all sides.
    void HideOuterBoundary()
    {
        for (int i = 0; i < NumX; i++)
        {
            rooms[i, 0].HideWallVisual(Room.Directions.BOTTOM);
            rooms[i, NumY - 1].HideWallVisual(Room.Directions.TOP);

            // Both bottom-facing decorations on the bottom row, both
            // top-facing ones on the top row - otherwise these little
            // filler squares stay floating where the hidden boundary wall
            // used to be, which is what makes corners look sealed.
            rooms[i, 0].HideCornerDecoration(true, false);
            rooms[i, 0].HideCornerDecoration(false, false);
            rooms[i, NumY - 1].HideCornerDecoration(true, true);
            rooms[i, NumY - 1].HideCornerDecoration(false, true);
        }
        for (int j = 0; j < NumY; j++)
        {
            rooms[0, j].HideWallVisual(Room.Directions.LEFT);
            rooms[NumX - 1, j].HideWallVisual(Room.Directions.RIGHT);

            rooms[0, j].HideCornerDecoration(true, true);
            rooms[0, j].HideCornerDecoration(true, false);
            rooms[NumX - 1, j].HideCornerDecoration(false, true);
            rooms[NumX - 1, j].HideCornerDecoration(false, false);
        }
    }

    // Defensive: the generator always connects every cell, but a corner room
    // can still end up with its ONE opening facing an outer edge, which the
    // player can't actually walk through. This guarantees each of the four
    // corners has at least one opening pointing INTO the maze.
    void EnsureCornersReachable()
    {
        OpenCornerIfSealed(0, 0, Room.Directions.TOP, Room.Directions.RIGHT);
        OpenCornerIfSealed(NumX - 1, 0, Room.Directions.TOP, Room.Directions.LEFT);
        OpenCornerIfSealed(0, NumY - 1, Room.Directions.BOTTOM, Room.Directions.RIGHT);
        OpenCornerIfSealed(NumX - 1, NumY - 1, Room.Directions.BOTTOM, Room.Directions.LEFT);
    }

    void OpenCornerIfSealed(int x, int y, Room.Directions a, Room.Directions b)
    {
        Room room = rooms[x, y];
        if (!room.HasWall(a) || !room.HasWall(b)) return; // already open one way

        // Both interior sides are walled - knock one out at random.
        RemoveRoomWall(x, y, UnityEngine.Random.value < 0.5f ? a : b);
    }

    void ResetWalls()
    {
        for (int i = 0; i < NumX; i++)
        {
            for (int j = 0; j < NumY; j++)
            {
                rooms[i, j].visited = false;
                rooms[i, j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
            }
        }
    }
}