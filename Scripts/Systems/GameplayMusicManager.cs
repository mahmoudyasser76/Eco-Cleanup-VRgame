using UnityEngine;

/// <summary>
/// Manages background music during gameplay.
/// Plays a looping music track that persists throughout the gameplay session.
/// Automatically stops when the scene is unloaded.
/// </summary>
public class GameplayMusicManager : MonoBehaviour
{
    public static GameplayMusicManager Instance { get; private set; }

    [Header("Music")]
    [Tooltip("Background music clip for gameplay.")]
    public AudioClip gameplayMusic;

    [Range(0f, 1f)]
    [Tooltip("Volume for background music.")]
    public float musicVolume = 0.25f;

    [Tooltip("Fade in duration in seconds.")]
    public float fadeInDuration = 2f;

    private AudioSource musicSource;
    private float targetVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Setup AudioSource
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // 2D music
        musicSource.priority = 0; // Highest priority for music
    }

    private void Start()
    {
        if (gameplayMusic != null)
        {
            targetVolume = musicVolume;
            musicSource.clip = gameplayMusic;
            musicSource.volume = 0f;
            musicSource.Play();
            StartCoroutine(FadeIn());
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / fadeInDuration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    /// <summary>
    /// Stops the music with an optional fade out.
    /// </summary>
    public void StopMusic(float fadeDuration = 1f)
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            StartCoroutine(FadeOutAndStop(fadeDuration));
        }
    }

    private System.Collections.IEnumerator FadeOutAndStop(float duration)
    {
        float startVol = musicSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
