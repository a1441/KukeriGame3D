using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public Stat health;

    [Header("Offense")]
    public Stat damage;

    public int currentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action OnDied;   // <- enemies can subscribe

    private HitFlash _hitFlash;

    public int MaxHealthValue => (health != null) ? health.GetValue() : maxHealth;
    public int DamageValue => (damage != null) ? damage.GetValue() : 0;

    void Awake()
    {
        currentHealth = MaxHealthValue;
        _hitFlash = GetComponentInChildren<HitFlash>(includeInactive: true);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        amount = Mathf.Max(0, amount);
        if (amount <= 0) return;

        currentHealth -= amount;

        // AudioManager.PlaySound(SoundType.Hit, 0.7f);

        if (_hitFlash != null)
            _hitFlash.Flash();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            IsDead = true;
            Die();
        }
    }

    void Die()
    {
        OnDied?.Invoke();
        Debug.Log($"{transform.name} died.");
    }
}
