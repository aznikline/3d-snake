using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Player;
using NeonSerpent.Gameplay;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Procedural UI factory. Creates all game UI elements at runtime —
    /// no prefabs or scene setup needed.
    /// </summary>
    public static class UIFactory
    {
        private static TMP_FontAsset _cachedFont;

        public static TMP_FontAsset DefaultFont
        {
            get
            {
                if (_cachedFont != null) return _cachedFont;
                _cachedFont = TMP_Settings.defaultFontAsset;
                if (_cachedFont == null)
                    _cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                return _cachedFont;
            }
        }

        // ── Canvas Helpers ──

        public static GameObject CreateCanvas(string name, int sortingOrder, bool addScaler = true, bool addRaycaster = true)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            if (addScaler)
            {
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (addRaycaster)
                go.AddComponent<GraphicRaycaster>();

            return go;
        }

        // ── Text ──

        public static TextMeshProUGUI CreateText(GameObject parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = DefaultFont;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            return tmp;
        }

        // ── Image ──

        public static Image CreateFullscreenOverlay(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        // ── Buttons ──

        public static Button CreateButton(Transform parent, string name, string label, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Color? bgColor = null, Color? textColor = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = bgColor ?? new Color(0.9f, 0.92f, 0.9f, 0.95f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = image;

            var colors = btn.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.7f, 0.85f, 0.7f, 1f);
            colors.pressedColor = new Color(0.5f, 0.7f, 0.5f, 1f);
            btn.colors = colors;

            var labelText = CreateText(go, "Label", label, fontSize, textColor ?? new Color(0.15f, 0.4f, 0.15f));
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return btn;
        }

        public static Button CreateFixedButton(Transform parent, string name, string label, float yOffset)
        {
            return CreateButton(parent, name, label, 26f,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        }

        // ── Panels ──

        public static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color? bgColor = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var image = go.AddComponent<Image>();
            image.color = bgColor ?? new Color(0.9f, 0.92f, 0.9f, 0.95f);

            return go;
        }

        // ── Full Screen Builders ──

        public static HUDController BuildHUD(ComboSystem comboSystem, SnakeHeadController snakeController, VerletSnakeBody snakeBody)
        {
            var canvasGO = CreateCanvas("HUDCanvas", 0, addScaler: true);

            var hudGO = new GameObject("HUDController");
            hudGO.transform.SetParent(canvasGO.transform);
            var hud = hudGO.AddComponent<HUDController>();
            hud.comboSystem = comboSystem;
            hud.snakeController = snakeController;
            hud.snakeBody = snakeBody;

            // Collection flash
            var flashImg = CreateFullscreenOverlay(canvasGO.transform, "CollectionFlash");
            flashImg.gameObject.SetActive(false);
            hud.collectionFlash = flashImg;

            // Damage vignette
            var vignetteImg = CreateFullscreenOverlay(canvasGO.transform, "DamageVignette");
            vignetteImg.gameObject.SetActive(false);
            hud.damageVignette = vignetteImg;

            // HUD elements
            var t = canvasGO.transform;

            var speedText = CreateText(canvasGO, "SpeedText", "0.0 m/s", 20f, Color.white);
            var sr = speedText.rectTransform;
            sr.anchorMin = new Vector2(0.02f, 0.92f);
            sr.anchorMax = new Vector2(0.2f, 0.98f);
            hud.speedText = speedText;

            var barImg = new GameObject("SpeedBar").AddComponent<Image>();
            barImg.transform.SetParent(t);
            barImg.color = Color.green;
            var br = barImg.rectTransform;
            br.anchorMin = new Vector2(0.02f, 0.90f);
            br.anchorMax = new Vector2(0.2f, 0.91f);
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
            hud.speedBar = barImg;

            var comboFillImg = new GameObject("ComboFill").AddComponent<Image>();
            comboFillImg.transform.SetParent(t);
            comboFillImg.color = Color.white;
            var cfr = comboFillImg.rectTransform;
            cfr.anchorMin = new Vector2(0.35f, 0.03f);
            cfr.anchorMax = new Vector2(0.65f, 0.05f);
            cfr.offsetMin = Vector2.zero;
            cfr.offsetMax = Vector2.zero;
            hud.comboFill = comboFillImg;

            var comboCount = CreateText(canvasGO, "ComboCount", "", 24f, new Color(0.2f, 0.7f, 0.3f));
            var ccr = comboCount.rectTransform;
            ccr.anchorMin = new Vector2(0.45f, 0.06f);
            ccr.anchorMax = new Vector2(0.55f, 0.09f);
            hud.comboCountText = comboCount;

            var dashImg = new GameObject("DashIcon").AddComponent<Image>();
            dashImg.transform.SetParent(t);
            dashImg.color = Color.gray;
            var dr = dashImg.rectTransform;
            dr.anchorMin = new Vector2(0.92f, 0.03f);
            dr.anchorMax = new Vector2(0.97f, 0.09f);
            dr.offsetMin = Vector2.zero;
            dr.offsetMax = Vector2.zero;
            hud.dashIcon = dashImg;

            var lengthText = CreateText(canvasGO, "LengthText", "Length: 10", 20f, Color.white, TextAlignmentOptions.Right);
            var lr = lengthText.rectTransform;
            lr.anchorMin = new Vector2(0.8f, 0.92f);
            lr.anchorMax = new Vector2(0.98f, 0.98f);
            hud.lengthText = lengthText;

            var scoreText = CreateText(canvasGO, "ScoreText", "Score: 0", 18f, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Right);
            var scr = scoreText.rectTransform;
            scr.anchorMin = new Vector2(0.8f, 0.87f);
            scr.anchorMax = new Vector2(0.98f, 0.91f);
            hud.scoreText = scoreText;

            return hud;
        }

        public static MainMenuController BuildMainMenu(GameBootstrap bootstrap)
        {
            var canvasGO = CreateCanvas("MainMenuCanvas", 10);

            var mainMenu = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            mainMenu.transform.SetParent(canvasGO.transform);
            mainMenu.gameBootstrap = bootstrap;

            // Background
            var bg = CreateFullscreenOverlay(canvasGO.transform, "MenuBackground");
            bg.color = new Color(0.85f, 0.88f, 0.85f, 1f);
            bg.raycastTarget = true;
            bg.transform.SetAsFirstSibling();

            // Title
            var title = CreateText(canvasGO, "TitleText", "POLY SERPENT", 72f, new Color(0.2f, 0.7f, 0.3f));
            title.fontStyle = FontStyles.Bold;
            var tr = title.rectTransform;
            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = new Vector2(0f, 300f);
            tr.sizeDelta = new Vector2(800f, 100f);

            // Info
            var info = CreateText(canvasGO, "InfoText",
                "WASD Move | Mouse Look | Shift Dash | E Grapple | ESC Pause",
                18f, new Color(0.7f, 0.7f, 0.7f, 1f));
            var ir = info.rectTransform;
            ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.5f);
            ir.anchoredPosition = new Vector2(0f, -350f);
            ir.sizeDelta = new Vector2(800f, 40f);
            mainMenu.infoText = info;

            // Buttons
            var t = canvasGO.transform;
            mainMenu.campaignButton = CreateMenuFixedButton(t, "BtnCampaign", "CAMPAIGN", 80f);
            mainMenu.settingsButton = CreateMenuFixedButton(t, "BtnSettings", "SETTINGS", 0f);
            mainMenu.quitButton = CreateMenuFixedButton(t, "BtnQuit", "QUIT", -80f);

            var mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(t);
            mainMenu.mainPanel = mainPanel;

            return mainMenu;
        }

        private static Button CreateMenuFixedButton(Transform parent, string name, string label, float yOffset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(320f, 56f);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.9f, 0.92f, 0.9f, 0.95f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = image;

            var colors = btn.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.7f, 0.85f, 0.7f, 1f);
            colors.pressedColor = new Color(0.5f, 0.7f, 0.5f, 1f);
            btn.colors = colors;

            var lt = CreateText(go, "Label", label, 26f, new Color(0.15f, 0.4f, 0.15f));
            var lr = lt.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;

            return btn;
        }

        public static PauseMenuController BuildPauseMenu()
        {
            var canvasGO = CreateCanvas("PauseCanvas", 100);

            var pauseMenu = new GameObject("PauseMenuController").AddComponent<PauseMenuController>();
            pauseMenu.transform.SetParent(canvasGO.transform);

            // Pause panel
            var panelGO = CreatePanel(canvasGO.transform, "PausePanel",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f));
            pauseMenu.pausePanel = panelGO;

            // Title
            var title = CreateText(panelGO, "PauseTitle", "PAUSED", 48f, new Color(0.2f, 0.7f, 0.3f));
            var tr = title.rectTransform;
            tr.anchorMin = new Vector2(0.1f, 0.75f);
            tr.anchorMax = new Vector2(0.9f, 0.9f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            // Buttons
            var pt = panelGO.transform;
            pauseMenu.resumeButton     = CreatePauseMenuButton(pt, "ResumeButton",    "RESUME",         0);
            pauseMenu.restartButton    = CreatePauseMenuButton(pt, "RestartButton",   "RESTART",        1);
            pauseMenu.settingsButton   = CreatePauseMenuButton(pt, "SettingsButton",  "SETTINGS",       2);
            pauseMenu.quitToMenuButton = CreatePauseMenuButton(pt, "QuitMenuButton",  "QUIT TO MENU",   3);
            pauseMenu.quitGameButton   = CreatePauseMenuButton(pt, "QuitGameButton",  "QUIT GAME",      4);

            panelGO.SetActive(false);
            return pauseMenu;
        }

        private static Button CreatePauseMenuButton(Transform parent, string name, string label, int index)
        {
            return CreateButton(parent, name, label, 20f,
                new Vector2(0.2f, 0.65f - index * 0.12f),
                new Vector2(0.8f, 0.75f - index * 0.12f));
        }

        public static GameOverScreenController BuildGameOverScreen()
        {
            var canvasGO = CreateCanvas("GameOverCanvas", 200);

            var controller = canvasGO.AddComponent<GameOverScreenController>();

            var campaignPanel = CreateGameOverPanel(canvasGO.transform, "CampaignPanel", "GAME OVER", new Color(0.8f, 0.2f, 0.2f));
            controller.campaignPanel = campaignPanel;

            controller.retryButton = CreateButton(canvasGO.transform, "RetryButton", "RETRY", 20f,
                new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.15f));
            controller.backToMenuButton = CreateButton(canvasGO.transform, "MenuButton", "MAIN MENU", 20f,
                new Vector2(0.35f, 0.00f), new Vector2(0.65f, 0.07f));

            canvasGO.SetActive(false);
            return controller;
        }

        private static GameObject CreateGameOverPanel(Transform parent, string name, string title, Color titleColor)
        {
            var panelGO = CreatePanel(parent, name,
                new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.8f));

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.font = DefaultFont;
            titleText.text = title;
            titleText.fontSize = 48f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = titleColor;

            return panelGO;
        }

        public static LevelCompleteScreenController BuildLevelCompleteScreen()
        {
            var canvasGO = CreateCanvas("LevelCompleteCanvas", 200);

            var controller = canvasGO.AddComponent<LevelCompleteScreenController>();

            var panelGO = CreatePanel(canvasGO.transform, "Panel",
                new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.85f));

            // Title
            var titleText = CreateText(panelGO, "Title", "LEVEL COMPLETE", 42f, Color.green);
            var tr = titleText.rectTransform;
            tr.anchorMin = new Vector2(0.1f, 0.85f);
            tr.anchorMax = new Vector2(0.9f, 0.95f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            // Rank
            var rankText = CreateText(panelGO, "RankText", "", 64f, Color.yellow);
            rankText.fontStyle = FontStyles.Bold;
            var rr = rankText.rectTransform;
            rr.anchorMin = new Vector2(0.1f, 0.6f);
            rr.anchorMax = new Vector2(0.9f, 0.8f);
            rr.offsetMin = Vector2.zero;
            rr.offsetMax = Vector2.zero;
            controller.rankText = rankText;

            // Score
            var scoreText = CreateText(panelGO, "ScoreText", "", 28f, Color.white);
            var sr = scoreText.rectTransform;
            sr.anchorMin = new Vector2(0.1f, 0.45f);
            sr.anchorMax = new Vector2(0.9f, 0.55f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            controller.scoreText = scoreText;

            // Time
            var timeText = CreateText(panelGO, "TimeText", "", 22f, new Color(0.7f, 0.7f, 0.7f));
            var tr2 = timeText.rectTransform;
            tr2.anchorMin = new Vector2(0.1f, 0.35f);
            tr2.anchorMax = new Vector2(0.9f, 0.42f);
            tr2.offsetMin = Vector2.zero;
            tr2.offsetMax = Vector2.zero;
            controller.timeText = timeText;

            // Next Level button
            var nextBtn = CreateButton(panelGO.transform, "NextLevelButton", "NEXT LEVEL", 22f,
                new Vector2(0.55f, 0.05f), new Vector2(0.9f, 0.12f),
                new Color(0.05f, 0.15f, 0.1f, 0.9f), Color.green);
            controller.nextLevelButton = nextBtn;

            // Menu button
            var menuBtn = CreateButton(panelGO.transform, "MenuButton", "MENU", 22f,
                new Vector2(0.1f, 0.05f), new Vector2(0.45f, 0.12f));
            controller.backToMenuButton = menuBtn;

            canvasGO.SetActive(false);
            return controller;
        }

        public static TutorialController BuildTutorial()
        {
            var canvasGO = CreateCanvas("TutorialCanvas", 50);

            var tutorial = canvasGO.AddComponent<TutorialController>();

            // Panel
            var panelGO = CreatePanel(canvasGO.transform, "TutorialPanel",
                new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.7f),
                new Color(0.02f, 0.02f, 0.05f, 0.9f));
            tutorial.tutorialPanel = panelGO;

            // Title
            var titleText = CreateText(panelGO, "TitleText", "", 32f, new Color(0.2f, 0.7f, 0.3f));
            var tr = titleText.rectTransform;
            tr.anchorMin = new Vector2(0.1f, 0.75f);
            tr.anchorMax = new Vector2(0.9f, 0.9f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            tutorial.titleText = titleText;

            // Instruction
            var instrText = CreateText(panelGO, "InstructionText", "", 22f, Color.white);
            var ir = instrText.rectTransform;
            ir.anchorMin = new Vector2(0.1f, 0.4f);
            ir.anchorMax = new Vector2(0.9f, 0.7f);
            ir.offsetMin = Vector2.zero;
            ir.offsetMax = Vector2.zero;
            tutorial.instructionText = instrText;

            // Skip button
            var skipBtn = CreateButton(panelGO.transform, "SkipButton", "SKIP", 18f,
                new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.15f),
                new Color(0.2f, 0.1f, 0.1f, 0.9f), new Color(0.8f, 0.4f, 0.4f));
            tutorial.skipButton = skipBtn;

            // Continue button
            var contBtn = CreateButton(panelGO.transform, "ContinueButton", "CONTINUE", 18f,
                new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.15f),
                new Color(0.1f, 0.2f, 0.2f, 0.9f), new Color(0.2f, 0.7f, 0.3f));
            tutorial.continueButton = contBtn;

            // Steps
            tutorial.steps = new TutorialController.TutorialStep[]
            {
                new() { title = "WELCOME TO POLY SERPENT", instruction = "You are a serpent in a low-poly city.\n\nUse WASD to move and Mouse to look around.", hint = "Press any key to continue...", displayDuration = 5f },
                new() { title = "COLLECT ENERGY CORES", instruction = "Find and collect energy cores to grow longer.\n\nThe more you eat, the longer you become!", hint = "Look for the glowing spheres.", displayDuration = 5f },
                new() { title = "BUILD YOUR COMBO", instruction = "Eat food quickly to build your combo meter.\n\nWhen full, you can activate DASH!", hint = "The combo meter is shown on your HUD.", displayDuration = 5f },
                new() { title = "AVOID COLLISIONS", instruction = "Hitting walls or your own tail will kill you.\n\nUse dash to survive one fatal collision!", hint = "Plan your path carefully.", displayDuration = 5f },
                new() { title = "READY TO BEGIN", instruction = "Complete each level by reaching the target length.\n\nGood luck, serpent!", hint = "Press ESC anytime to pause.", displayDuration = 4f }
            };

            canvasGO.SetActive(false);
            return tutorial;
        }
    }
}
