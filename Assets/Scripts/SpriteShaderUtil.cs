using UnityEngine;

// URP projects often don't ship the legacy "Sprites/Default" shader, which
// made Shader.Find return null and crash Material creation for particles/
// trails. This tries modern URP shaders first, then legacy fallbacks, and
// never lets a caller create a Material with a null shader.
public static class SpriteShaderUtil
{
    public static Shader Get()
    {
        string[] candidates =
        {
            "Universal Render Pipeline/2D/Sprite-Lit-Default",
            "Universal Render Pipeline/2D/Sprite-Unlit-Default",
            "Sprites/Default",
            "Unlit/Transparent",
            "UI/Default",
        };

        foreach (var name in candidates)
        {
            Shader s = Shader.Find(name);
            if (s != null) return s;
        }

        Debug.LogError("SpriteShaderUtil: no compatible sprite shader found - trail/particle visuals may be missing, but this won't crash the game.");
        return null;
    }

    // Convenience: returns a usable Material, or null if truly nothing was found.
    public static Material CreateMaterial()
    {
        Shader shader = Get();
        return shader != null ? new Material(shader) : null;
    }
}