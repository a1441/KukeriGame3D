using System.Collections.Generic;
using UnityEngine;

public class ShockwaveAttack : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxRadius = 4f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Ring Visuals")]
    [SerializeField] private float ringWidth = 0.1f;
    [SerializeField] private int ringSegments = 64;
    [SerializeField] private Material ringMaterial;
    [SerializeField] private Color ringColor = new Color(0.6f, 0.9f, 1f, 0.9f);
    [SerializeField] private LineRenderer ringRenderer;

    private readonly HashSet<int> hitTargets = new HashSet<int>();
    private float currentRadius;
    private float elapsedTime;
    private bool isActive;

    private void Awake()
    {
        SetupRingRenderer();
        DisableRing();
    }

    private void Update()
    {
        // Default input trigger (can be replaced by calling Trigger() from other systems).
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Trigger();
        }

        if (isActive)
        {
            UpdateShockwave();
        }
    }

    public void Trigger()
    {
        // Prevent overlapping shockwaves.
        if (isActive)
        {
            return;
        }

        isActive = true;
        elapsedTime = 0f;
        currentRadius = 0f;
        hitTargets.Clear();
        EnableRing();
    }

    private void UpdateShockwave()
    {
        elapsedTime += Time.deltaTime;
        float normalizedTime = duration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / duration);
        currentRadius = Mathf.Lerp(0f, maxRadius, normalizedTime);

        // Apply damage to targets caught by the expanding ring.
        ApplyDamage(currentRadius);
        UpdateRing(currentRadius);

        if (normalizedTime >= 1f)
        {
            isActive = false;
            DisableRing();
        }
    }

    private void ApplyDamage(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            // Only process NPC-tagged enemies.
            if (!hit.CompareTag("NPC"))
            {
                continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                continue;
            }

            Component damageableComponent = damageable as Component;
            int id = damageableComponent != null ? damageableComponent.GetInstanceID() : damageable.GetHashCode();
            if (hitTargets.Contains(id))
            {
                continue;
            }

            damageable.TakeDamage(damage);
            hitTargets.Add(id);
        }
    }

    private void SetupRingRenderer()
    {
        if (ringRenderer != null)
        {
            return;
        }

        GameObject ringObject = new GameObject("ShockwaveRing");
        ringObject.transform.SetParent(transform, false);
        ringRenderer = ringObject.AddComponent<LineRenderer>();
    }

    private void EnableRing()
    {
        if (ringRenderer == null)
        {
            return;
        }

        ringRenderer.enabled = true;
        ringRenderer.loop = true;
        // Use local space so the ring follows the player without recalculating world offsets.
        ringRenderer.useWorldSpace = false;
        ringRenderer.positionCount = Mathf.Max(3, ringSegments);
        ringRenderer.startWidth = ringWidth;
        ringRenderer.endWidth = ringWidth;
        ringRenderer.material = ringMaterial != null ? ringMaterial : ringRenderer.material;
        ringRenderer.startColor = ringColor;
        ringRenderer.endColor = ringColor;
    }

    private void DisableRing()
    {
        if (ringRenderer != null)
        {
            ringRenderer.enabled = false;
        }
    }

    private void UpdateRing(float radius)
    {
        if (ringRenderer == null || !ringRenderer.enabled)
        {
            return;
        }

        int segments = Mathf.Max(3, ringSegments);
        if (ringRenderer.positionCount != segments)
        {
            ringRenderer.positionCount = segments;
        }

        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (angleStep * i);
            Vector3 position = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            ringRenderer.SetPosition(i, position);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isActive)
        {
            return;
        }

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}
