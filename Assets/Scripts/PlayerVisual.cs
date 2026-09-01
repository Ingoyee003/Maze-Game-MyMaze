using UnityEngine;
using System.Collections;

// Fully code-generated player look - no external art needed.
// - Draws a soft circle sprite at runtime (Texture2D, pixel by pixel)
// - A TrailRenderer behind it acts as the "rocket flame" (fades yellow -> red -> transparent)
// - A ParticleSystem bursts a few sparkles every time the player takes a step
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisual : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] int textureSize = 64;
    [SerializeField] Color bodyColor = new Color(0.25f, 0.85f, 1f); // cyan circle
    [Tooltip("How big the player appears in world units - increase this if the player looks too small next to the maze rooms")]
    [SerializeField] float bodyWorldSize = 2.2f;

    [Header("Flame trail")]
    [SerializeField] Color trailStartColor = Color.yellow;
    [SerializeField] Color trailMidColor = new Color(1f, 0.5f, 0f); // orange
    [SerializeField] float trailTime = 0.35f;
    [SerializeField] float trailWidth = 0.35f;

    [Header("Sparkles")]
    [SerializeField] int sparkleCount = 8;
    [SerializeField] float sparkleSpeed = 2f;

    SpriteRenderer spriteRenderer;
    TrailRenderer trail;
    ParticleSystem sparkles;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GenerateCircleSprite(textureSize, bodyColor);
        spriteRenderer.sortingOrder = 3;

        SetupTrail();
        SetupSparkles();
    }

    // Draws a filled circle into a Texture2D with a soft antialiased edge,
    // then turns it into a Sprite. This is the whole "shape" - no image file needed.
    Sprite GenerateCircleSprite(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float edge = radius - dist;
                float alpha = Mathf.Clamp01(edge); // 1 inside, fades to 0 right at the edge
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        tex.Apply();
        // pixelsPerUnit = textureSize / bodyWorldSize means the sprite renders
        // exactly bodyWorldSize units wide in the world, regardless of texture
        // resolution - this is what makes the player properly sized next to
        // the maze rooms instead of looking like a tiny dot.
        float pixelsPerUnit = size / bodyWorldSize;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    void SetupTrail()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.03f;
        trail.material = SpriteShaderUtil.CreateMaterial();
        trail.sortingOrder = 1; // behind the player body, in front of the maze

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(trailStartColor, 0f),
                new GradientColorKey(trailMidColor, 0.5f),
                new GradientColorKey(Color.red, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        trail.colorGradient = gradient;
    }

    void SetupSparkles()
    {
        GameObject sparkleObj = new GameObject("Sparkles");
        sparkleObj.transform.SetParent(transform);
        sparkleObj.transform.localPosition = Vector3.zero;

        sparkles = sparkleObj.AddComponent<ParticleSystem>();

        var main = sparkles.main;
        main.startLifetime = 0.4f;
        main.startSpeed = sparkleSpeed;
        main.startSize = 0.08f;
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = sparkles.emission;
        emission.enabled = false; // we trigger bursts manually from PlayMoveEffect

        var shape = sparkles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        var colorOverLifetime = sparkles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.yellow, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = g;

        var renderer = sparkles.GetComponent<ParticleSystemRenderer>();
        renderer.material = SpriteShaderUtil.CreateMaterial();
        renderer.sortingOrder = 4; // in front of everything
    }

    // Call this to hide/show the player's look without deactivating the
    // GameObject itself (deactivating would stop Awake() from ever running
    // again and break PlayerController.Instance).
    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (trail != null) trail.enabled = visible;
    }

    // Call this from PlayerController every time the player takes a step,
    // passing the direction they just moved in (e.g. Vector2.up).
    public void PlayMoveEffect(Vector2 direction)
    {
        if (sparkles == null) return;

        var emitParams = new ParticleSystem.EmitParams
        {
            velocity = (-direction * sparkleSpeed) + (Vector2)Random.insideUnitCircle * 0.5f,
        };

        sparkles.Emit(emitParams, sparkleCount);
    }

    // Call this when the player taps a direction that's blocked by a wall -
    // a quick red flash makes it obvious the input registered but was blocked.
    public void PlayBumpEffect()
    {
        StopCoroutine(nameof(BumpFlash));
        StartCoroutine(BumpFlash());
    }

    IEnumerator BumpFlash()
    {
        Color original = bodyColor;
        spriteRenderer.color = Color.red;

        float t = 0f;
        float duration = 0.15f;
        while (t < duration)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.red, original, t / duration);
            yield return null;
        }

        spriteRenderer.color = original;
    }
}