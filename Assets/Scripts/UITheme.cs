using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class UITheme : MonoBehaviour
{
    public static class Palette
    {
        public static Color BackgroundLight = Hex("EAF2ED"); // slightly deeper so white cards stand out
        public static Color CardWhite = Hex("FFFFFF");
        public static Color CardBorder = Hex("D5E3DB");
        public static Color HUDOverlay = HexA("FFFFFF", 0);
        public static Color HUDBar = Hex("FFFFFF");

        public static Color PrimaryGreen = Hex("4CAF7D");
        public static Color SecondaryMint = Hex("A7E3C5");
        public static Color MutedGrey = Hex("D7DEDA"); // more visible than before

        public static Color TextDark = Hex("2D3436");
        public static Color TextMuted = Hex("8A9A97");
        public static Color TextOnGreen = Hex("FFFFFF");
        public static Color GoldStar = Hex("E8A93C"); // slightly deeper gold, reads better on white

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }

        static Color HexA(string hex, int alpha)
        {
            Color c = Hex(hex);
            c.a = alpha / 255f;
            return c;
        }
    }

    [SerializeField] int panelCornerRadius = 36;
    [SerializeField] int buttonCornerRadius = 24;

    // Buttons that should look like the MAIN action (solid green).
    // Everything else defaults to a white bordered "secondary" look.
    static readonly string[] PrimaryButtons =
    {
        "PlayButton", "ContinueButton", "ResumeButton", "NextLevelButton"
    };

    // Low-emphasis buttons (muted grey).
    static readonly string[] MutedButtons =
    {
        "QuitButton", "ExitButton", "BackButton", "CloseSettingsButton"
    };

    void OnEnable() => Apply();
    void Start() => Apply();

    [ContextMenu("Apply Theme Now")]
    public void Apply()
    {
        StylePanel("MainMenuPanel", Palette.BackgroundLight);
        StylePanel("LevelSelectPanel", Palette.BackgroundLight);
        StylePanel("HUDPanel", Palette.HUDOverlay);
        StylePanel("WinPanel", Palette.BackgroundLight);
        StylePanel("SettingsPanel", Palette.BackgroundLight);
        StylePanel("HowToPlayPanel", Palette.BackgroundLight);

        SetTextColor("TimerText", Palette.TextDark);
        SetTextColor("ScoreText", Palette.GoldStar);
        SetTextColor("LevelText", Palette.TextMuted);
        SetTextColor("ResultText", Palette.TextDark);
        SetTextColor("HintText", Palette.TextMuted);

        // Every button gets a sane DEFAULT secondary look first...
        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            var img = btn.GetComponent<Image>();
            if (img == null) continue;

            RoundedUI.ApplyRoundedStyle(img, buttonCornerRadius);
            img.color = Palette.CardWhite;
            AddBorder(btn.gameObject, Palette.CardBorder);

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.color = Palette.TextDark;
        }

        // ...then the curated lists override specific ones.
        foreach (var name in PrimaryButtons)
            StyleButton(name, Palette.PrimaryGreen, Palette.TextOnGreen, removeBorder: true);

        foreach (var name in MutedButtons)
            StyleButton(name, Palette.MutedGrey, Palette.TextDark, removeBorder: true);

        // D-pad + pause button keep the mint accent.
        foreach (var name in new[] { "UpBtn", "DownBtn", "LeftBtn", "RightBtn", "PauseButton" })
            StyleButton(name, Palette.SecondaryMint, Palette.TextDark, removeBorder: true);
    }

    void StylePanel(string objectName, Color color)
    {
        Transform t = FindDeep(transform, objectName);
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = null;
        img.color = color;
    }

    void StyleButton(string objectName, Color bgColor, Color textColor, bool removeBorder = false)
    {
        Transform t = FindDeep(transform, objectName);
        if (t == null) return;

        var img = t.GetComponent<Image>();
        if (img != null) img.color = bgColor;

        if (removeBorder)
        {
            var outline = t.GetComponent<Outline>();
            if (outline != null) Destroy(outline);
        }

        var label = t.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.color = textColor;
    }

    // A thin visible border so white cards don't blend into the light background.
    void AddBorder(GameObject go, Color color)
    {
        var outline = go.GetComponent<Outline>();
        if (outline == null) outline = go.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(2, -2);
    }

    void SetTextColor(string objectName, Color color)
    {
        Transform t = FindDeep(transform, objectName);
        if (t == null) return;
        var txt = t.GetComponent<TMP_Text>();
        if (txt != null) txt.color = color;
    }

    Transform FindDeep(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName) return child;
            Transform result = FindDeep(child, objectName);
            if (result != null) return result;
        }
        return null;
    }
}