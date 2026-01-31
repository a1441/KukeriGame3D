// ShockwaveHitbox.cs
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveHitbox : MonoBehaviour
{
    private LayerMask _hittableLayers;
    private float _damage;
    private float _knockbackForce;
    private Transform _source;
    private bool _active;

    // ensures each target only takes damage once per shockwave
    private readonly HashSet<Collider> _hit = new HashSet<Collider>();

    [SerializeField] private DamagePopupSpawner popupSpawner;

    [Header("Popup Spawn Tuning (WORLD)")]
    [SerializeField] private float popupUpOffset = 0.35f;     // smaller = closer to target
    [SerializeField] private float popupSideJitter = 0.10f;   // smaller = less sideways randomness
    [SerializeField] private float popupUpJitter = 0.05f;     // smaller = less vertical randomness
    [SerializeField] private float popupProbeDistance = 10f;  // doesn't affect distance much; just needs to be > collider size

    void Awake()
    {
        if (!popupSpawner)
            popupSpawner = FindFirstObjectByType<DamagePopupSpawner>();
    }

    public void Arm(Transform source, LayerMask hittableLayers, float damage, float knockbackForce)
    {
        _source = source;
        _hittableLayers = hittableLayers;
        _damage = damage;
        _knockbackForce = knockbackForce;

        _hit.Clear();
        _active = true;
    }

    public void Disarm()
    {
        _active = false;
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);
    private void OnTriggerStay(Collider other) => TryHit(other); // helps if collider grows past an object

    private void TryHit(Collider other)
    {
        if (!_active) return;
        if (!other) return;

        // Layer filter
        if (((1 << other.gameObject.layer) & _hittableLayers.value) == 0)
            return;

        // Ignore self (player root)
        if (_source != null && other.transform == _source)
            return;

        // Hit once per shockwave
        if (_hit.Contains(other))
            return;

        _hit.Add(other);

        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(_damage);

            // Damage number popup (furthest side from player, with small jitter)
            if (popupSpawner != null && _source != null)
            {
                Vector3 worldPos = GetPopupWorldPos(
                    other,
                    _source.position,
                    popupUpOffset,
                    popupSideJitter,
                    popupUpJitter,
                    popupProbeDistance
                );

                popupSpawner.Spawn(_damage, worldPos);
            }

            // Flash (optional)
            HitFlash flash = other.GetComponentInParent<HitFlash>();
            if (flash != null)
                flash.Flash();

            Debug.Log($"[Shockwave] Damaged {other.name} for {_damage}. Remaining HP: {health.GetCurrentHealth()}");
        }

        // Knockback (requires Rigidbody)
        if (_knockbackForce > 0f && other.attachedRigidbody != null && _source != null)
        {
            Vector3 dir = (other.transform.position - _source.position);
            dir.y = 0f;
            dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : _source.forward;

            other.attachedRigidbody.AddForce(dir * _knockbackForce, ForceMode.Impulse);
        }
    }

    private Vector3 GetPopupWorldPos(Collider targetCol, Vector3 playerPos, float upOffset, float sideJitter, float upJitter, float probeDistance)
    {
        // Direction from player -> target (XZ)
        Vector3 dir = (targetCol.bounds.center - playerPos);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        dir.Normalize();

        // Furthest surface point away from player
        Vector3 probePoint = targetCol.bounds.center + dir * probeDistance;
        Vector3 surfacePoint = targetCol.ClosestPoint(probePoint);

        // Up offset
        Vector3 pos = surfacePoint + Vector3.up * upOffset;

        // Random jitter (small)
        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
        float sideAmt = Random.Range(-sideJitter, sideJitter);
        float upAmt = Random.Range(0f, upJitter);

        pos += side * sideAmt;
        pos += Vector3.up * upAmt;

        return pos;
    }
}
