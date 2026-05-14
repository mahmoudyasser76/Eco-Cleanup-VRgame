using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using SmartRecycling.Pause;

public class PauseMenuSetupEditor
{
    // Sprites
    private const string GREEN_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Green.png";
    private const string RED_BTN = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Rectangles/Red.png";
    private const string PAUSE_BTN_BG = "Assets/GUIPackCartoon/Demo/Sprites/Buttons/Square/Blue.png";
    private const string PAUSE_ICON = "Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons White/Navigation/Pause.png";
    private const string HEADLINE_FLAG = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/Headline - Flag.png";

    private const string POPUP_BG = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/background.png";
    private const string POPUP_BLUE = "Assets/GUIPackCartoon/Demo/Sprites/Backgrounds/Popup/Blue.png";

    private const string HOVER_SOUND = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Bubble 04.wav";
    private const string CLICK_SOUND = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Interface Click 13-1.wav";

    private const string FLAG_KEY = "PauseMenuSetup_Done_v4";

    [InitializeOnLoadMethod]
    private static void OnDomainReload()
    {
        if (SessionState.GetBool(FLAG_KEY, false)) return;

        EditorApplication.delayCall += () =>
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name == "UrbanRecyclingCity" || scene.name == "MainScene")
            {
                SessionState.SetBool(FLAG_KEY, true);
                SetupPauseMenu();
            }
        };
    }

    [MenuItem("Tools/Setup Pause Menu System")]
    public static void SetupPauseMenu()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != "UrbanRecyclingCity" && scene.name != "MainScene")
        {
            Debug.LogWarning("[PauseMenuSetup] Please open the UrbanRecyclingCity or MainScene gameplay scene first.");
            return;
        }

        // Find HUD Canvas (Looking for UIManager or first Canvas)
        Canvas hudCanvas = null;
        var uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            hudCanvas = uiManager.GetComponentInParent<Canvas>();
        }

        if (hudCanvas == null)
        {
            hudCanvas = Object.FindObjectOfType<Canvas>();
            if (hudCanvas == null)
            {
                Debug.LogError("[PauseMenuSetup] Could not find a Canvas in the scene!");
                return;
            }
        }

        Undo.RecordObject(hudCanvas, "Setup Pause Menu");

        // Pause Button HUD
        CreateHUDPauseButton(hudCanvas.transform);

        // Pause Menu Panel
        CreatePauseMenuPanel(hudCanvas.transform);

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[PauseMenuSetup] Pause Menu successfully created and saved.");
    }

    private static void CreateHUDPauseButton(Transform parent)
    {
        string name = "PauseButtonHUD";
        var existing = parent.Find(name);
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

        var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        var rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(80, 80);

        var img = btnObj.GetComponent<Image>();
        img.sprite = LoadSprite(PAUSE_BTN_BG);
        img.type = Image.Type.Sliced;

        var btn = btnObj.GetComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;

        // Shadow
        var shadow = btnObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.4f);
        shadow.effectDistance = new Vector2(3, -3);

        // Icon
        var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(btnObj.transform, false);
        var iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(15, 15);
        iconRect.offsetMax = new Vector2(-15, -15);

        var iconImg = iconObj.GetComponent<Image>();
        iconImg.sprite = LoadSprite(PAUSE_ICON);
        iconImg.preserveAspect = true;
    }

    private static void CreatePauseMenuPanel(Transform parent)
    {
        string name = "PauseMenuPanel";
        var existing = parent.Find(name);
        if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

        // Main overlay
        var panelObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelObj.transform.SetParent(parent, false);

        var panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Dark background
        var bgImage = panelObj.GetComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        // The popup container
        var popupObj = new GameObject("PopupBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        popupObj.transform.SetParent(panelObj.transform, false);

        var popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(400, 350);

        var popupImg = popupObj.GetComponent<Image>();
        popupImg.sprite = LoadSprite(POPUP_BG) ?? LoadSprite(POPUP_BLUE);
        popupImg.type = Image.Type.Sliced;

        // Headline Flag
        var headlineObj = new GameObject("Headline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        headlineObj.transform.SetParent(popupObj.transform, false);
        var hlRect = headlineObj.GetComponent<RectTransform>();
        hlRect.anchorMin = new Vector2(0.5f, 1);
        hlRect.anchorMax = new Vector2(0.5f, 1);
        hlRect.pivot = new Vector2(0.5f, 0.5f);
        hlRect.sizeDelta = new Vector2(400, 100);
        hlRect.anchoredPosition = new Vector2(0, 0);

        var hlImg = headlineObj.GetComponent<Image>();
        hlImg.sprite = LoadSprite(HEADLINE_FLAG);
        hlImg.preserveAspect = true;

        // Title
        var titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer));
        titleObj.transform.SetParent(headlineObj.transform, false);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "PAUSED";
        titleTmp.fontSize = 45;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;
        titleTmp.outlineWidth = 0.2f;
        titleTmp.outlineColor = new Color32(0, 0, 0, 255);

        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(0, 20); // shift up within the ribbon
        titleRect.offsetMax = Vector2.zero;

        // Buttons Container
        var btnsObj = new GameObject("ButtonContainer", typeof(RectTransform));
        btnsObj.transform.SetParent(popupObj.transform, false);
        var layout = btnsObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        var btnsRect = btnsObj.GetComponent<RectTransform>();
        btnsRect.anchorMin = new Vector2(0.1f, 0.1f);
        btnsRect.anchorMax = new Vector2(0.9f, 0.75f);
        btnsRect.offsetMin = Vector2.zero;
        btnsRect.offsetMax = Vector2.zero;

        // Resume Button
        var resumeBtn = CreateButton(btnsObj.transform, "ResumeButton", "RESUME", GREEN_BTN);
        
        // Main Menu Button
        var mainBtn = CreateButton(btnsObj.transform, "MainMenuButton", "MAIN MENU", RED_BTN);

        // Attach Manager
        var managerObj = GameObject.Find("PauseMenuManager");
        if (managerObj == null) managerObj = new GameObject("PauseMenuManager");
        var manager = managerObj.GetComponent<PauseMenuManager>();
        if (manager == null) manager = managerObj.AddComponent<PauseMenuManager>();

        manager.pauseMenuPanel = panelObj;
        manager.pauseButtonHUD = parent.Find("PauseButtonHUD").GetComponent<Button>();
        manager.resumeButton = resumeBtn.GetComponent<Button>();
        manager.mainMenuButton = mainBtn.GetComponent<Button>();

        manager.hoverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(HOVER_SOUND);
        manager.clickSound = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SOUND);
        
        var audioSrc = managerObj.GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = managerObj.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        manager.audioSource = audioSrc;

        manager.mainMenuSceneName = "MainMenu";

        // Hide by default
        panelObj.SetActive(false);
        EditorUtility.SetDirty(manager);
    }

    private static GameObject CreateButton(Transform parent, string name, string text, string spritePath)
    {
        var btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Shadow));
        btnObj.transform.SetParent(parent, false);

        var img = btnObj.GetComponent<Image>();
        img.sprite = LoadSprite(spritePath);
        img.type = Image.Type.Sliced;

        var le = btnObj.GetComponent<LayoutElement>();
        le.preferredHeight = 70;

        var shadow = btnObj.GetComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.4f);
        shadow.effectDistance = new Vector2(3, -3);

        var txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        txtObj.transform.SetParent(btnObj.transform, false);
        var txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 30;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = new Color32(0, 0, 0, 128);

        return btnObj;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
