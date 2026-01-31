using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;

    // (damageAmount, attackerTransform)
    public event Action<float, Transform> OnHit;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    public void TakeDamage(float damage) => TakeDamage(damage, null);

    public void TakeDamage(float damage, Transform source)
    {
        float prev = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (damage > 0f && prev > 0f)
            OnHit?.Invoke(damage, source);
    }
}
