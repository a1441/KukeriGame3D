using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBarUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private CharacterStats targetStats;

    [Header("UI")]
    [SerializeField] private Image currentFill;   // left->right
    [SerializeField] private Image missingFill;   // right->left

    [Header("Positioning")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);
    [SerializeField] private Transform followRoot; // optional; defaults to targetStats.transform

    [Header("Behavior")]
    [SerializeField] private bool hideWhenFull = false;

    Camera _cam;

    private GameObject healthBar;

    void Awake()
    {
        healthBar = this.gameObject;
        _cam = Camera.main;
        // HideHealthBar();

        // Auto-find stats from parent if not set
        if (!targetStats)
            targetStats = GetComponentInParent<CharacterStats>();

        if (!followRoot && targetStats)
            followRoot = targetStats.transform;
    }

    void Start()
    {
        var master = targetStats ? targetStats.transform.root : transform.root;

        Debug.Log($"[HealthBar] master={master.name}, masterTag={master.tag}, barObj={name}, barTag={tag}");

        bool isPlayer = master.CompareTag("Player") || (targetStats && targetStats.CompareTag("Player"));

        Debug.Log($"[HealthBar] isPlayer={isPlayer}");

        if (isPlayer) ShowHealthBar();
        else HideHealthBar();
 
    }

    void LateUpdate()
    {
        if (!targetStats || !currentFill || !missingFill) return;

        // Follow above head
        if (followRoot)
            transform.position = followRoot.position + worldOffset;

        // Face camera (billboard)
        if (_cam)
        {
            Vector3 forward = _cam.transform.forward;
            forward.y = 0f; // optional: keep upright
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(forward);
        }

        // Update fills
        float max = Mathf.Max(1f, targetStats.MaxHealthValue);
        float ratio = Mathf.Clamp01(targetStats.currentHealth / max);

        currentFill.fillAmount = ratio;
        missingFill.fillAmount = 1f - ratio;

        // Optional hide when full HP
        if (hideWhenFull)
            gameObject.SetActive(ratio < 0.999f);
    }

    public void ShowHealthBar()
    {
        healthBar.SetActive(true);
    }

    public void HideHealthBar()
    {
        healthBar.SetActive(false);
    }

    // Call this after instantiating to bind a specific target
    public void Bind(CharacterStats stats, Transform follow = null)
    {
        targetStats = stats;
        followRoot = follow ? follow : (stats ? stats.transform : null);
    }
}
