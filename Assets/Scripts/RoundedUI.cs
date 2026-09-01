using UnityEngine;
using UnityEngine.UI;

// Generates rounded-rectangle sprites at runtime (same technique as
// PlayerVisual's circle) so panels/buttons/cards get soft rounded corners
// without any external art, matching a clean modern design. Uses a 9-slice
// border so the same sprite scales cleanly at any button/panel size.
public static class RoundedUI
{
    public static Sprite CreateRoundedRect(int size, int cornerRadius, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = SampleAlpha(x, y, size, size, cornerRadius);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * color.a));
            }
        }
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius) // 9-slice border
        );
    }

    static float SampleAlpha(int x, int y, int width, int height, int radius)
    {
        float px = x + 0.5f;
        float py = y + 0.5f;

        // Clamp the point into the "inner rectangle" - if it's already
        // inside (on a straight edge), distance is 0 and it's fully opaque.
        // Near a corner, this measures distance to the rounded arc.
        float cx = Mathf.Clamp(px, radius, width - radius);
        float cy = Mathf.Clamp(py, radius, height - radius);
        float dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));

        return Mathf.Clamp01(radius - dist + 0.5f); // +0.5 gives a soft antialiased edge
    }

    // Applies a rounded sprite to any Image in one call, set to Sliced so it
    // scales properly regardless of the panel/button's actual size.
    public static void ApplyRoundedStyle(Image image, int cornerRadius = 28)
    {
        if (image == null) return;
        image.sprite = CreateRoundedRect(128, cornerRadius, Color.white);
        image.type = Image.Type.Sliced;
    }
}