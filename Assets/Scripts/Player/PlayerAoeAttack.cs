using System.Collections.Generic;
using UnityEngine;

public class PlayerAoeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private float maxRadius = 3f;
    [SerializeField] private float expansionDuration = 0.25f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private readonly HashSet<Health> hitTargets = new HashSet<Health>();
    private float nextAttackTime;
    private bool isAttacking;
    private float attackStartTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            UpdateAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackStartTime = Time.time;
        nextAttackTime = Time.time + cooldown;
        hitTargets.Clear();
    }

    private void UpdateAttack()
    {
        float elapsed = Time.time - attackStartTime;
        float normalizedTime = expansionDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / expansionDuration);
        float currentRadius = Mathf.Lerp(0f, maxRadius, normalizedTime);

        ApplyDamage(currentRadius);

        if (normalizedTime >= 1f)
        {
            isAttacking = false;
        }
    }

    private void ApplyDamage(float currentRadius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius, collisionMask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("NPC"))
            {
                continue;
            }

            Health health = hit.GetComponentInParent<Health>();
            if (health == null || hitTargets.Contains(health))
            {
                continue;
            }

            health.TakeDamage(damage);
            hitTargets.Add(health);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
