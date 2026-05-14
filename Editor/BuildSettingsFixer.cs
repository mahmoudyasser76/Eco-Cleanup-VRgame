using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Ensures all required scenes are in Build Settings.
/// Runs automatically on domain reload.
/// </summary>
[InitializeOnLoad]
public class BuildSettingsFixer
{
    static BuildSettingsFixer()
    {
        EditorApplication.delayCall += FixBuildSettings;
    }

    [MenuItem("Tools/Fix Build Settings")]
    public static void FixBuildSettings()
    {
        string[] requiredScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/UrbanRecyclingCity.unity"
        };

        var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        foreach (string scenePath in requiredScenes)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", scenePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[BuildSettingsFixer] Scene file not found: {scenePath}");
                continue;
            }

            bool found = false;
            for (int i = 0; i < currentScenes.Count; i++)
            {
                if (currentScenes[i].path == scenePath)
                {
                    if (!currentScenes[i].enabled)
                    {
                        currentScenes[i] = new EditorBuildSettingsScene(scenePath, true);
                        changed = true;
                        Debug.Log($"[BuildSettingsFixer] Enabled scene: {scenePath}");
                    }
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                changed = true;
                Debug.Log($"[BuildSettingsFixer] Added scene to Build Settings: {scenePath}");
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = currentScenes.ToArray();
            Debug.Log("[BuildSettingsFixer] Build Settings updated successfully!");
        }
        else
        {
            Debug.Log("[BuildSettingsFixer] All required scenes already in Build Settings.");
        }
    }
}
