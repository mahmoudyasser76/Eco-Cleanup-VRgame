using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to automatically assign audio clips to the RecyclingFeedback component.
/// Run via: Tools → Recycling → Setup Audio Clips
/// </summary>
public class RecyclingAudioSetup : Editor
{
    private const string SUCCESS_CLIP_PATH = "Assets/GUIPackCartoon/Demo/Sounds/Coins.wav";
    private const string WRONG_CLIP_PATH = "Assets/GUIPackCartoon/Demo/Sounds/Close.wav";

    [MenuItem("Tools/Recycling/Setup Audio Clips")]
    public static void SetupAudioClips()
    {
        // Find the RecyclingFeedback component in the scene
        RecyclingFeedback feedback = FindFirstObjectByType<RecyclingFeedback>();
        if (feedback == null)
        {
            Debug.LogError("[RecyclingAudioSetup] RecyclingFeedback component not found in scene!");
            return;
        }

        // Load audio clips from AssetDatabase
        AudioClip successClip = AssetDatabase.LoadAssetAtPath<AudioClip>(SUCCESS_CLIP_PATH);
        AudioClip wrongClip = AssetDatabase.LoadAssetAtPath<AudioClip>(WRONG_CLIP_PATH);

        if (successClip == null)
            Debug.LogWarning($"[RecyclingAudioSetup] Success clip not found at: {SUCCESS_CLIP_PATH}");
        if (wrongClip == null)
            Debug.LogWarning($"[RecyclingAudioSetup] Wrong clip not found at: {WRONG_CLIP_PATH}");

        // Register undo for the component
        Undo.RecordObject(feedback, "Setup Recycling Audio Clips");

        // Assign the clips
        feedback.successClip = successClip;
        feedback.wrongClip = wrongClip;
        feedback.sfxVolume = 0.5f;

        // Mark as dirty so Unity saves the change
        EditorUtility.SetDirty(feedback);

        int assigned = 0;
        if (successClip != null) assigned++;
        if (wrongClip != null) assigned++;

        Debug.Log($"[RecyclingAudioSetup] ✓ Assigned {assigned}/2 audio clips to RecyclingFeedback." +
                  $"\n  Success: {(successClip != null ? successClip.name : "MISSING")}" +
                  $"\n  Wrong: {(wrongClip != null ? wrongClip.name : "MISSING")}" +
                  $"\n  Volume: 0.5");
    }
}
