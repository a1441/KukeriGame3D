using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class RangedAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Tuning")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float maxDistanceMultiplier = 1.5f;
    [SerializeField] private float fireCooldown = 1.2f;
    [SerializeField] private float minTravelDistance = 15f; // set this = lookRadius (detection range)

    [Header("On-Hit/Muzzle VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleVfx;   // assign in inspector (child on NPC)


    private CharacterStats _stats;
    private float _nextFireTime;

    void Awake()
    {
        _stats = GetComponent<CharacterStats>();

        if (firePoint == null)
            firePoint = transform; // fallback
    }

    public bool CanFire => Time.time >= _nextFireTime;

    public void FireAt(Transform target)
    {
        if (!projectilePrefab) { Debug.LogError("[RangedAttack] projectilePrefab missing", this); return; }
        if (!target) return;
        if (!CanFire) return;

        _nextFireTime = Time.time + fireCooldown;

        Vector3 toTarget = (target.position - firePoint.position);
        float distance = toTarget.magnitude;
        Vector3 dir = (distance > 0.001f) ? (toTarget / distance) : firePoint.forward;

        int dmg = (_stats != null) ? _stats.DamageValue : 0;
        float maxDist = Mathf.Max(minTravelDistance, distance * maxDistanceMultiplier);



        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
        p.Init(transform, dir, dmg, maxDist, projectileSpeed);
        if (muzzleVfx != null)
        {
            muzzleVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleVfx.Play(true);
        }
        Debug.Log($"[RangedAttack] {name} fired at {target.name} dist={distance:F2} dmg={dmg} maxDist={maxDist:F2}");
    }
}
