using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

/// <summary>
/// Editor script to fully set up and polish the Main Menu scene.
/// Runs automatically via InitializeOnLoadMethod after compilation.
/// Also available from: Tools > Setup Main Menu
/// </summary>
public class MainMenuSetupEditor
{
    // Sprite paths from GUIPackCartoon
    private const string GREEN_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Green.png";
    private const string BLUE_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Blue.png";
    private const string PURPLE_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Purple.png";
    private const string RED_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Red.png";
    private const string ORANGE_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Orange.png";
    private const string TEAL_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Blue (Teal).png";
    
    // Popup / panel backgrounds
    private const string POPUP_BLUE = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/Blue.png";
    private const string POPUP_WHITE = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/White.png";
    private const string POPUP_BG = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/background.png";
    private const string HEADLINE_FLAG = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/Headline - Flag.png";

    // Icons
    private const string TROPHY_ICON = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Badges/Cup.png";
    private const string TROPHY_BIG = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Badges/Cup (Big).png";
    private const string CROWN_ICON = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Badges/Crown.png";
    private const string STAR_ICON = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Stars/Star (Yellow).png";
    
    // Circle buttons (for close)
    private const string RED_CIRCLE = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Circles/Red.png";
    
    // Shapes
    private const string ROUNDED_RECT = "Assets/GUIPackCartoon/Demo/Sprites/Shapes/Rectangle/Rounded Rectangle - 512px.png";
    
    // Background
    private const string BG_IMAGE = "Assets/UI/MainMenu/main-menu-background.png";
    
    // Audio
    private const string HOVER_SOUND = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Bubble 04.wav";
    private const string CLICK_SOUND = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Interface Click 13-1.wav";
    private const string MENU_MUSIC = "Assets/Audio/main-menu.mp3";

    // Flag file to prevent re-running
    private const string FLAG_KEY = "MainMenuSetup_Done_v5";

