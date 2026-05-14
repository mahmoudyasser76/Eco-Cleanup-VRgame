using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Editor utility that builds the complete Main Menu UI hierarchy programmatically.
/// Run via menu: Tools > Build Main Menu UI
/// </summary>
public class MainMenuBuilder
{
    // Asset paths
    private const string BG_IMAGE_PATH = "Assets/main-menu background-image.png";
    private const string GREEN_BTN_PATH = "Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Green.psd";
    private const string BLUE_BTN_PATH = "Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Blue.psd";
    private const string PURPLE_BTN_PATH = "Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Purple.psd";
    private const string RED_BTN_PATH = "Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Red.psd";
    private const string TEAL_BTN_PATH = "Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Blue (Teal).psd";
    private const string PANEL_SHAPE_PATH = "Assets/GUIPackCartoon/Sources/Shapes/Rectangle/Square - Cartoon.psd";

    // Audio paths
    private const string HOVER_SOUND_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Pop Interface 02.wav";
    private const string CLICK_SOUND_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Interface Click 13-1.wav";

    [MenuItem("Tools/Build Main Menu UI")]
    public static void BuildMainMenuUI()
    {
        // Ensure we're in the MainMenu scene
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "MainMenu")
        {
            Debug.LogError("[MainMenuBuilder] Please open the MainMenu scene first!");
            return;
        }

        // Clean up existing UI (except camera and light)
        foreach (var root in activeScene.GetRootGameObjects())
        {
            if (root.name != "Main Camera" && root.name != "Directional Light")
            {
                Object.DestroyImmediate(root);
            }
        }

        // ═══════════════════════════════════════════
        // 1. MAIN CANVAS
        // ═══════════════════════════════════════════
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add CanvasGroup for fade-in
        CanvasGroup mainCG = canvasObj.AddComponent<CanvasGroup>();

