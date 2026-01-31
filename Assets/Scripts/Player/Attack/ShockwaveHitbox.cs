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

    // Optional but recommended: prevents missing targets already inside or with physics timing
    private void OnTriggerStay(Collider other) => TryHit(other);

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

        // Damage: look for Health on the collider or its parents
        // (use GetComponentInParent so it works if colliders are on child bones)
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            health.TakeDamage(_damage);
            Debug.Log($"[Shockwave] Damaged {other.name} for {_damage}. Remaining HP: {health.GetCurrentHealth()}");
        }

        // Knockback (requires Rigidbody on the target)
        if (_knockbackForce > 0f && other.attachedRigidbody != null && _source != null)
        {
            Vector3 dir = (other.transform.position - _source.position);
            dir.y = 0f; // optional: keep knockback horizontal
            dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : _source.forward;

            other.attachedRigidbody.AddForce(dir * _knockbackForce, ForceMode.Impulse);
        }
    }
}
