using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public enum Directions
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        NONE,
    }

    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;

    [Header("Optional visuals (can be left empty)")]
    [Tooltip("A highlighted sprite/button shown when this room is a selectable corner")]
    [SerializeField] GameObject cornerHighlight;
    [Tooltip("A flag/marker shown once this room is chosen as the exit")]
    [SerializeField] GameObject exitMarker;

    [Header("Decorative corner-fill pieces (optional - the diagonal pieces at wall intersections)")]
    [SerializeField] GameObject cornerTopLeft;
    [SerializeField] GameObject cornerTopRight;
    [SerializeField] GameObject cornerBottomLeft;
    [SerializeField] GameObject cornerBottomRight;

    [Header("Theming (optional - for visual polish)")]
    [Tooltip("The floor/background sprite of this room (e.g. the 'Square' child)")]
    [SerializeField] SpriteRenderer floorRenderer;

    Dictionary<Directions, GameObject> walls;
    Dictionary<Directions, bool> wallActive = new Dictionary<Directions, bool>();
    SpriteRenderer[] wallRenderers;
    Coroutine pulseRoutine;

    public Vector2Int Index { get; set; }
    public bool visited { get; set; } = false;

    // Fired when the player taps/clicks this room (used for corner selection)
    public event Action<Room> OnClicked;

    void Awake()
    {
        walls = new Dictionary<Directions, GameObject>
        {
            { Directions.TOP, topWall },
            { Directions.BOTTOM, bottomWall },
            { Directions.LEFT, leftWall },
            { Directions.RIGHT, rightWall },
        };

        wallRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    // Hides a wall's SPRITE ONLY, without changing whether it actually
    // blocks movement (that's controlled separately by SetDirFlag/HasWall).
    // Used to make the outer maze boundary look "open" for an endless feel,
    // while the player still can't walk past it (GenerateMaze.GetRoom
    // returns null beyond the grid edge regardless).
    public void HideWallVisual(Directions dir)
    {
        if (walls.TryGetValue(dir, out var wall) && wall != null)
            wall.SetActive(false);
    }

    // Hides one decorative diagonal filler piece. These sit where two walls
    // meet; when the adjacent walls are hidden (outer boundary) the piece is
    // left floating and makes the room look like a sealed box even though
    // it's open, so boundary rooms hide their outward-facing pieces.
    public void HideCornerDecoration(bool left, bool top)
    {
        GameObject piece =
            left && top ? cornerTopLeft :
            !left && top ? cornerTopRight :
            left ? cornerBottomLeft :
            cornerBottomRight;

        if (piece != null) piece.SetActive(false);
    }

    public void SetDirFlag(Directions dir, bool flag)
    {
        wallActive[dir] = flag;

        if (walls.TryGetValue(dir, out var wall) && wall != null)
            wall.SetActive(flag);
    }

    // True = wall present (blocked), false = open. Defaults to "walled" if unset.
    public bool HasWall(Directions dir)
    {
        return wallActive.TryGetValue(dir, out bool v) ? v : true;
    }

    // Colors the walls + floor. Call once after generation so the maze
    // doesn't look like flat default-grey boxes.
    public void ApplyTheme(Color wallColor, Color floorColor)
    {


        if (floorRenderer != null)
            floorRenderer.color = floorColor;

        foreach (var wall in walls.Values)
        {
            if (wall == null) continue;
            var sr = wall.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = wallColor;
        }
    }

    public void ShowAsCornerOption(bool show)
    {
        EnsureCornerHighlight();
        cornerHighlight.SetActive(show);

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        if (show) pulseRoutine = StartCoroutine(PulseHighlight());
    }

    // If nobody wired a cornerHighlight in the prefab, build a simple glowing
    // dot via code so this always works regardless of manual Editor setup.
    void EnsureCornerHighlight()
    {
        if (cornerHighlight != null) return;

        Color yellow = new Color(1f, 0.85f, 0.2f, 0.9f);

        GameObject glow = new GameObject("CornerGlow_Auto");
        glow.transform.SetParent(transform, false);
        var glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = RoundedUI.CreateGlowSprite(96, new Color(yellow.r, yellow.g, yellow.b, 0.5f));
        glowSr.sortingOrder = 9;
        glow.transform.localScale = Vector3.one * 1.6f;

        GameObject go = new GameObject("CornerHighlight_Auto");
        go.transform.SetParent(glow.transform, false);
        go.transform.localPosition = Vector3.zero;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = RoundedUI.CreateRoundedRect(64, 32, yellow); // full radius = circle
        sr.sortingOrder = 10; // draw above walls/floor
        go.transform.localScale = Vector3.one * 0.4f; // relative to the glow parent's 1.6x scale

        cornerHighlight = glow;
    }

    // A gentle pulsing scale so the tappable corners actually catch the eye
    // instead of sitting there as a static dot.
    IEnumerator PulseHighlight()
    {
        Transform t = cornerHighlight.transform;
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * 4f) * 0.25f;
            t.localScale = Vector3.one * scale;
            yield return null;
        }
    }

    public void ShowAsExit(bool show)
    {
        EnsureExitMarker();
        exitMarker.SetActive(show);
    }

    void EnsureExitMarker()
    {
        if (exitMarker != null) return;

        Color red = new Color(1f, 0.3f, 0.3f, 0.8f);

        GameObject glow = new GameObject("ExitGlow_Auto");
        glow.transform.SetParent(transform, false);
        var glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = RoundedUI.CreateGlowSprite(96, new Color(red.r, red.g, red.b, 0.5f));
        glowSr.sortingOrder = 9;
        glow.transform.localScale = Vector3.one * 1.6f;

        GameObject go = new GameObject("ExitMarker_Auto");
        go.transform.SetParent(glow.transform, false);
        go.transform.localPosition = Vector3.zero;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = RoundedUI.CreateRoundedRect(64, 32, red); // small red dot - "exit here"
        sr.sortingOrder = 10;
        go.transform.localScale = Vector3.one * 0.3f; // relative to the glow parent's 1.6x scale

        exitMarker = glow;
    }

    // Requires a Collider2D on this GameObject (or a child). Works for mouse
    // and single-touch taps on both desktop and mobile.
    void OnMouseDown()
    {
        OnClicked?.Invoke(this);
    }

    public static Directions Opposite(Directions dir)
    {
        switch (dir)
        {
            case Directions.TOP: return Directions.BOTTOM;
            case Directions.BOTTOM: return Directions.TOP;
            case Directions.LEFT: return Directions.RIGHT;
            case Directions.RIGHT: return Directions.LEFT;
            default: return Directions.NONE;
        }
    }

    public static Vector2Int Delta(Directions dir)
    {
        switch (dir)
        {
            case Directions.TOP: return Vector2Int.up;
            case Directions.BOTTOM: return Vector2Int.down;
            case Directions.LEFT: return Vector2Int.left;
            case Directions.RIGHT: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }
}