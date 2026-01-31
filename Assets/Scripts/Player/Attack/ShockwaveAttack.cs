using System.Collections;
using UnityEngine;

public class ShockwaveAttack : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private float cooldown = 1.5f;

    [Header("Gameplay (WORLD UNITS)")]
    [SerializeField] private float maxWorldRadius = 6f;
    [SerializeField] private float damageMultiplier = 1.0f;  // <-- scales from CharacterStats.damage
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Wave Timing")]
    [SerializeField] private float waveDuration = 0.25f;
    [SerializeField] private float activeExtraTime = 0.05f;

    [Header("VFX (Visual Only)")]
    [SerializeField] private ParticleSystem vfx_shockwave_01;
    [SerializeField] private Transform shockwaveVfxRoot;
    [SerializeField] private float vfxRadiusMultiplier = 1f;

    [Header("HITBOX (Child Object)")]
    [SerializeField] private ShockwaveHitbox hitbox;
    [SerializeField] private SphereCollider hitboxCollider;

    private float _nextTime;
    private Coroutine _waveRoutine;

    private Vector3 _vfxBaseScale;
    private bool _vfxBaseScaleCaptured;

    private CharacterCombat _combat;

    void Awake()
    {
        _combat = GetComponent<CharacterCombat>(); // <-- attacker combat/stats source
        CaptureVfxBaseScale();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryShockwave();
    }

    public void TryShockwave()
    {
        if (Time.time < _nextTime) return;
        _nextTime = Time.time + cooldown;

        if (!_combat) { Debug.LogError("[Shockwave] CharacterCombat missing on caster"); return; }
        if (!hitboxCollider) { Debug.LogError("[Shockwave] hitboxCollider not assigned"); return; }
        if (!hitbox) { Debug.LogError("[Shockwave] hitbox not assigned"); return; }

        hitboxCollider.transform.localPosition = Vector3.zero;
        hitboxCollider.transform.localRotation = Quaternion.identity;
        hitboxCollider.center = Vector3.zero;

        CaptureVfxBaseScale();

        // Arm hit logic with combat/stats
        hitbox.Arm(_combat, hittableLayers, damageMultiplier, knockbackForce);

        SetWaveWorldRadius(0f);
        PlayVfx();

        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(ExpandWaveWorld());
    }

    IEnumerator ExpandWaveWorld()
    {
        float t = 0f;
        float duration = Mathf.Max(0.0001f, waveDuration);

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            float worldR = Mathf.Lerp(0f, maxWorldRadius, p);
            SetWaveWorldRadius(worldR);

            yield return null;
        }

        SetWaveWorldRadius(maxWorldRadius);

        if (activeExtraTime > 0f)
            yield return new WaitForSeconds(activeExtraTime);

        hitbox.Disarm();
        SetWaveWorldRadius(0f);

        _waveRoutine = null;
    }

    void SetWaveWorldRadius(float worldRadius)
    {
        hitboxCollider.radius = WorldRadiusToLocal(worldRadius, hitboxCollider.transform);

        if (shockwaveVfxRoot)
        {
            float p = (maxWorldRadius <= 0.0001f) ? 0f : (worldRadius / maxWorldRadius);
            p = Mathf.Clamp01(p);

            shockwaveVfxRoot.localScale = _vfxBaseScale * (p * vfxRadiusMultiplier);
        }
    }

    static float WorldRadiusToLocal(float worldRadius, Transform t)
    {
        Vector3 s = t.lossyScale;
        float max = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        if (max < 0.0001f) max = 1f;
        return worldRadius / max;
    }

    void CaptureVfxBaseScale()
    {
        if (!shockwaveVfxRoot) return;
        if (_vfxBaseScaleCaptured) return;

        _vfxBaseScale = shockwaveVfxRoot.localScale;
        _vfxBaseScaleCaptured = true;
    }

    void PlayVfx()
    {
        if (!vfx_shockwave_01) return;
        vfx_shockwave_01.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx_shockwave_01.Play(true);
    }
}
