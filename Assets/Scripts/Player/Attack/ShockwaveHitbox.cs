using System.Collections.Generic;
using UnityEngine;

public class ShockwaveHitbox : MonoBehaviour
{
    private LayerMask _hittableLayers;
    private float _damageMultiplier;
    private float _knockbackForce;

    private CharacterCombat _sourceCombat;
    private Transform _source;
    private bool _active;

    // ensures each CHARACTER only takes damage once per shockwave
    private readonly HashSet<CharacterStats> _hit = new HashSet<CharacterStats>();

    [SerializeField] private DamagePopupSpawner popupSpawner;

    [Header("Popup Spawn Tuning (WORLD)")]
    [SerializeField] private float popupUpOffset = 0.35f;
    [SerializeField] private float popupSideJitter = 0.10f;
    [SerializeField] private float popupUpJitter = 0.05f;
    [SerializeField] private float popupProbeDistance = 10f;

    void Awake()
    {
        if (!popupSpawner)
            popupSpawner = FindFirstObjectByType<DamagePopupSpawner>();
    }

    public void Arm(CharacterCombat sourceCombat, LayerMask hittableLayers, float damageMultiplier, float knockbackForce)
    {
        _sourceCombat = sourceCombat;
        _source = sourceCombat ? sourceCombat.transform : null;

        _hittableLayers = hittableLayers;
        _damageMultiplier = damageMultiplier;
        _knockbackForce = knockbackForce;

        _hit.Clear();
        _active = true;
    }

    public void Disarm()
    {
        _active = false;
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);
    private void OnTriggerStay(Collider other) => TryHit(other);

    private void TryHit(Collider other)
    {
        if (!_active || !other) return;

        // Layer filter
        if (((1 << other.gameObject.layer) & _hittableLayers.value) == 0)
            return;

        // Identify the character target
        CharacterStats targetStats = other.GetComponentInParent<CharacterStats>();
        if (!targetStats) return;

        // Ignore self (same character)
        if (_source != null && targetStats.transform == _source)
            return;

        // Hit once per shockwave per character
        if (_hit.Contains(targetStats))
            return;

        _hit.Add(targetStats);

        // Compute damage from attacker stats
        int baseDamage = (_sourceCombat && _sourceCombat.Stats) ? _sourceCombat.Stats.DamageValue : 0;
        int finalDamage = Mathf.RoundToInt(baseDamage * _damageMultiplier);

        targetStats.TakeDamage(finalDamage);

        // Damage number popup
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

            popupSpawner.Spawn(finalDamage, worldPos); // adjust if your Spawn expects float
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
        Vector3 dir = (targetCol.bounds.center - playerPos);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        dir.Normalize();

        Vector3 probePoint = targetCol.bounds.center + dir * probeDistance;
        Vector3 surfacePoint = targetCol.ClosestPoint(probePoint);

        Vector3 pos = surfacePoint + Vector3.up * upOffset;

        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
        float sideAmt = Random.Range(-sideJitter, sideJitter);
        float upAmt = Random.Range(0f, upJitter);

        pos += side * sideAmt;
        pos += Vector3.up * upAmt;

        return pos;
    }
}
