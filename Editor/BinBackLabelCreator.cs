using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor utility that creates BackLabelPlate and BackLabel_Text_* objects
/// for every BIN_* GameObject that only has front labels,
/// and ensures all back labels start INACTIVE in the scene.
/// Run once via Tools menu, then save the scene.
/// </summary>
public class BinBackLabelCreator
{
    [MenuItem("Tools/Create Back Labels for All Bins")]
    static void CreateBackLabels()
    {
        int created = 0;
        List<Transform> bins = FindAllBins();

        foreach (Transform bin in bins)
        {
            Transform frontPlate = null;
            Transform frontText = null;
            bool hasBackPlate = false;
            bool hasBackText = false;

            int childCount = bin.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = bin.GetChild(i);
                string n = child.name;
                if (n == "FrontLabelPlate") frontPlate = child;
                else if (n.StartsWith("FrontLabel_Text_")) frontText = child;
                else if (n == "BackLabelPlate") hasBackPlate = true;
                else if (n.StartsWith("BackLabel_Text_")) hasBackText = true;
            }

            if (frontPlate != null && !hasBackPlate)
            {
                GameObject bp = Object.Instantiate(frontPlate.gameObject, bin);
                bp.name = "BackLabelPlate";
                Mirror(bp.transform, frontPlate);
                bp.SetActive(false); // Start inactive
                Undo.RegisterCreatedObjectUndo(bp, "Create BackLabelPlate");
                created++;
            }

            if (frontText != null && !hasBackText)
            {
                GameObject bt = Object.Instantiate(frontText.gameObject, bin);
                bt.name = frontText.name.Replace("FrontLabel", "BackLabel");
                Mirror(bt.transform, frontText);
                bt.SetActive(false); // Start inactive
                Undo.RegisterCreatedObjectUndo(bt, "Create BackLabel_Text");
                created++;
            }
        }

        if (created > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[BinBackLabelCreator] Created " + created + " back label objects (inactive by default).");
        }
        else
        {
            Debug.Log("[BinBackLabelCreator] All bins already have back labels.");
        }
    }

    /// <summary>
    /// Sets all BackLabelPlate and BackLabel_Text_* to inactive in the scene.
    /// This prevents them from showing before the visibility script runs.
    /// </summary>
    [MenuItem("Tools/Disable All Back Labels In Scene")]
    static void DisableAllBackLabels()
    {
        int disabled = 0;
        List<Transform> bins = FindAllBins();

        foreach (Transform bin in bins)
        {
            int childCount = bin.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = bin.GetChild(i);
                string n = child.name;

                if (n == "BackLabelPlate" || n.StartsWith("BackLabel_Text_"))
                {
                    if (child.gameObject.activeSelf)
                    {
                        Undo.RecordObject(child.gameObject, "Disable back label");
                        child.gameObject.SetActive(false);
                        disabled++;
                    }
                }
            }
        }

        if (disabled > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[BinBackLabelCreator] Disabled " + disabled + " back label objects.");
        }
        else
        {
            Debug.Log("[BinBackLabelCreator] All back labels are already inactive.");
        }
    }

    /// <summary>
    /// Finds all BIN_* transforms by traversing scene roots.
    /// Works with both active and inactive GameObjects.
    /// </summary>
    static List<Transform> FindAllBins()
    {
        List<Transform> bins = new List<Transform>();
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            CollectBins(root.transform, bins);
        }

        return bins;
    }

    static void CollectBins(Transform parent, List<Transform> bins)
    {
        if (parent.name.StartsWith("BIN_"))
        {
            bins.Add(parent);
            return; // BINs don't contain other BINs.
        }

        int childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            CollectBins(parent.GetChild(i), bins);
        }
    }

    static void Mirror(Transform back, Transform front)
    {
        Vector3 pos = front.localPosition;
        pos.z = -pos.z;
        back.localPosition = pos;
        back.localRotation = front.localRotation * Quaternion.Euler(0f, 180f, 0f);
        back.localScale = front.localScale;
    }
}
