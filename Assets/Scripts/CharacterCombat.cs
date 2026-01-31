using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    public float attacksSpeed = 1f;
    public float attackDelay = 0.6f;

    private float attackCooldown = 0f;
    private CharacterStats myStats;

    public CharacterStats Stats => myStats;

    void Awake()
    {
        myStats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    public void Attack(CharacterStats targetStats)
    {
        if (targetStats == null) return;
        if (myStats == null) return;

        if (attackCooldown <= 0f)
        {
            StartCoroutine(DoDamage(targetStats, attackDelay));
            attackCooldown = (attacksSpeed <= 0f) ? 0f : (1f / attacksSpeed);
        }
    }

    IEnumerator DoDamage(CharacterStats targetStats, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetStats == null)
        {
            Debug.LogWarning($"[Combat] {name} attack canceled: targetStats is null");
            yield break;
        }

        int dmg = (myStats != null) ? myStats.DamageValue : 0;

        Debug.Log($"[Combat] {name} -> {targetStats.name} for {dmg} dmg (before HP: {targetStats.currentHealth})");

        targetStats.TakeDamage(dmg);

        Debug.Log($"[Combat] {name} -> {targetStats.name} done. (after HP: {targetStats.currentHealth})");
    }

}
