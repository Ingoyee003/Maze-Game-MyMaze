using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] float zoomSpeed = 0.05f;
    [SerializeField] float panSpeed = 1f;
    [SerializeField] float minZoomFactor = 0.15f; // closest zoom, relative to full-maze size
    [SerializeField] float edgePadding = 2f;

    Camera cam;
    float minZoom = 2f;   // sane defaults BEFORE any maze is loaded, so
    float maxZoom = 10f;  // scrolling on the menu screens can't zero these out
    Vector2 clampMin, clampMax;
    bool wasPinching = false;
    Vector3 shakeOffset = Vector3.zero;
    Vector3 basePosition; // real camera target position, WITHOUT shake mixed in

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();

        // Self-heal: if the scene was saved with a broken (zero/negative)
        // orthographic size, snap it back to something sane immediately.
        if (cam.orthographicSize <= 0.01f)
            cam.orthographicSize = maxZoom;
    }

    // Call this for tactile feedback (e.g. player bumps into a wall).
    public void Shake(float duration = 0.15f, float magnitude = 0.15f)
    {
        StopCoroutine(nameof(ShakeRoutine));
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / duration);
            shakeOffset = (Vector3)Random.insideUnitCircle * magnitude * damper;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    // Call after GenerateMaze.Initialize() so the camera frames the whole maze
    // and zoom limits scale correctly with maze size (bigger mazes on later levels).
    public void FitToMaze(int numX, int numY, float roomWidth, float roomHeight)
    {
        basePosition = new Vector3(
            (numX * roomWidth - roomWidth) / 2f,
            (numY * roomHeight - roomHeight) / 2f,
            -100f
        );
        transform.position = basePosition;

        float sizeX = numX * roomWidth;
        float sizeY = numY * roomHeight;

        maxZoom = Mathf.Max(sizeX, sizeY) * 0.6f;
        minZoom = Mathf.Max(1.5f, maxZoom * minZoomFactor);
        cam.orthographicSize = maxZoom; // start zoomed out to show the whole maze

        clampMin = Vector2.zero;
        clampMax = new Vector2(sizeX, sizeY);
    }

    void Update()
    {
        HandleTouch();
        HandleMouseScroll(); // editor/desktop testing
        ClampPosition();
    }

    void HandleMouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * 10f, minZoom, maxZoom);
    }

    void HandleTouch()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevDist = (t0Prev - t1Prev).magnitude;
            float curDist = (t0.position - t1.position).magnitude;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - (curDist - prevDist) * zoomSpeed,
                minZoom, maxZoom
            );

            wasPinching = true;
        }
        else if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (wasPinching)
            {
                // Swallow one frame right after a pinch ends to avoid a pan jump.
                wasPinching = false;
                return;
            }

            if (t.phase == TouchPhase.Moved)
            {
                Vector3 worldNow = cam.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, 0));
                Vector3 worldPrev = cam.ScreenToWorldPoint(
                    new Vector3(t.position.x - t.deltaPosition.x, t.position.y - t.deltaPosition.y, 0));

                basePosition -= (worldNow - worldPrev) * panSpeed;
            }
        }
        else
        {
            wasPinching = false;
        }
    }

    void ClampPosition()
    {
        basePosition.x = Mathf.Clamp(basePosition.x, clampMin.x - edgePadding, clampMax.x + edgePadding);
        basePosition.y = Mathf.Clamp(basePosition.y, clampMin.y - edgePadding, clampMax.y + edgePadding);
        transform.position = basePosition + shakeOffset;
    }
}