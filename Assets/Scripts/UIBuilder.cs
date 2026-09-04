using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Attach this ONE script to the Canvas. It builds the ENTIRE UI at runtime -
// every panel, button, toggle, and the scrollable level grid - using Layout
// Groups so it lays out correctly on any screen size automatically. No
// manual RectTransform/anchor/OnClick wiring needed anywhere.
//
// After this runs, UITheme (also on the Canvas) applies colors on top.
public class UIBuilder : MonoBehaviour
{
    RectTransform canvasRect;

    void Awake()
    {
        canvasRect = GetComponent<RectTransform>();

        BuildMainMenu();
        BuildLevelSelect();
        BuildSettings();
        BuildHowToPlay();
        BuildHUD();
        BuildPause();
        BuildWin();
    }

    // ============================================================
    // MAIN MENU
    // ============================================================
    void BuildMainMenu()
    {
        RectTransform panel = CreateFullScreenPanel("MainMenuPanel");
        RectTransform menu = CreateVerticalMenu(panel, 700, 30);

        CreateTitle(menu, "GenMaze", 90);
        CreateSpacer(menu, 40);

        CreateMenuButton(menu, "PlayButton", "PLAY", 110, () => GameManager.Instance.OnPlayPressed());
        CreateMenuButton(menu, "LevelSelectButton", "LEVEL SELECT", 90, () => GameManager.Instance.OnOpenLevelSelectPressed());
        CreateMenuButton(menu, "HowToPlayButton", "HOW TO PLAY", 90, () => GameManager.Instance.OnOpenHowToPlayPressed());
        CreateMenuButton(menu, "SettingsButton", "SETTINGS", 90, () => GameManager.Instance.OnSettingsPressed());
        CreateMenuButton(menu, "QuitButton", "EXIT", 90, () => GameManager.Instance.OnQuitPressed());
    }

