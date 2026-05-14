using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SmartRecycling.Pause
{
    public class PauseMenuManager : MonoBehaviour
    {
        public static PauseMenuManager Instance;

        [Header("UI References")]
        public GameObject pauseMenuPanel;
        public Button resumeButton;
        public Button mainMenuButton;
        public Button pauseButtonHUD; // The icon top-right

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip hoverSound;
        public AudioClip clickSound;

        [Header("Settings")]
        public string mainMenuSceneName = "MainMenu";

        private bool isPaused = false;
        private CanvasGroup pauseCanvasGroup;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            if (pauseMenuPanel != null)
            {
                pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
                if (pauseCanvasGroup == null)
                    pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
                
                pauseMenuPanel.SetActive(false);
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (pauseButtonHUD != null)
                pauseButtonHUD.onClick.AddListener(PauseGame);

            // Audio for Hover / Click
            AddButtonSounds(resumeButton);
            AddButtonSounds(mainMenuButton);
            AddButtonSounds(pauseButtonHUD);
        }

        private void AddButtonSounds(Button btn)
        {
            if (btn == null || audioSource == null) return;
            
            // Add EventTrigger for hover sounds
            var eventTrigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((data) => { PlaySound(hoverSound); });
            eventTrigger.triggers.Add(pointerEnter);

            btn.onClick.AddListener(() => { PlaySound(clickSound); });
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
                if (pauseCanvasGroup != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadePanel(pauseCanvasGroup, 0f, 1f, 0.15f));
                }
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pauseMenuPanel != null && pauseCanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadePanel(pauseCanvasGroup, 1f, 0f, 0.15f, () => {
                    pauseMenuPanel.SetActive(false);
                }));
            }
            else if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private IEnumerator FadePanel(CanvasGroup cg, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
        {
            float time = 0;
            // Since timeScale is 0, we must use unscaledDeltaTime
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
                yield return null;
            }
            cg.alpha = endAlpha;
            onComplete?.Invoke();
        }
    }
}
