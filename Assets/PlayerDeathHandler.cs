using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerDeathHandler : MonoBehaviour
{
    public static event Action OnPlayerDied;

    private CharacterStats _stats;
    private bool _fired;

    void Awake() => _stats = GetComponent<CharacterStats>();

    void OnEnable()
    {
        if (_stats != null)
            _stats.OnDied += HandleDeath;
    }

    void OnDisable()
    {
        if (_stats != null)
            _stats.OnDied -= HandleDeath;
    }

    void HandleDeath()
    {
        if (_fired) return;
        _fired = true;
        OnPlayerDied?.Invoke();
    }
}