    // ============================================================
    // LEVEL SELECT (scrollable endless grid)
    // ============================================================
    void BuildLevelSelect()
    {
        RectTransform panel = CreateFullScreenPanel("LevelSelectPanel");

        CreateBackButton(panel, () => GameManager.Instance.OnCloseLevelSelectPressed());
        CreateHeaderTitle(panel, "SELECT LEVEL");

        // --- Scroll View, built entirely in code ---
        RectTransform scrollRoot = CreateRect("ScrollView", panel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(scrollRoot, 60, 220, 60, 60); // left, top, right, bottom padding

        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        RectTransform viewport = CreateRect("Viewport", scrollRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(viewport, 0, 0, 0, 0);
        viewport.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, 0.001f); // near-invisible but needed for Mask raycasting
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        RectTransform content = CreateRect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0, 0);

        var grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(140, 140);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(10, 10, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.UpperCenter;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;

        var levelSelectUI = panel.gameObject.AddComponent<LevelSelectUI>();
        levelSelectUI.SetGridContent(content);
    }

    // ============================================================
    // SETTINGS
    // ============================================================
    void BuildSettings()
    {
        RectTransform panel = CreateFullScreenPanel("SettingsPanel");

        CreateBackButton(panel, () => GameManager.Instance.OnCloseSettingsPressed());
        CreateHeaderTitle(panel, "SETTINGS");

        RectTransform menu = CreateVerticalMenu(panel, 700, 25);
        CreateSpacer(menu, 80);

        CreateToggleRow(menu, "Sound", true, isOn => AudioManager.Instance.SetSfxVolume(isOn ? 1f : 0f));
        CreateToggleRow(menu, "Music", true, isOn => AudioManager.Instance.SetMusicVolume(isOn ? 1f : 0f));
        CreateToggleRow(menu, "Vibration", true, isOn => PlayerPrefs.SetInt("VibrationEnabled", isOn ? 1 : 0));
        CreateToggleRow(menu, "Tutorial Hints", true, isOn => PlayerPrefs.SetInt("TutorialHintsEnabled", isOn ? 1 : 0));
    }

    // ============================================================
    // HOW TO PLAY
    // ============================================================
    void BuildHowToPlay()
    {
        RectTransform panel = CreateFullScreenPanel("HowToPlayPanel");
        panel.gameObject.SetActive(false);

        CreateBackButton(panel, () => GameManager.Instance.OnCloseHowToPlayPressed());
        CreateHeaderTitle(panel, "HOW TO PLAY");

        RectTransform menu = CreateVerticalMenu(panel, 800, 30);
        CreateSpacer(menu, 100);

        string[] tips =
        {
            "Tap a glowing corner to start.",
            "Find the flagged exit room.",
            "Use the arrows to move around.",
            "Pinch to zoom, drag to pan.",
            "Finish fast for a higher score!",
        };

        foreach (var tip in tips)
            CreateBodyText(menu, "- " + tip, 34);
    }

    // ============================================================
    // HUD (in-game overlay)
    // ============================================================
    void BuildHUD()
    {
        RectTransform panel = CreateFullScreenPanel("HUDPanel");
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0); // fully transparent container

        // Top info strip
        RectTransform topBar = CreateRect("TopBar", panel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        topBar.anchoredPosition = new Vector2(0, -30);
        topBar.sizeDelta = new Vector2(600, 140);
        var topLayout = topBar.gameObject.AddComponent<VerticalLayoutGroup>();
        topLayout.childAlignment = TextAnchor.UpperCenter;
        topLayout.spacing = 4;
        topLayout.childControlHeight = false;
        topLayout.childForceExpandHeight = false;

        CreateHUDText(topBar, "LevelText", "Level 1", 28, () => UITheme.Palette.TextMuted);
        CreateHUDText(topBar, "TimerText", "Time: 0", 40, () => UITheme.Palette.TextDark);
        CreateHUDText(topBar, "ScoreText", "Score: 0", 34, () => UITheme.Palette.GoldStar);
        CreateHUDText(topBar, "HintText", "", 26, () => UITheme.Palette.TextMuted);

        // Pause button, top-right corner
        RectTransform pauseBtn = CreateButton(panel, "PauseButton", "II", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-30, -30), new Vector2(100, 100), () => GameManager.Instance.OnPausePressed());

        // D-pad, bottom-left corner
        RectTransform dpad = CreateRect("DPadContainer", panel, Vector2.zero, Vector2.zero, Vector2.zero);
        dpad.anchoredPosition = new Vector2(230, 230);
        dpad.sizeDelta = new Vector2(10, 10);

        CreateButton(dpad, "UpBtn", "^", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 140), new Vector2(140, 140), () => PlayerController.Instance.MoveUp());
        CreateButton(dpad, "DownBtn", "v", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -140), new Vector2(140, 140), () => PlayerController.Instance.MoveDown());
        CreateButton(dpad, "LeftBtn", "<", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-140, 0), new Vector2(140, 140), () => PlayerController.Instance.MoveLeft());
        CreateButton(dpad, "RightBtn", ">", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(140, 0), new Vector2(140, 140), () => PlayerController.Instance.MoveRight());

        // Wire everything up in code (no manual Inspector dragging needed).
        var hud = panel.gameObject.AddComponent<HUDDisplay>();
        hud.SetReferences(
            topBar.Find("TimerText").GetComponent<TMP_Text>(),
            topBar.Find("ScoreText").GetComponent<TMP_Text>(),
            topBar.Find("LevelText").GetComponent<TMP_Text>()
        );

        var cornerSelector = FindObjectOfType<MazeCornerSelector>();
        if (cornerSelector != null)
            cornerSelector.SetHintText(topBar.Find("HintText").GetComponent<TMP_Text>());
    }

    // ============================================================
    // PAUSE MENU (overlay)
    // ============================================================
    void BuildPause()
    {
        RectTransform panel = CreateFullScreenPanel("PausePanel");
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f); // dim backdrop

        RectTransform card = CreateVerticalMenu(panel, 620, 22);
        var cardBg = card.gameObject.AddComponent<Image>();
        RoundedUI.ApplyRoundedStyle(cardBg, 36);
        cardBg.color = UITheme.Palette.CardWhite;
        var cardLayout = card.GetComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(40, 40, 50, 50);

        CreateTitle(card, "PAUSED", 56);
        CreateSpacer(card, 20);

        CreateMenuButton(card, "ResumeButton", "RESUME", 90, () => GameManager.Instance.OnResumePressed());
        CreateMenuButton(card, "RestartButton", "RESTART", 80, () => GameManager.Instance.OnRestartLevelPressed());
        CreateMenuButton(card, "PauseSettingsButton", "SETTINGS", 80, () => GameManager.Instance.OnSettingsPressed());
        CreateMenuButton(card, "MainMenuButton", "MAIN MENU", 80, () => GameManager.Instance.OnQuitToMenuPressed());
        CreateMenuButton(card, "ExitButton", "EXIT", 80, () => GameManager.Instance.OnQuitPressed());
    }

    // ============================================================
    // WIN / LEVEL COMPLETE
    // ============================================================
    void BuildWin()
    {
        RectTransform panel = CreateFullScreenPanel("WinPanel");

        RectTransform card = CreateVerticalMenu(panel, 650, 20);

        // Simple code-drawn checkmark badge - a colored circle with a check symbol, no art needed.
        RectTransform badge = CreateRect("CheckBadge", card, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        var le = badge.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 140;
        le.preferredHeight = 140;
        var badgeImg = badge.gameObject.AddComponent<Image>();
        badgeImg.sprite = RoundedUI.CreateRoundedRect(128, 64, UITheme.Palette.PrimaryGreen);
        var checkText = CreateText(badge, "✓", 70, Color.white);
        StretchFull(checkText.GetComponent<RectTransform>());

        CreateTitle(card, "LEVEL COMPLETE!", 54);
        CreateSpacer(card, 10);

        CreateStatRow(card, "TimeRow", "Time", "0.0s");
        CreateStatRow(card, "ScoreRow", "Score", "0");
        CreateSpacer(card, 10);

        CreateMenuButton(card, "NextLevelButton", "NEXT LEVEL", 90, () => GameManager.Instance.OnNextLevelPressed());
        CreateMenuButton(card, "WinLevelSelectButton", "LEVEL SELECT", 80, () => GameManager.Instance.OnOpenLevelSelectPressed());
        CreateMenuButton(card, "WinHomeButton", "HOME", 80, () => GameManager.Instance.OnQuitToMenuPressed());

        var winDisplay = panel.gameObject.AddComponent<WinPanelDisplay>();
        winDisplay.SetReferences(
            card.Find("TimeRow/Value").GetComponent<TMP_Text>(),
            card.Find("ScoreRow/Value").GetComponent<TMP_Text>()
        );
    }

    // ============================================================
    // Low-level helpers
    // ============================================================

    RectTransform CreateFullScreenPanel(string name)
    {
        RectTransform rt = CreateRect(name, canvasRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        SetOffsets(rt, 0, 0, 0, 0);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = null;
        img.color = UITheme.Palette.BackgroundLight;
        rt.gameObject.SetActive(false); // GameManager controls visibility - never start active
        return rt;
    }

    RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        return rt;
    }

    void SetOffsets(RectTransform rt, float left, float top, float right, float bottom)
    {
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // A centered vertical stack that auto-sizes to its content - this is
    // what makes every menu correct on any screen size without manual
    // positioning of each button.
    RectTransform CreateVerticalMenu(Transform parent, float width, float spacing)
    {
        RectTransform rt = CreateRect("MenuContainer", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.sizeDelta = new Vector2(width, 0);

        var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = rt.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rt;
    }

    void CreateSpacer(Transform parent, float height)
    {
        RectTransform rt = CreateRect("Spacer", parent, Vector2.zero, Vector2.zero, Vector2.zero);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    TextMeshProUGUI CreateTitle(Transform parent, string text, int fontSize)
    {
        var t = CreateText(parent, text, fontSize, UITheme.Palette.TextDark);
        t.fontStyle = FontStyles.Bold;
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize * 1.3f;
        return t;
    }

    TextMeshProUGUI CreateBodyText(Transform parent, string text, int fontSize)
    {
        var t = CreateText(parent, text, fontSize, UITheme.Palette.TextDark);
        t.alignment = TextAlignmentOptions.Left;
        var le = t.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize * 1.6f;
        return t;
    }

    TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, Color color)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    Button CreateMenuButton(Transform parent, string name, string label, float height, Action onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        var img = go.GetComponent<Image>();
        RoundedUI.ApplyRoundedStyle(img, 24);
        img.color = UITheme.Palette.PrimaryGreen;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        var text = CreateText(go.transform, label, 36, Color.white);
        text.fontStyle = FontStyles.Bold;
        StretchFull(text.GetComponent<RectTransform>());

        return btn;
    }

    // Generic corner/absolute-positioned button (used for D-pad, pause button).
    RectTransform CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size, Action onClick)
    {
        RectTransform rt = CreateRect(name, parent, anchorMin, anchorMax, pivot);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = rt.gameObject.AddComponent<Image>();
        RoundedUI.ApplyRoundedStyle(img, 20);
        img.color = UITheme.Palette.SecondaryMint;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        var text = CreateText(rt, label, 32, UITheme.Palette.TextDark);
        text.fontStyle = FontStyles.Bold;
        StretchFull(text.GetComponent<RectTransform>());

        return rt;
    }

    void CreateBackButton(Transform parent, Action onClick)
    {
        CreateButton(parent, "BackButton", "<", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(30, -30), new Vector2(90, 90), onClick);
    }

    void CreateHeaderTitle(Transform parent, string text)
    {
        RectTransform rt = CreateRect("HeaderTitle", parent, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        rt.anchoredPosition = new Vector2(0, -50);
        rt.sizeDelta = new Vector2(800, 100);
        var t = CreateText(rt, text, 50, UITheme.Palette.TextDark);
        t.fontStyle = FontStyles.Bold;
        StretchFull(t.GetComponent<RectTransform>());
    }

    TextMeshProUGUI CreateHUDText(Transform parent, string name, string text, int fontSize, Func<Color> colorFn)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize * 1.3f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = colorFn();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
    }

    void CreateToggleRow(Transform parent, string label, bool defaultOn, Action<bool> onChanged)
    {
        RectTransform row = CreateRect(label + "Row", parent, Vector2.zero, Vector2.zero, Vector2.zero);
        var le = row.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 90;
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlWidth = false;
        rowLayout.childForceExpandWidth = false;

        var labelText = CreateText(row, label, 34, UITheme.Palette.TextDark);
        labelText.alignment = TextAlignmentOptions.Left;
        var labelLe = labelText.gameObject.AddComponent<LayoutElement>();
        labelLe.preferredWidth = 450;
        labelLe.preferredHeight = 80;

        GameObject toggleGo = new GameObject("Toggle", typeof(RectTransform));
        toggleGo.transform.SetParent(row, false);
        var toggleLe = toggleGo.AddComponent<LayoutElement>();
        toggleLe.preferredWidth = 100;
        toggleLe.preferredHeight = 60;

        var bg = toggleGo.AddComponent<Image>();
        RoundedUI.ApplyRoundedStyle(bg, 30);
        bg.color = UITheme.Palette.MutedGrey;

        var toggle = toggleGo.AddComponent<Toggle>();
        toggle.targetGraphic = bg;
        toggle.isOn = defaultOn;

        GameObject knobGo = new GameObject("Knob", typeof(RectTransform));
        knobGo.transform.SetParent(toggleGo.transform, false);
        var knobRt = knobGo.GetComponent<RectTransform>();
        knobRt.anchorMin = knobRt.anchorMax = new Vector2(defaultOn ? 1 : 0, 0.5f);
        knobRt.pivot = new Vector2(0.5f, 0.5f);
        knobRt.sizeDelta = new Vector2(50, 50);
        knobRt.anchoredPosition = new Vector2(defaultOn ? -28 : 28, 0);
        var knobImg = knobGo.AddComponent<Image>();
        RoundedUI.ApplyRoundedStyle(knobImg, 25);
        knobImg.color = Color.white;
        toggle.graphic = knobImg;

        toggle.onValueChanged.AddListener(isOn =>
        {
            knobRt.anchorMin = knobRt.anchorMax = new Vector2(isOn ? 1 : 0, 0.5f);
            knobRt.anchoredPosition = new Vector2(isOn ? -28 : 28, 0);
            bg.color = isOn ? UITheme.Palette.PrimaryGreen : UITheme.Palette.MutedGrey;
            onChanged(isOn);
        });
    }

    void CreateStatRow(Transform parent, string name, string label, string defaultValue)
    {
        RectTransform row = CreateRect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero);
        var le = row.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 70;
        var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.spacing = 20;

        var labelText = CreateText(row, label, 32, UITheme.Palette.TextMuted);
        labelText.gameObject.AddComponent<LayoutElement>().preferredWidth = 200;

        var valueText = CreateText(row, defaultValue, 36, UITheme.Palette.TextDark);
        valueText.name = "Value";
        valueText.fontStyle = FontStyles.Bold;
        valueText.gameObject.AddComponent<LayoutElement>().preferredWidth = 200;
    }
}