    [InitializeOnLoadMethod]
    private static void OnDomainReload()
    {
        // Only run once per flag key
        if (SessionState.GetBool(FLAG_KEY, false))
            return;

        // Delay to let Unity finish loading
        EditorApplication.delayCall += () =>
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == "MainMenu")
            {
                SessionState.SetBool(FLAG_KEY, true);
                SetupMainMenu();
            }
        };
    }

    [MenuItem("Tools/Setup Main Menu")]
    public static void SetupMainMenu()
    {
        // Ensure we have the MainMenu scene open
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogError("[MainMenuSetup] Please open the MainMenu scene first!");
            return;
        }

        Debug.Log("[MainMenuSetup] Starting Main Menu setup...");

        // Ensure sprites are set as Sprite type
        EnsureSpriteImport(GREEN_BTN);
        EnsureSpriteImport(BLUE_BTN);
        EnsureSpriteImport(PURPLE_BTN);
        EnsureSpriteImport(RED_BTN);
        EnsureSpriteImport(ORANGE_BTN);
        EnsureSpriteImport(TEAL_BTN);
        EnsureSpriteImport(POPUP_BLUE);
        EnsureSpriteImport(POPUP_WHITE);
        EnsureSpriteImport(POPUP_BG);
        EnsureSpriteImport(HEADLINE_FLAG);
        EnsureSpriteImport(TROPHY_ICON);
        EnsureSpriteImport(TROPHY_BIG);
        EnsureSpriteImport(CROWN_ICON);
        EnsureSpriteImport(RED_CIRCLE);
        EnsureSpriteImport(ROUNDED_RECT);
        EnsureSpriteImport(BG_IMAGE, filterMode: FilterMode.Bilinear, maxSize: 2048, spriteMode: 1);
        
        if (File.Exists(GetFullPath(STAR_ICON)))
            EnsureSpriteImport(STAR_ICON);

        AssetDatabase.Refresh();

        // Find root objects
        var canvas = FindByName<Canvas>("MainMenuCanvas");
        if (canvas == null)
        {
            Debug.LogError("[MainMenuSetup] MainMenuCanvas not found!");
            return;
        }

        // ============================================
        // CANVAS SETUP
        // ============================================
        var canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        // ============================================
        // BACKGROUND IMAGE
        // ============================================
        var bgObj = FindChild(canvas.transform, "Background");
        if (bgObj != null)
        {
            var bgImage = bgObj.GetComponent<Image>();
            if (bgImage != null)
            {
                var bgSprite = LoadSprite(BG_IMAGE);
                if (bgSprite != null)
                {
                    bgImage.sprite = bgSprite;
                    bgImage.type = Image.Type.Simple;
                    bgImage.preserveAspect = false;
                    bgImage.color = Color.white;
                }

                // Set to stretch full screen
                var bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
        }

        // ============================================
        // BUTTON CONTAINER - Position under logo area
        // ============================================
        var buttonContainer = FindChild(canvas.transform, "ButtonContainer");
        if (buttonContainer != null)
        {
            var containerRect = buttonContainer.GetComponent<RectTransform>();
            // Position in lower portion of screen (below logo area)
            containerRect.anchorMin = new Vector2(0.5f, 0.05f);
            containerRect.anchorMax = new Vector2(0.5f, 0.55f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(500, 0);
            containerRect.pivot = new Vector2(0.5f, 0.5f);

            var layout = buttonContainer.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.spacing = 18;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(10, 10, 10, 10);
            }

            // Add ContentSizeFitter
            var fitter = buttonContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = buttonContainer.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        // ============================================
        // PLAY BUTTON - Green glossy
        // ============================================
        SetupButton(
            FindChild(canvas.transform, "ButtonContainer/PlayButton"),
            GREEN_BTN,
            "PLAY",
            72f,
            new Color(1f, 1f, 1f, 1f),
            FontStyles.Bold
        );

        // ============================================
        // LEADERBOARD BUTTON - Purple/Blue glossy
        // ============================================
        SetupButton(
            FindChild(canvas.transform, "ButtonContainer/LeaderboardButton"),
            PURPLE_BTN,
            "LEADERBOARD",
            60f,
            new Color(1f, 1f, 1f, 1f),
            FontStyles.Bold
        );

        // ============================================
        // BEST SCORE PANEL - Blue glossy
        // ============================================
        var bestScorePanel = FindChild(canvas.transform, "ButtonContainer/BestScorePanel");
        if (bestScorePanel != null)
        {
            var panelImage = bestScorePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                var blueSprite = LoadSprite(TEAL_BTN);
                if (blueSprite != null)
                {
                    panelImage.sprite = blueSprite;
                    panelImage.type = Image.Type.Sliced;
                    panelImage.color = Color.white;
                }
            }

            var le = bestScorePanel.GetComponent<LayoutElement>();
            if (le == null) le = bestScorePanel.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 80;
            le.minHeight = 80;

            // Trophy icon text
            var trophyText = FindChild(bestScorePanel, "TrophyIcon");
            if (trophyText != null)
            {
                // Replace TMP text with Image if possible
                var trophySprite = LoadSprite(TROPHY_ICON);
                if (trophySprite == null) trophySprite = LoadSprite(TROPHY_BIG);
                
                if (trophySprite != null)
                {
                    // Remove unnecessary CanvasRenderer to avoid conflict
                    var existingCR = trophyText.GetComponent<CanvasRenderer>();
                    
                    // Remove TMP text first since we're replacing with image
                    var tmpComponent = trophyText.GetComponent<TextMeshProUGUI>();
                    if (tmpComponent != null)
                    {
                        Object.DestroyImmediate(tmpComponent);
                    }
                    
                    // Add Image component
                    var tImg = trophyText.GetComponent<Image>();
                    if (tImg == null) tImg = trophyText.gameObject.AddComponent<Image>();
                    tImg.sprite = trophySprite;
                    tImg.preserveAspect = true;
                    tImg.raycastTarget = false;
                    tImg.color = Color.white;
                }
                else
                {
                    // Fallback: use text star if no trophy sprite
                    var tmp = trophyText.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = "*";
                        tmp.fontSize = 40;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color(1f, 0.85f, 0.1f, 1f);
                    }
                }
                
                var tRect = trophyText.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0.02f, 0.1f);
                tRect.anchorMax = new Vector2(0.18f, 0.9f);
                tRect.offsetMin = Vector2.zero;
                tRect.offsetMax = Vector2.zero;
            }

            // Label
            var labelObj = FindChild(bestScorePanel, "Label");
            if (labelObj != null)
            {
                var tmp = labelObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "BEST SCORE";
                    tmp.fontSize = 28;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    tmp.enableAutoSizing = false;
                    tmp.outlineWidth = 0.1f;
                    tmp.outlineColor = new Color32(0, 0, 0, 100);
                }
                var lRect = labelObj.GetComponent<RectTransform>();
                lRect.anchorMin = new Vector2(0.18f, 0f);
                lRect.anchorMax = new Vector2(0.65f, 1f);
                lRect.offsetMin = Vector2.zero;
                lRect.offsetMax = Vector2.zero;
            }

            // Score Number
            var scoreObj = FindChild(bestScorePanel, "ScoreNumber");
            if (scoreObj != null)
            {
                var tmp = scoreObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "0";
                    tmp.fontSize = 42;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(1f, 1f, 0.7f, 1f);
                    tmp.enableAutoSizing = false;
                    tmp.outlineWidth = 0.15f;
                    tmp.outlineColor = new Color32(0, 0, 0, 128);
                }
                var sRect = scoreObj.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.6f, 0f);
                sRect.anchorMax = new Vector2(0.98f, 1f);
                sRect.offsetMin = Vector2.zero;
                sRect.offsetMax = Vector2.zero;
            }

            // Shadow
            var shadow = bestScorePanel.GetComponent<Shadow>();
            if (shadow == null) shadow = bestScorePanel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(3, -3);
        }

        // ============================================
        // QUIT BUTTON - Red glossy
        // ============================================
        SetupButton(
            FindChild(canvas.transform, "ButtonContainer/QuitButton"),
            RED_BTN,
            "QUIT",
            60f,
            new Color(1f, 1f, 1f, 1f),
            FontStyles.Bold
        );

        // ============================================
        // LEADERBOARD PANEL - Cartoon popup
        // ============================================
        var leaderboardPanel = FindChild(canvas.transform, "LeaderboardPanel");
        if (leaderboardPanel != null)
        {
            // Dim overlay behind panel
            var overlayImage = leaderboardPanel.GetComponent<Image>();
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0f, 0f, 0f, 0.6f);
                overlayImage.sprite = null;
            }

            var lpRect = leaderboardPanel.GetComponent<RectTransform>();
            lpRect.anchorMin = Vector2.zero;
            lpRect.anchorMax = Vector2.one;
            lpRect.offsetMin = Vector2.zero;
            lpRect.offsetMax = Vector2.zero;

            // Panel body
            var panelBody = FindChild(leaderboardPanel, "PanelBody");
            if (panelBody != null)
            {
                var bodyImage = panelBody.GetComponent<Image>();
                if (bodyImage != null)
                {
                    var popupSprite = LoadSprite(POPUP_BG);
                    if (popupSprite == null) popupSprite = LoadSprite(POPUP_BLUE);
                    if (popupSprite != null)
                    {
                        bodyImage.sprite = popupSprite;
                        bodyImage.type = Image.Type.Sliced;
                        bodyImage.color = Color.white;
                    }
                }

                var bodyRect = panelBody.GetComponent<RectTransform>();
                bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
                bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
                bodyRect.sizeDelta = new Vector2(600, 500);
                bodyRect.anchoredPosition = Vector2.zero;

                // Title
                var titleObj = FindChild(panelBody, "Title");
                if (titleObj != null)
                {
                    var tmp = titleObj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = "TOP SCORES";
                        tmp.fontSize = 42;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color(1f, 0.85f, 0.1f, 1f);
                        tmp.enableAutoSizing = false;
                        tmp.outlineWidth = 0.2f;
                        tmp.outlineColor = new Color32(80, 40, 0, 255);
                    }
                    var tRect = titleObj.GetComponent<RectTransform>();
                    tRect.anchorMin = new Vector2(0, 0.82f);
                    tRect.anchorMax = new Vector2(1, 1f);
                    tRect.offsetMin = new Vector2(20, 0);
                    tRect.offsetMax = new Vector2(-20, -10);
                }

                // Score list
                var scoreList = FindChild(panelBody, "ScoreList");
                if (scoreList != null)
                {
                    var slRect = scoreList.GetComponent<RectTransform>();
                    slRect.anchorMin = new Vector2(0.1f, 0.15f);
                    slRect.anchorMax = new Vector2(0.9f, 0.80f);
                    slRect.offsetMin = Vector2.zero;
                    slRect.offsetMax = Vector2.zero;

                    var slLayout = scoreList.GetComponent<VerticalLayoutGroup>();
                    if (slLayout != null)
                    {
                        slLayout.spacing = 8;
                        slLayout.childAlignment = TextAnchor.MiddleCenter;
                        slLayout.childControlWidth = true;
                        slLayout.childControlHeight = true;
                        slLayout.childForceExpandWidth = true;
                        slLayout.childForceExpandHeight = true;
                    }

                    // Style each score row
                    for (int i = 0; i < 5; i++)
                    {
                        var row = FindChild(scoreList, $"ScoreRow_{i}");
                        if (row != null)
                        {
                            var tmp = row.GetComponent<TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                tmp.text = $"{i + 1}. ---";
                                tmp.fontSize = 32;
                                tmp.fontStyle = FontStyles.Bold;
                                tmp.alignment = TextAlignmentOptions.Center;
                                tmp.color = i == 0 ? new Color(1f, 0.84f, 0f) : 
                                           i == 1 ? new Color(0.75f, 0.75f, 0.75f) :
                                           i == 2 ? new Color(0.8f, 0.5f, 0.2f) :
                                           Color.white;
                                tmp.outlineWidth = 0.1f;
                                tmp.outlineColor = new Color32(0, 0, 0, 80);
                            }
                        }
                    }
                }

                // Close button
                var closeBtn = FindChild(panelBody, "CloseButton");
                if (closeBtn != null)
                {
                    var cbImage = closeBtn.GetComponent<Image>();
                    if (cbImage != null)
                    {
                        var redCircle = LoadSprite(RED_CIRCLE);
                        if (redCircle != null)
                        {
                            cbImage.sprite = redCircle;
                            cbImage.type = Image.Type.Simple;
                            cbImage.preserveAspect = true;
                            cbImage.color = Color.white;
                        }
                    }

                    var cbRect = closeBtn.GetComponent<RectTransform>();
                    cbRect.anchorMin = new Vector2(1, 1);
                    cbRect.anchorMax = new Vector2(1, 1);
                    cbRect.anchoredPosition = new Vector2(20, 20);
                    cbRect.sizeDelta = new Vector2(60, 60);

                    // Close button text
                    var closeTxt = FindChild(closeBtn, "Text");
                    if (closeTxt != null)
                    {
                        var tmp = closeTxt.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.text = "X";
                            tmp.fontSize = 36;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = Color.white;
                        }
                        var ctRect = closeTxt.GetComponent<RectTransform>();
                        ctRect.anchorMin = Vector2.zero;
                        ctRect.anchorMax = Vector2.one;
                        ctRect.offsetMin = Vector2.zero;
                        ctRect.offsetMax = Vector2.zero;
                    }
                }
            }
        }

        // ============================================
        // WIRE MANAGERS
        // ============================================
        WireMainMenuManager(canvas.transform);
        WireAudioManager();

        // ============================================
        // DELETE City_Trash_Distributions (shouldn't be in menu scene)
        // ============================================
        var trash = GameObject.Find("City_Trash_Distributions");
        if (trash != null)
        {
            Undo.DestroyObjectImmediate(trash);
            Debug.Log("[MainMenuSetup] Removed City_Trash_Distributions from menu scene.");
        }

        // ============================================
        // ENSURE BUILD SETTINGS
        // ============================================
        EnsureBuildSettings();

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[MainMenuSetup] ===== Main Menu setup complete! Scene saved. =====");
    }

    private static void EnsureBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] requiredScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/UrbanRecyclingCity.unity"
        };

        bool changed = false;
        foreach (string scenePath in requiredScenes)
        {
            if (!File.Exists(GetFullPath(scenePath)))
            {
                Debug.LogWarning($"[MainMenuSetup] Scene not found: {scenePath}");
                continue;
            }

            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    if (!s.enabled)
                    {
                        s.enabled = true;
                        changed = true;
                    }
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                changed = true;
                Debug.Log($"[MainMenuSetup] Added {scenePath} to Build Settings.");
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[MainMenuSetup] Build Settings updated.");
        }
    }

    private static void SetupButton(Transform buttonTransform, string spritePath, string text, float fontSize, Color textColor, FontStyles style)
    {
        if (buttonTransform == null) return;

        var image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            var sprite = LoadSprite(spritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                image.pixelsPerUnitMultiplier = 1f;
            }
        }

        // Set preferred height via LayoutElement
        var le = buttonTransform.GetComponent<LayoutElement>();
        if (le == null) le = buttonTransform.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 85;
        le.minHeight = 85;

        // Button transition
        var button = buttonTransform.GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        // Shadow
        var shadow = buttonTransform.GetComponent<Shadow>();
        if (shadow == null) shadow = buttonTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(4, -4);

        // Text child
        var textTransform = FindChild(buttonTransform, "Text");
        if (textTransform != null)
        {
            var tmp = textTransform.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.fontStyle = style;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = textColor;
                tmp.enableAutoSizing = false;

                // Outline for cartoon effect
                tmp.outlineWidth = 0.15f;
                tmp.outlineColor = new Color32(0, 0, 0, 128);
            }

            var tRect = textTransform.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(10, 5);
            tRect.offsetMax = new Vector2(-10, -5);
        }
    }

    private static void WireMainMenuManager(Transform canvasTransform)
    {
        var managersObj = GameObject.Find("MenuManagers");
        if (managersObj == null) return;

        var mmm = managersObj.GetComponent<MainMenuManager>();
        if (mmm == null) return;

        // Wire buttons
        var playBtn = FindChild(canvasTransform, "ButtonContainer/PlayButton");
        if (playBtn != null) mmm.playButton = playBtn.GetComponent<Button>();

        var lbBtn = FindChild(canvasTransform, "ButtonContainer/LeaderboardButton");
        if (lbBtn != null) mmm.leaderboardButton = lbBtn.GetComponent<Button>();

        var quitBtn = FindChild(canvasTransform, "ButtonContainer/QuitButton");
        if (quitBtn != null) mmm.quitButton = quitBtn.GetComponent<Button>();

        // Wire leaderboard panel
        var lbPanel = FindChild(canvasTransform, "LeaderboardPanel");
        if (lbPanel != null) mmm.leaderboardPanel = lbPanel.gameObject;

        // Wire close button
        var closeBtn = FindChild(canvasTransform, "LeaderboardPanel/PanelBody/CloseButton");
        if (closeBtn != null) mmm.closeLeaderboardButton = closeBtn.GetComponent<Button>();

        // Wire leaderboard score texts
        var scoreList = FindChild(canvasTransform, "LeaderboardPanel/PanelBody/ScoreList");
        if (scoreList != null)
        {
            mmm.leaderboardScoreTexts = new TextMeshProUGUI[5];
            for (int i = 0; i < 5; i++)
            {
                var row = FindChild(scoreList, $"ScoreRow_{i}");
                if (row != null) mmm.leaderboardScoreTexts[i] = row.GetComponent<TextMeshProUGUI>();
            }
        }

        // Wire canvas group
        mmm.mainCanvasGroup = canvasTransform.GetComponent<CanvasGroup>();

        // Wire HighScoreDisplay
        var bestScore = FindChild(canvasTransform, "ButtonContainer/BestScorePanel");
        if (bestScore != null)
        {
            var hsd = bestScore.GetComponent<HighScoreDisplay>();
            if (hsd != null)
            {
                var scoreNum = FindChild(bestScore, "ScoreNumber");
                if (scoreNum != null) hsd.bestScoreText = scoreNum.GetComponent<TextMeshProUGUI>();

                var label = FindChild(bestScore, "Label");
                if (label != null) hsd.labelText = label.GetComponent<TextMeshProUGUI>();
            }
        }

        EditorUtility.SetDirty(mmm);
        Debug.Log("[MainMenuSetup] MainMenuManager wired.");
    }

    private static void WireAudioManager()
    {
        var audioObj = GameObject.Find("MenuAudio");
        if (audioObj == null) return;

        var mam = audioObj.GetComponent<MenuAudioManager>();
        if (mam == null) return;

        // SFX source is on the MenuAudio object itself
        var sfxSource = audioObj.GetComponent<AudioSource>();
        if (sfxSource != null) mam.sfxSource = sfxSource;

        // Music source is on the child
        var musicChild = audioObj.transform.Find("MusicSource");
        if (musicChild != null)
        {
            var musicSource = musicChild.GetComponent<AudioSource>();
            if (musicSource != null)
            {
                mam.musicSource = musicSource;
                musicSource.loop = true;
                musicSource.volume = 0.3f;
                musicSource.playOnAwake = false;
            }
        }

        // Load audio clips
        var hoverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HOVER_SOUND);
        if (hoverClip != null) mam.hoverSound = hoverClip;

        var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SOUND);
        if (clickClip != null) mam.clickSound = clickClip;

        // Background music
        var menuMusicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MENU_MUSIC);
        if (menuMusicClip != null)
        {
            mam.menuMusic = menuMusicClip;
            Debug.Log("[MainMenuSetup] Menu music clip assigned (main-menu.mp3).");
        }
        else
        {
            Debug.LogWarning($"[MainMenuSetup] Menu music not found at: {MENU_MUSIC}");
        }

        mam.musicVolume = 0.3f;
        mam.sfxVolume = 0.7f;

        EditorUtility.SetDirty(mam);
        Debug.Log("[MainMenuSetup] MenuAudioManager wired with audio clips.");
    }

    // ============================================
    // HELPER METHODS
    // ============================================
    
    private static void EnsureSpriteImport(string path, FilterMode filterMode = FilterMode.Bilinear, int maxSize = 1024, int spriteMode = 1)
    {
        if (!File.Exists(GetFullPath(path)))
        {
            Debug.LogWarning($"[MainMenuSetup] Sprite not found: {path}");
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        
        bool changed = false;
        
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }
        if (importer.spriteImportMode != (SpriteImportMode)spriteMode)
        {
            importer.spriteImportMode = (SpriteImportMode)spriteMode;
            changed = true;
        }
        if (importer.filterMode != filterMode)
        {
            importer.filterMode = filterMode;
            changed = true;
        }
        if (importer.maxTextureSize != maxSize)
        {
            importer.maxTextureSize = maxSize;
            changed = true;
        }
        
        // Enable 9-slice for buttons/panels if border not set
        if (importer.spriteBorder == Vector4.zero && 
            !path.Contains("Circle") && !path.Contains("Icon") && 
            !path.Contains("Cup") && !path.Contains("Crown") && 
            !path.Contains("Star") && !path.Contains("background-image"))
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                int border = Mathf.Max(tex.width, tex.height) / 4;
                importer.spriteBorder = new Vector4(border, border, border, border);
                changed = true;
            }
        }
        
        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static string GetFullPath(string assetPath)
    {
        return Path.Combine(Application.dataPath, "..", assetPath);
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"[MainMenuSetup] Could not load sprite: {path}");
        }
        return sprite;
    }

    private static T FindByName<T>(string name) where T : Component
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == name)
            {
                var comp = root.GetComponent<T>();
                if (comp != null) return comp;
            }
            var found = root.GetComponentsInChildren<T>(true);
            foreach (var f in found)
            {
                if (f.gameObject.name == name) return f;
            }
        }
        return null;
    }

    private static Transform FindChild(Transform parent, string path)
    {
        if (parent == null) return null;
        
        string[] parts = path.Split('/');
        Transform current = parent;
        
        foreach (string part in parts)
        {
            Transform found = null;
            for (int i = 0; i < current.childCount; i++)
            {
                if (current.GetChild(i).name == part)
                {
                    found = current.GetChild(i);
                    break;
                }
            }
            
            if (found == null) return null;
            current = found;
        }
        
        return current;
    }
}
