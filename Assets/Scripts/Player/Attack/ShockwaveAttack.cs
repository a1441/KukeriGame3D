using System.Collections;
using UnityEngine;

public class ShockwaveAttack : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private float cooldown = 1.5f;

    [Header("Gameplay (WORLD UNITS)")]
    [SerializeField] private float maxWorldRadius = 6f;   // final shockwave radius in world units
    [SerializeField] private float damage = 25f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Wave Timing")]
    [SerializeField] private float waveDuration = 0.25f;  // time to go from 0 -> maxWorldRadius
    [SerializeField] private float activeExtraTime = 0.05f;

    [Header("VFX (Visual Only)")]
    [SerializeField] private ParticleSystem vfx_shockwave_01;
    [SerializeField] private Transform shockwaveVfxRoot;  // scaled visually
    [SerializeField] private float vfxRadiusMultiplier = 1f;

    [Header("HITBOX (Child Object)")]
    [SerializeField] private ShockwaveHitbox hitbox;
    [SerializeField] private SphereCollider hitboxCollider;

    float _nextTime;
    Coroutine _waveRoutine;

    Vector3 _vfxBaseScale;
    bool _vfxBaseScaleCaptured;

    void Awake()
    {
        CaptureVfxBaseScale();
    }

    void OnValidate()
    {
        if (cooldown < 0f) cooldown = 0f;
        if (maxWorldRadius < 0f) maxWorldRadius = 0f;
        if (waveDuration < 0.0001f) waveDuration = 0.0001f;
        if (activeExtraTime < 0f) activeExtraTime = 0f;
        if (vfxRadiusMultiplier < 0f) vfxRadiusMultiplier = 0f;
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

        if (!hitboxCollider) { Debug.LogError("[Shockwave] hitboxCollider not assigned"); return; }
        if (!hitbox) { Debug.LogError("[Shockwave] hitbox not assigned"); return; }

        // Keep hitbox centered
        hitboxCollider.transform.localPosition = Vector3.zero;
        hitboxCollider.transform.localRotation = Quaternion.identity;
        hitboxCollider.center = Vector3.zero;

        // Capture base VFX scale once (or if you reassign shockwaveVfxRoot at runtime, call ResetVfxBaseScale())
        CaptureVfxBaseScale();

        // Arm hit logic
        hitbox.Arm(transform, hittableLayers, damage, knockbackForce);

        // Start from 0 (both gameplay radius and VFX scale)
        SetWaveWorldRadius(0f);

        // Play VFX
        PlayVfx();

        // Expand
        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(ExpandWaveWorld());
    }

    IEnumerator ExpandWaveWorld()
    {
        float t = 0f;
        float duration = Mathf.Max(0.0001f, waveDuration);

        // Expand from 0 -> maxWorldRadius linearly
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration); // 0..1 linear

            float worldR = Mathf.Lerp(0f, maxWorldRadius, p);
            SetWaveWorldRadius(worldR);

            yield return null;
        }

        // Ensure exact final size
        SetWaveWorldRadius(maxWorldRadius);

        // Optional small active window
        if (activeExtraTime > 0f)
            yield return new WaitForSeconds(activeExtraTime);

        // Disarm + reset
        hitbox.Disarm();
        SetWaveWorldRadius(0f);

        _waveRoutine = null;
    }

    void SetWaveWorldRadius(float worldRadius)
    {
        // Collider radius (LOCAL units) -> convert world -> local so scaling doesn't break it
        hitboxCollider.radius = WorldRadiusToLocal(worldRadius, hitboxCollider.transform);

        // VFX scale from 0 -> target (relative to captured base scale)
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

        // IMPORTANT:
        // Set shockwaveVfxRoot in the prefab/scene to the size you consider "full size".
        // We'll scale from 0 to this base scale (times vfxRadiusMultiplier).
        _vfxBaseScale = shockwaveVfxRoot.localScale;
        _vfxBaseScaleCaptured = true;
    }

    // Call this if you swap shockwaveVfxRoot at runtime and want to recapture base scale.
    public void ResetVfxBaseScale()
    {
        _vfxBaseScaleCaptured = false;
        CaptureVfxBaseScale();
    }

    void PlayVfx()
    {
        if (!vfx_shockwave_01) return;
        vfx_shockwave_01.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx_shockwave_01.Play(true);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!hitboxCollider) return;

        Gizmos.color = Color.cyan;
        Vector3 center = hitboxCollider.transform.TransformPoint(hitboxCollider.center);

        float worldRadius = hitboxCollider.radius * Mathf.Max(
            Mathf.Abs(hitboxCollider.transform.lossyScale.x),
            Mathf.Abs(hitboxCollider.transform.lossyScale.y),
            Mathf.Abs(hitboxCollider.transform.lossyScale.z)
        );

        Gizmos.DrawWireSphere(center, worldRadius);
    }
#endif
}
