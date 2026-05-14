using UnityEngine;

/// <summary>
/// Lightweight visibility system for recycling bin labels.
/// Finds existing front and back label GameObjects by naming convention
/// and toggles their visibility so only the camera-facing label set
/// is shown at any time. Uses SetActive to completely hide/show labels.
/// Back labels MUST already exist in the scene (use Tools/Create Back Labels).
/// Attach this component to each BIN_* GameObject.
/// </summary>
[DisallowMultipleComponent]
public class BinLabelVisibility : MonoBehaviour
{
    // ── Cached label references ──────────────────────────────────────
    private GameObject frontLabelPlate;
    private GameObject frontLabelText;
    private GameObject backLabelPlate;
    private GameObject backLabelText;

    // ── Camera reference ─────────────────────────────────────────────
    private Transform cachedCameraTransform;

    // ── Direction from bin center to front label (horizontal only) ───
    private Vector3 frontLabelDirection;
    private bool hasLabels;

    // ── State & anti-flicker ─────────────────────────────────────────
    private bool showingFront = true;
    private const float Hysteresis = 0.06f;

    // ── Performance: throttled updates ───────────────────────────────
    private const float UpdateInterval = 0.1f;
    private float nextUpdateTime;

    // ── Distance culling ─────────────────────────────────────────────
    private const float MaxVisibleDistanceSqr = 900f; // 30m squared
    private bool labelsHidden;

    // ─────────────────────────────────────────────────────────────────
    // Initialization
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        FindLabels();

        if (frontLabelPlate == null && frontLabelText == null)
        {
            enabled = false;
            return;
        }

        hasLabels = true;
        CacheFrontLabelDirection();

        // CRITICAL: Force correct initial state.
        // Front visible, back completely hidden.
        ForceSetActive(frontLabelPlate, true);
        ForceSetActive(frontLabelText, true);
        ForceSetActive(backLabelPlate, false);
        ForceSetActive(backLabelText, false);
        showingFront = true;
        labelsHidden = false;
    }

    private void OnEnable()
    {
        // Re-apply state when component is re-enabled.
        if (hasLabels)
        {
            ApplyVisibility(showingFront);
        }
    }

    private void Start()
    {
        CacheMainCamera();
    }

    // ─────────────────────────────────────────────────────────────────
    // Label discovery – finds existing front and back labels by name.
    // Also searches inactive children so it finds pre-disabled back labels.
    // ─────────────────────────────────────────────────────────────────

    private void FindLabels()
    {
        // Use GetComponentsInChildren to also find inactive children,
        // but only search direct children (depth 1).
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            string childName = child.name;

            if (childName == "FrontLabelPlate")
                frontLabelPlate = child.gameObject;
            else if (childName.StartsWith("FrontLabel_Text_"))
                frontLabelText = child.gameObject;
            else if (childName == "BackLabelPlate")
                backLabelPlate = child.gameObject;
            else if (childName.StartsWith("BackLabel_Text_"))
                backLabelText = child.gameObject;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Direction caching
    // ─────────────────────────────────────────────────────────────────

    private void CacheFrontLabelDirection()
    {
        // Temporarily enable front label to get correct world position.
        Transform refTransform = frontLabelPlate != null
            ? frontLabelPlate.transform
            : frontLabelText.transform;

        Vector3 dir = refTransform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            // Fallback: use the bin's negative forward as front direction.
            dir = -transform.forward;
            dir.y = 0f;
        }

        frontLabelDirection = dir.normalized;
    }

    private void CacheMainCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
            cachedCameraTransform = cam.transform;
    }

    // ─────────────────────────────────────────────────────────────────
    // Visibility update (throttled, with hysteresis + distance culling)
    // ─────────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (cachedCameraTransform == null)
        {
            CacheMainCamera();
            if (cachedCameraTransform == null) return;
        }

        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + UpdateInterval;

        Vector3 toCamera = cachedCameraTransform.position - transform.position;
        toCamera.y = 0f;

        float distSqr = toCamera.sqrMagnitude;

        // Distance culling: completely hide both labels when far away.
        if (distSqr > MaxVisibleDistanceSqr)
        {
            if (!labelsHidden)
            {
                ForceSetActive(frontLabelPlate, false);
                ForceSetActive(frontLabelText, false);
                ForceSetActive(backLabelPlate, false);
                ForceSetActive(backLabelText, false);
                labelsHidden = true;
            }
            return;
        }

        if (distSqr < 0.001f) return;

        float dot = Vector3.Dot(toCamera.normalized, frontLabelDirection);

        // Determine desired state with hysteresis.
        bool wantFront;
        if (showingFront)
            wantFront = dot > -Hysteresis;
        else
            wantFront = dot > Hysteresis;

        // Apply if state changed or recovering from distance culling.
        if (wantFront != showingFront || labelsHidden)
        {
            showingFront = wantFront;
            labelsHidden = false;
            ApplyVisibility(showingFront);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Visibility helpers – use SetActive for complete hide/show
    // ─────────────────────────────────────────────────────────────────

    private void ApplyVisibility(bool front)
    {
        ForceSetActive(frontLabelPlate, front);
        ForceSetActive(frontLabelText, front);
        ForceSetActive(backLabelPlate, !front);
        ForceSetActive(backLabelText, !front);
    }

    /// <summary>
    /// Sets active state only if different from current, to avoid
    /// unnecessary activation/deactivation overhead.
    /// </summary>
    private static void ForceSetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
