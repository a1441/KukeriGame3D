using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;

    private int _damage;
    private Vector3 _dir;
    private float _maxDistance;
    private Vector3 _startPos;
    private Transform _owner;

    [SerializeField] private LayerMask playerLayers;    // set to Player layer
    [SerializeField] private LayerMask obstacleLayers;  // set to Default/Environment layers

    public void Init(Transform owner, Vector3 direction, int damage, float maxDistance, float overrideSpeed = -1f)
    {
        _owner = owner;
        _dir = direction.normalized;
        _damage = damage;
        _maxDistance = Mathf.Max(0.1f, maxDistance);
        _startPos = transform.position;

        if (overrideSpeed > 0f) speed = overrideSpeed;

        Debug.Log($"[Projectile] Spawned by {(_owner ? _owner.name : "NULL")} dmg={_damage} maxDist={_maxDistance:F2} speed={speed:F2}");
    }

    void Update()
    {
        // Move linearly
        transform.position += _dir * speed * Time.deltaTime;

        // Expire after travelling max distance
        float travelled = Vector3.Distance(_startPos, transform.position);
        if (travelled >= _maxDistance)
        {
            Debug.Log($"[Projectile] Expired after {travelled:F2}m");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other) return;

        // Ignore owner (and owner's children)
        if (_owner != null && (other.transform == _owner || other.transform.IsChildOf(_owner)))
            return;

        int otherMaskBit = 1 << other.gameObject.layer;

        // 1) HIT PLAYER ONLY
        if ((playerLayers.value & otherMaskBit) != 0)
        {
            CharacterStats targetStats = other.GetComponentInParent<CharacterStats>();
            if (targetStats != null)
            {
                int before = targetStats.currentHealth;
                targetStats.TakeDamage(_damage);
                int after = targetStats.currentHealth;

                Debug.Log($"[Projectile] {(_owner ? _owner.name : "Unknown")} -> {targetStats.name} for {_damage} (HP {before} -> {after})");
            }
            else
            {
                Debug.LogWarning($"[Projectile] Hit player layer but no CharacterStats found on {other.name}");
            }

            Destroy(gameObject);
            return;
        }

        // 2) STOP ON OBSTACLES (walls, ground, props)
        if ((obstacleLayers.value & otherMaskBit) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // 3) IGNORE EVERYTHING ELSE (NPCs, enemies, allies) -> pass through
    }

}
