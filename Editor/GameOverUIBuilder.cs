using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.EventSystems;

public static class GameOverUIBuilder
{
    [MenuItem("Tools/Build Game Over UI")]
    public static void BuildUI()
    {
        // 0. CREATE EVENTSYSTEM IF MISSING (CRITICAL FOR BUTTON CLICKS)
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
            Debug.Log("[GameOverUIBuilder] Created missing EventSystem.");
        }

        // 1. Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("GameCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            if (canvas.GetComponent<GraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        // Cleanup old structures for idempotency
        Transform oldSystem = canvas.transform.parent != null ? canvas.transform.parent.Find("GameOverSystem") : GameObject.Find("GameOverSystem")?.transform;
        if (oldSystem != null) GameObject.DestroyImmediate(oldSystem.gameObject);
        
        Transform oldOverlay = canvas.transform.Find("GameOverOverlay");
        if (oldOverlay != null) GameObject.DestroyImmediate(oldOverlay.gameObject);

        // 2. Load required assets
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/background.png");
        if (panelSprite == null)
            panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Shapes/Shapes/Rectangle/Rounded Rectangle - 256px.png");
        
        Sprite restartBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Green.png");
        Sprite menuBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Orange.png");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GUIPackCartoon/Demo/Fonts/LilitaOne - Regular SDF.asset");

        if (font == null || panelSprite == null)
        {
            Debug.LogError("[GameOverUIBuilder] Missing Cartoon GUI assets!");
            return;
        }

        // Fallback sprites
        if (restartBtnSprite == null)
            restartBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Blue.png");
        if (menuBtnSprite == null)
            menuBtnSprite = restartBtnSprite;

        // ============================================
        // 3. Dark Background Overlay
        // ============================================
        GameObject overlayGo = new GameObject("GameOverOverlay");
        overlayGo.transform.SetParent(canvas.transform, false);
        overlayGo.transform.SetAsLastSibling();

        RectTransform overlayRt = overlayGo.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;

        Image overlayImg = overlayGo.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.8f);
        overlayImg.raycastTarget = true;

        // ============================================
        // 4. Main Popup Panel
        // ============================================
        GameObject panelGo = new GameObject("PopupPanel");
        panelGo.transform.SetParent(overlayGo.transform, false);
        RectTransform panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(750f, 550f);
        panelRt.anchoredPosition = Vector2.zero;

        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.sprite = panelSprite;
        panelImg.type = Image.Type.Sliced;
        panelImg.pixelsPerUnitMultiplier = 2f;
        panelImg.color = Color.white;

        Shadow panelShadow = panelGo.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        panelShadow.effectDistance = new Vector2(5, -5);

        // ============================================
        // 5. Title: "GAME OVER"
        // ============================================
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(panelGo.transform, false);
        RectTransform titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.72f);
        titleRt.anchorMax = new Vector2(1f, 0.98f);
        titleRt.offsetMin = new Vector2(30, 0);
        titleRt.offsetMax = new Vector2(-30, 0);

        TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.font = font;
        titleText.text = "GAME OVER";
        titleText.fontSize = 100f;
        titleText.color = new Color(1f, 0.8f, 0.1f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 40;
        titleText.fontSizeMax = 100;
        
        titleText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        titleText.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 1f);
        titleText.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -1f);
        titleText.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.6f));

        // ============================================
        // 6. Final Score Text
        // ============================================
        GameObject scoreGo = new GameObject("FinalScoreText");
        scoreGo.transform.SetParent(panelGo.transform, false);
        RectTransform scoreRt = scoreGo.AddComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(0f, 0.35f);
        scoreRt.anchorMax = new Vector2(1f, 0.72f);
        scoreRt.offsetMin = new Vector2(40, 0);
        scoreRt.offsetMax = new Vector2(-40, 0);

        TextMeshProUGUI scoreText = scoreGo.AddComponent<TextMeshProUGUI>();
        scoreText.font = font;
        scoreText.text = "FINAL SCORE\n0";
        scoreText.fontSize = 70f;
        scoreText.lineSpacing = -10f;
        scoreText.color = new Color(0.18f, 0.18f, 0.22f, 1f);
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.raycastTarget = false;
        scoreText.enableAutoSizing = true;
        scoreText.fontSizeMin = 30;
        scoreText.fontSizeMax = 70;

        // ============================================
        // 7. Button Container (vertical layout for proper spacing)
        // ============================================
        GameObject btnContainer = new GameObject("ButtonContainer");
        btnContainer.transform.SetParent(panelGo.transform, false);
        RectTransform btnContRt = btnContainer.AddComponent<RectTransform>();
        btnContRt.anchorMin = new Vector2(0.15f, 0.03f);
        btnContRt.anchorMax = new Vector2(0.85f, 0.35f);
        btnContRt.offsetMin = Vector2.zero;
        btnContRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = btnContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.padding = new RectOffset(10, 10, 5, 5);

        // ============================================
        // 8. Restart Button (Green)
        // ============================================
        Button restartBtn = CreateButton(
            btnContainer.transform, "RestartButton", restartBtnSprite, 
            "RESTART", font, 55f
        );

        // ============================================
        // 9. Main Menu Button (Orange)
        // ============================================
        Button menuBtn = CreateButton(
            btnContainer.transform, "MainMenuButton", menuBtnSprite,
            "MAIN MENU", font, 48f
        );

        // ============================================
        // 10. Wire GameOverManager
        // ============================================
        GameObject systemGo = new GameObject("GameOverSystem");
        GameOverManager manager = systemGo.AddComponent<GameOverManager>();
        manager.gameOverPanel = overlayGo;
        manager.finalScoreText = scoreText;
        manager.restartButton = restartBtn;
        manager.mainMenuButton = menuBtn;

        Debug.Log("[GameOverUIBuilder] Game Over UI built successfully with proper button layout!");
    }

    private static Button CreateButton(Transform parent, string name, Sprite sprite, string text, TMP_FontAsset font, float fontSize)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);

        Image btnImg = btnGo.AddComponent<Image>();
        btnImg.sprite = sprite;
        btnImg.type = Image.Type.Sliced;
        btnImg.pixelsPerUnitMultiplier = 2f;
        btnImg.raycastTarget = true;

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.95f, 0.95f, 0.98f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.85f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.1f;
        btn.colors = cb;

        Shadow shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
        shadow.effectDistance = new Vector2(3, -3);

        // Button text
        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.anchoredPosition = new Vector2(0f, 3f);

        TextMeshProUGUI btnText = txtGo.AddComponent<TextMeshProUGUI>();
        btnText.font = font;
        btnText.text = text;
        btnText.fontSize = fontSize;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;
        btnText.raycastTarget = false;
        btnText.enableAutoSizing = true;
        btnText.fontSizeMin = 24;
        btnText.fontSizeMax = fontSize;

        return btn;
    }
}
