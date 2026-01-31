using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Death Timing")]
    [Tooltip("Wait this long after the killing blow before starting death (lets flash + damage text show).")]
    [SerializeField] private float deathStartDelay = 0.08f;

    [Tooltip("Destroy after death starts (keep short for placeholder death).")]
    [SerializeField] private float despawnDelay = 0.25f;

    [Header("Death Animation (optional)")]
    [SerializeField] private string deathTrigger = "Die";

    [Header("Fallback Fade (if no death anim)")]
    [SerializeField] private bool fadeIfNoDeathAnim = true;

    private bool _isDead;
    private Coroutine _deathRoutine;

    private Animator _animator;
    private Renderer[] _renderers;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        currentHealth = maxHealth;

        _animator = GetComponentInChildren<Animator>();
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead => _isDead;

    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        if (damage <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (currentHealth <= 0f)
        {
            _isDead = true;

            if (_deathRoutine != null) StopCoroutine(_deathRoutine);
            _deathRoutine = StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        // 1) Let hit feedback (flash + popup) appear first
        if (deathStartDelay > 0f)
            yield return new WaitForSeconds(deathStartDelay);

        // 2) Start death anim if available
        bool playedAnim = TryPlayDeathAnim();

        // 3) If no anim, fade out as placeholder "death"
        if (!playedAnim && fadeIfNoDeathAnim)
        {
            // fade during despawn window (or part of it)
            StartCoroutine(FadeOutRoutine(despawnDelay));
        }

        // 4) Despawn shortly after death begins
        if (despawnDelay > 0f)
            yield return new WaitForSeconds(despawnDelay);

        Destroy(gameObject);
    }

    private bool TryPlayDeathAnim()
    {
        if (_animator == null) return false;
        if (string.IsNullOrWhiteSpace(deathTrigger)) return false;

        // Only set trigger if it exists (avoids mistakes)
        foreach (var p in _animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger && p.name == deathTrigger)
            {
                _animator.SetTrigger(deathTrigger);
                return true;
            }
        }
        return false;
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        duration = Mathf.Max(0.0001f, duration);

        // Cache originals
        var mats = new Material[_renderers.Length][];
        var originals = new Color[_renderers.Length][];

        for (int r = 0; r < _renderers.Length; r++)
        {
            if (_renderers[r] == null) continue;

            mats[r] = _renderers[r].materials; // instance materials
            originals[r] = new Color[mats[r].Length];

            for (int m = 0; m < mats[r].Length; m++)
                originals[r][m] = GetMatColor(mats[r][m]);
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / duration);

            for (int r = 0; r < _renderers.Length; r++)
            {
                if (mats[r] == null) continue;

                for (int m = 0; m < mats[r].Length; m++)
                {
                    Color c = originals[r][m];
                    c.a *= a;
                    SetMatColor(mats[r][m], c);
                }
            }

            yield return null;
        }
    }

    private static Color GetMatColor(Material mat)
    {
        if (mat.HasProperty(BaseColorId)) return mat.GetColor(BaseColorId);
        if (mat.HasProperty(ColorId)) return mat.GetColor(ColorId);
        return Color.white;
    }

    private static void SetMatColor(Material mat, Color c)
    {
        if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, c);
        else if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, c);
    }
}
