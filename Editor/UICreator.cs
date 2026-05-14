using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UICreator
{
    [MenuItem("Tools/Generate UI")]
    public static void GenerateUI()
    {
        string pathPanel = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Blue.png";
        string pathScoreIcon = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Stars/Star Yellow.png";
        string pathTimerIcon = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Time/Timer.png";
        string pathInvIcon = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Storage/Bag.png";
        string pathFont = "Assets/GUIPackCartoon/Demo/Fonts/LilitaOne - Regular.ttf";

        var sprPanel = AssetDatabase.LoadAssetAtPath<Sprite>(pathPanel);
        var sprScore = AssetDatabase.LoadAssetAtPath<Sprite>(pathScoreIcon);
        var sprTimer = AssetDatabase.LoadAssetAtPath<Sprite>(pathTimerIcon);
        var sprInv = AssetDatabase.LoadAssetAtPath<Sprite>(pathInvIcon);
        var font = AssetDatabase.LoadAssetAtPath<Font>(pathFont);

        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var oldCanvas = GameObject.Find("GameCanvas");
        if (oldCanvas) Object.DestroyImmediate(oldCanvas);

        var oldEvent = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (oldEvent == null) {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var canvasObj = new GameObject("GameCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        var uiManagerObj = new GameObject("UIManager");
        uiManagerObj.transform.SetParent(canvasObj.transform);
        var uiManager = uiManagerObj.AddComponent<UIManager>();

        GameObject CreatePanel(string n, Transform p, Vector2 amin, Vector2 amax, Vector2 piv, Vector2 pos, Vector2 size) {
            var go = new GameObject(n);
            go.transform.SetParent(p, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = piv;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprPanel; img.type = Image.Type.Sliced;
            return go;
        }

        GameObject CreateImage(string n, Transform p, Sprite spr, Vector2 pos, Vector2 size) {
            var go = new GameObject(n);
            go.transform.SetParent(p, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f); rt.anchorMax = new Vector2(0, 0.5f); rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = spr;
            return go;
        }

        GameObject CreateText(string n, Transform p, string txt, int fs, TextAnchor align, Color c) {
            var go = new GameObject(n);
            go.transform.SetParent(p, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = font; t.text = txt; t.fontSize = fs; t.alignment = align; t.color = c;
            var outl = go.AddComponent<Outline>();
            outl.effectColor = new Color(0, 0, 0, 0.5f);
            outl.effectDistance = new Vector2(2, -2);
            return go;
        }

        var hudGrp = new GameObject("HUD").AddComponent<RectTransform>();
        hudGrp.SetParent(canvasObj.transform, false);
        hudGrp.anchorMin = Vector2.zero; hudGrp.anchorMax = Vector2.one;
        hudGrp.offsetMin = Vector2.zero; hudGrp.offsetMax = Vector2.zero;

        // SCORE
        var scoreUI = CreatePanel("ScoreUI", hudGrp, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(50,-50), new Vector2(250,80));
        CreateImage("Icon", scoreUI.transform, sprScore, new Vector2(10,0), new Vector2(90,90));
        var scoreTextGo = CreateText("Text", scoreUI.transform, "0", 48, TextAnchor.MiddleRight, Color.white);
        var scoreTextRt = scoreTextGo.GetComponent<RectTransform>();
        scoreTextRt.offsetMin = new Vector2(100, 0); scoreTextRt.offsetMax = new Vector2(-20, 0);
        uiManager.scoreText = scoreTextGo.GetComponent<Text>();

        // TIMER
        var timerUI = CreatePanel("TimerUI", hudGrp, new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(0,-50), new Vector2(300,90));
        CreateImage("Icon", timerUI.transform, sprTimer, new Vector2(15,0), new Vector2(80,80));
        var timerTextGo = CreateText("Text", timerUI.transform, "02:30", 56, TextAnchor.MiddleCenter, Color.white);
        var timerTextRt = timerTextGo.GetComponent<RectTransform>();
        timerTextRt.offsetMin = new Vector2(90, 0); timerTextRt.offsetMax = new Vector2(-20, 0);
        uiManager.timerText = timerTextGo.GetComponent<Text>();

        // INVENTORY
        var invUI = CreatePanel("InventoryUI", hudGrp, new Vector2(1,0), new Vector2(1,0), new Vector2(1,0), new Vector2(-50,50), new Vector2(350,100));
        var invIconGo = CreateImage("Icon", invUI.transform, sprInv, new Vector2(15,0), new Vector2(90,90));
        var invTextGo = CreateText("Text", invUI.transform, "No Item", 42, TextAnchor.MiddleLeft, Color.white);
        var invTextRt = invTextGo.GetComponent<RectTransform>();
        invTextRt.offsetMin = new Vector2(120, 0); invTextRt.offsetMax = new Vector2(-20, 0);
        uiManager.inventoryIcon = invIconGo.GetComponent<Image>();
        uiManager.inventoryText = invTextGo.GetComponent<Text>();
        uiManager.inventoryIcon.enabled = false;

        // INTERACTION
        var intUI = CreatePanel("InteractionUI", canvasObj.transform, new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,250), new Vector2(450,80));
        var intTextGo = CreateText("Text", intUI.transform, "Press E to Pick Up", 40, TextAnchor.MiddleCenter, Color.yellow);
        uiManager.interactionGroup = intUI.AddComponent<CanvasGroup>();
        uiManager.interactionText = intTextGo.GetComponent<Text>();

        // FEEDBACK
        var fbUI = CreatePanel("FeedbackUI", canvasObj.transform, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0,100), new Vector2(500,100));
        var fbTextGo = CreateText("Text", fbUI.transform, "Correct Recycling!", 48, TextAnchor.MiddleCenter, Color.green);
        uiManager.feedbackGroup = fbUI.AddComponent<CanvasGroup>();
        uiManager.feedbackText = fbTextGo.GetComponent<Text>();

        // GAME OVER
        var goUI = new GameObject("GameOverUI").AddComponent<RectTransform>();
        goUI.SetParent(canvasObj.transform, false);
        goUI.anchorMin = Vector2.zero; goUI.anchorMax = Vector2.one;
        goUI.offsetMin = Vector2.zero; goUI.offsetMax = Vector2.zero;
        var goGroup = goUI.gameObject.AddComponent<CanvasGroup>();
        goGroup.alpha = 0; goGroup.interactable = false; goGroup.blocksRaycasts = false;
        CreateText("Text", goUI, "GAME OVER", 120, TextAnchor.MiddleCenter, Color.red);
        
        Debug.Log("UI Generated Successfully.");
    }
}