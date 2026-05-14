using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class GameplayUIBuilder
{
    [MenuItem("Tools/Build Gameplay UI")]
    public static void BuildUI()
    {
        // Find or Create Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("GameplayCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvas.gameObject.name = "GameplayCanvas";
            var scaler = canvas.GetComponent<CanvasScaler>();
            if(scaler) {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }
        }

        // Clean up old HUD if exists
        Transform oldHud = canvas.transform.Find("HUD");
        if (oldHud != null) GameObject.DestroyImmediate(oldHud.gameObject);
        Transform oldInt = canvas.transform.Find("InteractionUI");
        if (oldInt != null) GameObject.DestroyImmediate(oldInt.gameObject);
        Transform oldEff = canvas.transform.Find("Effects");
        if (oldEff != null) GameObject.DestroyImmediate(oldEff.gameObject);

        // Create main HUD container
        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvas.transform, false);
        RectTransform hudRt = hud.AddComponent<RectTransform>();
        hudRt.anchorMin = Vector2.zero;
        hudRt.anchorMax = Vector2.one;
        hudRt.sizeDelta = Vector2.zero;

        // Load Assets
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Shapes/Shapes/Rectangle/Rounded Rectangle - 256px.png");
        Sprite scoreIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Stars/Star Yellow.png");
        Sprite timerIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Time/Clock - Yellow.png");
        Sprite bagIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Storage/Bag.png");
        Sprite handIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Other/Hand.png");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/GUIPackCartoon/Demo/Fonts/LilitaOne - Regular SDF.asset");

        if (font == null) Debug.LogError("Font not found!");
        if (panelSprite == null) Debug.LogError("Panel sprite not found!");

        // 1. Score UI (Top Left)
        CreatePanel("ScoreUI", hud.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(280, 100), new Vector3(160, -70, 0), panelSprite, font, scoreIcon, "0", new Color(1f, 0.95f, 0.6f, 1f));

        // 2. Timer UI (Top Center)
        CreatePanel("TimerUI", hud.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(250, 100), new Vector3(0, -70, 0), panelSprite, font, timerIcon, "02:30", new Color(1f, 1f, 1f, 1f));

        // 3. Current Item UI (Bottom Right)
        CreatePanel("CurrentItemUI", hud.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(350, 120), new Vector3(-200, 90, 0), panelSprite, font, bagIcon, "No Item", new Color(0.85f, 0.95f, 1f, 1f));

        // 4. Interaction UI (Bottom Center)
        GameObject interactionUI = new GameObject("InteractionUI");
        interactionUI.transform.SetParent(canvas.transform, false);
        RectTransform intRt = interactionUI.AddComponent<RectTransform>();
        intRt.anchorMin = new Vector2(0.5f, 0);
        intRt.anchorMax = new Vector2(0.5f, 0);
        intRt.sizeDelta = new Vector2(450, 90);
        intRt.anchoredPosition = new Vector2(0, 200);

        Image intBg = interactionUI.AddComponent<Image>();
        intBg.sprite = panelSprite;
        intBg.type = Image.Type.Sliced;
        intBg.pixelsPerUnitMultiplier = 2f;
        intBg.color = new Color(0.95f, 0.95f, 0.95f, 1f); // White/Gray tint

        // Outline for interaction UI
        Outline outline = interactionUI.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        outline.effectDistance = new Vector2(2, -2);

        GameObject intIconGo = new GameObject("Icon");
        intIconGo.transform.SetParent(interactionUI.transform, false);
        RectTransform intIconRt = intIconGo.AddComponent<RectTransform>();
        intIconRt.anchorMin = new Vector2(0, 0.5f);
        intIconRt.anchorMax = new Vector2(0, 0.5f);
        intIconRt.sizeDelta = new Vector2(70, 70);
        intIconRt.anchoredPosition = new Vector2(50, 0);
        Image iIcon = intIconGo.AddComponent<Image>();
        iIcon.sprite = handIcon;
        iIcon.preserveAspect = true;

        GameObject intTextGo = new GameObject("Text");
        intTextGo.transform.SetParent(interactionUI.transform, false);
        RectTransform intTextRt = intTextGo.AddComponent<RectTransform>();
        intTextRt.anchorMin = new Vector2(0, 0);
        intTextRt.anchorMax = new Vector2(1, 1);
        intTextRt.offsetMin = new Vector2(100, 0);
        intTextRt.offsetMax = new Vector2(-20, 0);
        TextMeshProUGUI intText = intTextGo.AddComponent<TextMeshProUGUI>();
        intText.font = font;
        intText.text = "Press E to Pick Up";
        intText.fontSize = 36;
        intText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        intText.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.CenterGeoAligned;
        intText.enableWordWrapping = false;
        intText.fontStyle = FontStyles.Bold;

        // Effects Container
        GameObject effects = new GameObject("Effects");
        effects.transform.SetParent(canvas.transform, false);

        Debug.Log("Gameplay UI Built successfully!");
    }

    private static void CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector3 pos, Sprite bgSprite, TMP_FontAsset font, Sprite iconSprite, string defaultText, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image bg = panel.AddComponent<Image>();
        bg.sprite = bgSprite;
        bg.type = Image.Type.Sliced;
        bg.pixelsPerUnitMultiplier = 2f;
        bg.color = bgColor;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.15f);
        outline.effectDistance = new Vector2(3, -3);

        // Icon
        GameObject iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(panel.transform, false);
        RectTransform iconRt = iconGo.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 0.5f);
        iconRt.anchorMax = new Vector2(0, 0.5f);
        iconRt.sizeDelta = new Vector2(size.y * 0.7f, size.y * 0.7f);
        iconRt.anchoredPosition = new Vector2(size.y * 0.5f, 0);
        Image icon = iconGo.AddComponent<Image>();
        icon.sprite = iconSprite;
        icon.preserveAspect = true;

        // Text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panel.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(size.y * 0.9f, 0);
        textRt.offsetMax = new Vector2(-20, 0);
        
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = defaultText;
        tmp.fontSize = size.y * 0.45f;
        tmp.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
        tmp.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.CenterGeoAligned;
        tmp.fontStyle = FontStyles.Bold;
        
        // Add subtle shadow to text
        tmp.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        tmp.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 0.5f);
        tmp.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -0.5f);
        tmp.fontSharedMaterial.SetFloat("_UnderlayDilate", 0f);
        tmp.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0,0,0,0.3f));
    }
}