        // ═══════════════════════════════════════════
        // 2. BACKGROUND IMAGE
        // ═══════════════════════════════════════════
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_IMAGE_PATH);
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.preserveAspect = true;
            bgImage.type = Image.Type.Simple;
        }
        else
        {
            Debug.LogWarning("[MainMenuBuilder] Background sprite not found at: " + BG_IMAGE_PATH);
            bgImage.color = new Color(0.2f, 0.6f, 0.9f);
        }

        // ═══════════════════════════════════════════
        // 3. BUTTON CONTAINER (centered lower portion)
        // ═══════════════════════════════════════════
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform btnContRect = buttonContainer.AddComponent<RectTransform>();
        btnContRect.anchorMin = new Vector2(0.5f, 0.05f);
        btnContRect.anchorMax = new Vector2(0.5f, 0.55f);
        btnContRect.pivot = new Vector2(0.5f, 0.5f);
        btnContRect.sizeDelta = new Vector2(420, 500);
        btnContRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = buttonContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        // Load button sprites
        Sprite greenSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GREEN_BTN_PATH);
        Sprite blueSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BLUE_BTN_PATH);
        Sprite purpleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PURPLE_BTN_PATH);
        Sprite redSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RED_BTN_PATH);
        Sprite tealSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TEAL_BTN_PATH);
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PANEL_SHAPE_PATH);

        // ═══════════════════════════════════════════
        // 4. PLAY BUTTON
        // ═══════════════════════════════════════════
        GameObject playBtn = CreateCartoonButton("PlayButton", buttonContainer.transform,
            greenSprite, "▶  PLAY", 80f, new Color(0.2f, 0.75f, 0.2f));

        // ═══════════════════════════════════════════
        // 5. LEADERBOARD BUTTON
        // ═══════════════════════════════════════════
        GameObject leaderboardBtn = CreateCartoonButton("LeaderboardButton", buttonContainer.transform,
            purpleSprite != null ? purpleSprite : blueSprite, "🏆  LEADERBOARD", 80f, new Color(0.4f, 0.3f, 0.8f));

        // ═══════════════════════════════════════════
        // 6. BEST SCORE PANEL
        // ═══════════════════════════════════════════
        GameObject bestScorePanel = CreateBestScorePanel(buttonContainer.transform, tealSprite != null ? tealSprite : blueSprite);

        // ═══════════════════════════════════════════
        // 7. QUIT BUTTON
        // ═══════════════════════════════════════════
        GameObject quitBtn = CreateCartoonButton("QuitButton", buttonContainer.transform,
            redSprite, "⏻  QUIT", 80f, new Color(0.85f, 0.2f, 0.2f));

        // ═══════════════════════════════════════════
        // 8. LEADERBOARD POPUP PANEL (hidden)
        // ═══════════════════════════════════════════
        GameObject leaderboardPanel = CreateLeaderboardPanel(canvasObj.transform, panelSprite);

        // ═══════════════════════════════════════════
        // 9. EVENT SYSTEM
        // ═══════════════════════════════════════════
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ═══════════════════════════════════════════
        // 10. MANAGERS GAMEOBJECT
        // ═══════════════════════════════════════════
        GameObject managers = new GameObject("MenuManagers");

        // MainMenuManager
        MainMenuManager mmm = managers.AddComponent<MainMenuManager>();
        mmm.playButton = playBtn.GetComponent<Button>();
        mmm.leaderboardButton = leaderboardBtn.GetComponent<Button>();
        mmm.quitButton = quitBtn.GetComponent<Button>();
        mmm.leaderboardPanel = leaderboardPanel;
        mmm.mainCanvasGroup = mainCG;
        mmm.gameplaySceneName = "UrbanRecyclingCity";

        // Find close button in leaderboard panel
        Transform closeBtnTransform = leaderboardPanel.transform.Find("CloseButton");
        if (closeBtnTransform != null)
            mmm.closeLeaderboardButton = closeBtnTransform.GetComponent<Button>();

        // Wire up leaderboard score texts
        TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
        Transform scoreListTransform = leaderboardPanel.transform.Find("ScoreList");
        if (scoreListTransform != null)
        {
            for (int i = 0; i < 5; i++)
            {
                Transform scoreRow = scoreListTransform.Find("ScoreRow_" + i);
                if (scoreRow != null)
                    scoreTexts[i] = scoreRow.GetComponent<TextMeshProUGUI>();
            }
        }
        mmm.leaderboardScoreTexts = scoreTexts;

        // LeaderboardManager
        managers.AddComponent<LeaderboardManager>();

        // HighScoreDisplay on best score panel
        HighScoreDisplay hsd = bestScorePanel.AddComponent<HighScoreDisplay>();
        Transform scoreNumTransform = bestScorePanel.transform.Find("ScoreNumber");
        if (scoreNumTransform != null)
            hsd.bestScoreText = scoreNumTransform.GetComponent<TextMeshProUGUI>();
        Transform labelTransform = bestScorePanel.transform.Find("Label");
        if (labelTransform != null)
            hsd.labelText = labelTransform.GetComponent<TextMeshProUGUI>();

        // ═══════════════════════════════════════════
        // 11. AUDIO
        // ═══════════════════════════════════════════
        GameObject audioObj = new GameObject("MenuAudio");
        MenuAudioManager mam = audioObj.AddComponent<MenuAudioManager>();

        // SFX Source
        AudioSource sfxSource = audioObj.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        mam.sfxSource = sfxSource;

        // Music Source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(audioObj.transform);
        AudioSource musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        mam.musicSource = musicSource;

        // Load audio clips
        AudioClip hoverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HOVER_SOUND_PATH);
        AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SOUND_PATH);
        mam.hoverSound = hoverClip;
        mam.clickSound = clickClip;
        mam.musicVolume = 0.25f;
        mam.sfxVolume = 0.6f;

        // ═══════════════════════════════════════════
        // 12. SCENE TRANSITION MANAGER
        // ═══════════════════════════════════════════
        GameObject transitionObj = new GameObject("SceneTransitionManager");
        transitionObj.AddComponent<SceneTransitionManager>();

        // ═══════════════════════════════════════════
        // 13. Add MenuButtonAnimator to all buttons
        // ═══════════════════════════════════════════
        AddButtonAnimator(playBtn);
        AddButtonAnimator(leaderboardBtn);
        AddButtonAnimator(quitBtn);

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[MainMenuBuilder] ✅ Main Menu UI built successfully!");
    }

    /// <summary>
    /// Creates a cartoon-style button with icon+text layout.
    /// </summary>
    private static GameObject CreateCartoonButton(string name, Transform parent, Sprite buttonSprite, string labelText, float height, Color fallbackColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(400, height);

        Image btnImage = btnObj.AddComponent<Image>();
        if (buttonSprite != null)
        {
            btnImage.sprite = buttonSprite;
            btnImage.type = Image.Type.Sliced;
            btnImage.pixelsPerUnitMultiplier = 1f;
        }
        else
        {
            btnImage.color = fallbackColor;
        }

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        // Remove default color tint transition — we use scale animations instead
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        // Shadow for depth
        Shadow shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.4f);
        shadow.effectDistance = new Vector2(3, -4);

        // Text label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 5);
        textRect.offsetMax = new Vector2(-20, -5);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = labelText;
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;

        // Outline for text
        tmp.outlineWidth = 0.3f;
        tmp.outlineColor = new Color32(0, 0, 0, 180);

        return btnObj;
    }

    /// <summary>
    /// Creates the Best Score display panel.
    /// </summary>
    private static GameObject CreateBestScorePanel(Transform parent, Sprite panelSprite)
    {
        GameObject panel = new GameObject("BestScorePanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 90);

        Image panelImage = panel.AddComponent<Image>();
        if (panelSprite != null)
        {
            panelImage.sprite = panelSprite;
            panelImage.type = Image.Type.Sliced;
        }
        else
        {
            panelImage.color = new Color(0.15f, 0.5f, 0.75f, 0.9f);
        }

        // Add shadow
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.3f);
        shadow.effectDistance = new Vector2(2, -3);

        // Trophy icon text (emoji)
        GameObject trophyObj = new GameObject("TrophyIcon");
        trophyObj.transform.SetParent(panel.transform, false);
        RectTransform trophyRect = trophyObj.AddComponent<RectTransform>();
        trophyRect.anchorMin = new Vector2(0, 0);
        trophyRect.anchorMax = new Vector2(0.2f, 1);
        trophyRect.offsetMin = new Vector2(10, 5);
        trophyRect.offsetMax = new Vector2(0, -5);

        TextMeshProUGUI trophyTMP = trophyObj.AddComponent<TextMeshProUGUI>();
        trophyTMP.text = "🏆";
        trophyTMP.fontSize = 36;
        trophyTMP.alignment = TextAlignmentOptions.Center;

        // Label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(panel.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.2f, 0.5f);
        labelRect.anchorMax = new Vector2(0.65f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = new Vector2(0, -5);

        TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
        labelTMP.text = "BEST SCORE";
        labelTMP.fontSize = 20;
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.color = Color.white;
        labelTMP.alignment = TextAlignmentOptions.Left;
        labelTMP.outlineWidth = 0.2f;
        labelTMP.outlineColor = new Color32(0, 0, 0, 150);

        // Score number
        GameObject scoreObj = new GameObject("ScoreNumber");
        scoreObj.transform.SetParent(panel.transform, false);
        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.2f, 0);
        scoreRect.anchorMax = new Vector2(0.95f, 0.55f);
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = new Vector2(0, 0);

        TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "0";
        scoreTMP.fontSize = 34;
        scoreTMP.fontStyle = FontStyles.Bold;
        scoreTMP.color = new Color(1f, 0.95f, 0.4f);
        scoreTMP.alignment = TextAlignmentOptions.Left;
        scoreTMP.outlineWidth = 0.25f;
        scoreTMP.outlineColor = new Color32(0, 0, 0, 160);

        return panel;
    }

    /// <summary>
    /// Creates the Leaderboard popup panel (hidden by default).
    /// </summary>
    private static GameObject CreateLeaderboardPanel(Transform parent, Sprite panelSprite)
    {
        // Dim overlay
        GameObject overlay = new GameObject("LeaderboardPanel");
        overlay.transform.SetParent(parent, false);
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.6f);

        // Panel body
        GameObject panelBody = new GameObject("PanelBody");
        panelBody.transform.SetParent(overlay.transform, false);
        RectTransform bodyRect = panelBody.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(500, 500);

        Image bodyImage = panelBody.AddComponent<Image>();
        if (panelSprite != null)
        {
            bodyImage.sprite = panelSprite;
            bodyImage.type = Image.Type.Sliced;
        }
        else
        {
            bodyImage.color = new Color(0.15f, 0.25f, 0.55f, 0.95f);
        }

        // Add shadow
        Shadow bodyShadow = panelBody.AddComponent<Shadow>();
        bodyShadow.effectColor = new Color(0, 0, 0, 0.5f);
        bodyShadow.effectDistance = new Vector2(4, -5);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelBody.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.82f);
        titleRect.anchorMax = new Vector2(1, 0.98f);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);

        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "🏆 TOP SCORES 🏆";
        titleTMP.fontSize = 36;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(1f, 0.9f, 0.3f);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.outlineWidth = 0.3f;
        titleTMP.outlineColor = new Color32(0, 0, 0, 200);

        // Score list container
        GameObject scoreList = new GameObject("ScoreList");
        scoreList.transform.SetParent(panelBody.transform, false);
        RectTransform scoreListRect = scoreList.AddComponent<RectTransform>();
        scoreListRect.anchorMin = new Vector2(0.1f, 0.2f);
        scoreListRect.anchorMax = new Vector2(0.9f, 0.8f);
        scoreListRect.offsetMin = Vector2.zero;
        scoreListRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup scoreVLG = scoreList.AddComponent<VerticalLayoutGroup>();
        scoreVLG.spacing = 10;
        scoreVLG.childAlignment = TextAnchor.UpperCenter;
        scoreVLG.childControlWidth = true;
        scoreVLG.childControlHeight = true;
        scoreVLG.childForceExpandWidth = true;
        scoreVLG.childForceExpandHeight = true;

        // Create 5 score rows
        for (int i = 0; i < 5; i++)
        {
            GameObject row = new GameObject("ScoreRow_" + i);
            row.transform.SetParent(scoreList.transform, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(300, 45);

            TextMeshProUGUI rowTMP = row.AddComponent<TextMeshProUGUI>();
            rowTMP.text = $"{i + 1}. ---";
            rowTMP.fontSize = 30;
            rowTMP.fontStyle = FontStyles.Bold;
            rowTMP.color = Color.white;
            rowTMP.alignment = TextAlignmentOptions.Center;
            rowTMP.outlineWidth = 0.2f;
            rowTMP.outlineColor = new Color32(0, 0, 0, 150);
        }

        // Close button
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panelBody.transform, false);
        RectTransform closeRect = closeBtn.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0);
        closeRect.anchorMax = new Vector2(0.5f, 0);
        closeRect.pivot = new Vector2(0.5f, 0);
        closeRect.anchoredPosition = new Vector2(0, 15);
        closeRect.sizeDelta = new Vector2(200, 55);

        Image closeImage = closeBtn.AddComponent<Image>();
        Sprite redSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Sources/Buttons/Rectangles/Orange.psd");
        if (redSprite != null)
        {
            closeImage.sprite = redSprite;
            closeImage.type = Image.Type.Sliced;
        }
        else
        {
            closeImage.color = new Color(0.9f, 0.4f, 0.2f);
        }

        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeImage;
        ColorBlock closeBtnColors = closeBtnComp.colors;
        closeBtnColors.normalColor = Color.white;
        closeBtnColors.highlightedColor = Color.white;
        closeBtnColors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        closeBtnComp.colors = closeBtnColors;

        Shadow closeShadow = closeBtn.AddComponent<Shadow>();
        closeShadow.effectColor = new Color(0, 0, 0, 0.35f);
        closeShadow.effectDistance = new Vector2(2, -3);

        // Close button text
        GameObject closeText = new GameObject("Text");
        closeText.transform.SetParent(closeBtn.transform, false);
        RectTransform closeTextRect = closeText.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = new Vector2(10, 5);
        closeTextRect.offsetMax = new Vector2(-10, -5);

        TextMeshProUGUI closeTMP = closeText.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "CLOSE";
        closeTMP.fontSize = 26;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.color = Color.white;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.outlineWidth = 0.25f;
        closeTMP.outlineColor = new Color32(0, 0, 0, 160);

        // Add animator to close button
        AddButtonAnimator(closeBtn);

        overlay.SetActive(false);
        return overlay;
    }

    /// <summary>
    /// Adds MenuButtonAnimator component to a button.
    /// </summary>
    private static void AddButtonAnimator(GameObject btnObj)
    {
        MenuButtonAnimator animator = btnObj.AddComponent<MenuButtonAnimator>();
        animator.hoverScale = 1.08f;
        animator.pressScale = 0.95f;
        animator.animationSpeed = 12f;
    }
}
