using UnityEngine;

// Attach this to any empty GameObject in the scene (e.g. a "WinEffects" object,
// or on GameManager itself). Listens for ScoreManager.OnLevelWon and fires a
// colorful confetti burst at the player's position - no image assets needed.
public class WinCelebration : MonoBehaviour
{
    ParticleSystem confetti;

    void Awake()
    {
        GameObject obj = new GameObject("ConfettiBurst");
        confetti = obj.AddComponent<ParticleSystem>();

        var main = confetti.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 6f;
        main.startSize = 0.15f;
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 200;

        var emission = confetti.emission;
        emission.enabled = false; // manual bursts only

        var shape = confetti.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.2f;

        // Multiple colors so it actually reads as "confetti" rather than one blob.
        var colorOverLifetime = confetti.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.magenta, 0.5f),
                new GradientColorKey(Color.cyan, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        colorOverLifetime.color = g;

        var renderer = confetti.GetComponent<ParticleSystemRenderer>();
        renderer.material = SpriteShaderUtil.CreateMaterial();
        renderer.sortingOrder = 10;
    }

    void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnLevelWon += Celebrate;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnLevelWon -= Celebrate;
    }

    void Celebrate(int score, float time)
    {
        if (PlayerController.Instance == null) return;

        confetti.transform.position = PlayerController.Instance.transform.position;
        confetti.Emit(80);

        if (CameraController.Instance != null)
            CameraController.Instance.Shake(0.25f, 0.1f); // small celebratory punch, not a bump
    }
}