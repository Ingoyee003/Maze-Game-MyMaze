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
        if (cornerHighlight == null) return;

        cornerHighlight.SetActive(show);

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        if (show) pulseRoutine = StartCoroutine(PulseHighlight());
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
        if (exitMarker != null) exitMarker.SetActive(show);
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