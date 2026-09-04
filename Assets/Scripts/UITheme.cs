using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach this ONE script to the Canvas GameObject. It automatically finds
// every panel, button, and text by name (anywhere under the Canvas) and
// colors/shapes them according to the palette below - no manual clicking
// through the Inspector needed. Matches a clean, light, card-based design.
[ExecuteAlways]
public class UITheme : MonoBehaviour
{
    public static class Palette
    {
        // Light, minimal background + white cards (like the reference design)
        public static Color BackgroundLight = Hex("F3F7F5");
        public static Color CardWhite = Hex("FFFFFF");
        // HUDPanel itself should have NO visible background - it's just a
        // container. The maze needs to show through fully. Individual HUD
        // elements (timer text, D-pad buttons) already have their own
        // backgrounds, so the panel's own Image should be fully transparent.
        public static Color HUDOverlay = HexA("FFFFFF", 0);

        public static Color PrimaryGreen = Hex("4CAF7D");   // main CTA buttons (Play, Continue, Resume)
        public static Color SecondaryMint = Hex("A7E3C5");  // secondary actions (Settings, D-pad)
        public static Color MutedGrey = Hex("E7ECEA");      // quit / back / close (low emphasis)

        public static Color TextDark = Hex("2D3436");
        public static Color TextMuted = Hex("8A9A93");
        public static Color TextOnGreen = Hex("FFFFFF");
        public static Color GoldStar = Hex("FFC93C");

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

    [Tooltip("Corner roundness in pixels - higher = more rounded")]
    [SerializeField] int panelCornerRadius = 36;
    [SerializeField] int buttonCornerRadius = 24;

    void OnEnable() => Apply();
    void Start() => Apply();

    // Right-click this component's header in the Inspector to re-run any time.
    [ContextMenu("Apply Theme Now")]
    public void Apply()
    {
        StylePanel("MainMenuPanel", Palette.BackgroundLight, false);
        StylePanel("LevelSelectPanel", Palette.BackgroundLight, false); // pause panel
        StylePanel("HUDPanel", Palette.HUDOverlay, false);
        StylePanel("WinPanel", Palette.BackgroundLight, false);
        StylePanel("SettingsPanel", Palette.BackgroundLight, false);

        StyleButton("PlayButton", Palette.PrimaryGreen, Palette.TextOnGreen);
        StyleButton("ContinueButton", Palette.PrimaryGreen, Palette.TextOnGreen);
        StyleButton("NextLevelButton", Palette.PrimaryGreen, Palette.TextOnGreen);

        StyleButton("SettingsButton", Palette.CardWhite, Palette.TextDark);
        StyleButton("UpBtn", Palette.SecondaryMint, Palette.TextDark);
        StyleButton("DownBtn", Palette.SecondaryMint, Palette.TextDark);
        StyleButton("LeftBtn", Palette.SecondaryMint, Palette.TextDark);
        StyleButton("RightBtn", Palette.SecondaryMint, Palette.TextDark);
        StyleButton("PauseButton", Palette.CardWhite, Palette.TextDark);

        StyleButton("QuitButton", Palette.MutedGrey, Palette.TextDark);
        StyleButton("BackButton", Palette.MutedGrey, Palette.TextDark);
        StyleButton("CloseSettingsButton", Palette.MutedGrey, Palette.TextDark);

        SetTextColor("TimerText", Palette.TextDark);
        SetTextColor("ScoreText", Palette.GoldStar);
        SetTextColor("ResultText", Palette.TextDark);
        SetTextColor("HintText", Palette.TextMuted);

        // Any button not explicitly listed above still gets a sane default look.
        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            var img = btn.GetComponent<Image>();
            if (img != null && img.sprite == null)
                RoundedUI.ApplyRoundedStyle(img, buttonCornerRadius);
        }
    }

    void StylePanel(string objectName, Color color, bool rounded)
    {
        Transform t = FindDeep(transform, objectName);
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;

        if (rounded)
        {
            RoundedUI.ApplyRoundedStyle(img, panelCornerRadius);
        }
        else
        {
            // Unity's default "Background" UI sprite has its own grey tint
            // baked in, so setting color alone still looks grey. Clearing
            // the sprite makes it a plain solid-color fill instead.
            img.sprite = null;
        }
        img.color = color;
    }

    void StyleButton(string objectName, Color bgColor, Color textColor)
    {
        Transform t = FindDeep(transform, objectName);
        if (t == null) return;

        var img = t.GetComponent<Image>();
        if (img != null)
        {
            RoundedUI.ApplyRoundedStyle(img, buttonCornerRadius);
            img.color = bgColor;
        }

        var label = t.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.color = textColor;
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