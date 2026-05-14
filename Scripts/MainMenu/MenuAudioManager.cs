using UnityEngine;

/// <summary>
/// Manages Main Menu audio: UI sound effects and background music.
/// Provides static methods for buttons to trigger hover/click sounds.
/// </summary>
public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource for UI sound effects.")]
    public AudioSource sfxSource;

    [Tooltip("AudioSource for background music.")]
    public AudioSource musicSource;

    [Header("Sound Effects")]
    [Tooltip("Sound played on button hover.")]
    public AudioClip hoverSound;

    [Tooltip("Sound played on button click.")]
    public AudioClip clickSound;

    [Header("Music")]
    [Tooltip("Background music clip for the menu.")]
    public AudioClip menuMusic;

    [Range(0f, 1f)]
    [Tooltip("Volume for background music.")]
    public float musicVolume = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("Volume for sound effects.")]
    public float sfxVolume = 0.7f;

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
    }

    private void Start()
    {
        // Start background music
        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Plays the hover sound effect.
    /// </summary>
    public void PlayHover()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound, sfxVolume);
        }
    }

    /// <summary>
    /// Plays the click sound effect.
    /// </summary>
    public void PlayClick()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound, sfxVolume);
        }
    }

    /// <summary>
    /// Static convenience for buttons to call hover sound.
    /// </summary>
    public static void TriggerHover()
    {
        if (Instance != null) Instance.PlayHover();
    }

    /// <summary>
    /// Static convenience for buttons to call click sound.
    /// </summary>
    public static void TriggerClick()
    {
        if (Instance != null) Instance.PlayClick();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
