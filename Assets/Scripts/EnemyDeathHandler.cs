using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterStats))]
public class EnemyDeathHandler : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.5f;

    private CharacterStats _stats;
    private NavMeshAgent _agent;
    private EnemyController _enemyController;
    private CharacterCombat _combat;

    private bool _handled;

    void Awake()
    {
        _stats = GetComponent<CharacterStats>();
        _agent = GetComponent<NavMeshAgent>();
        _enemyController = GetComponent<EnemyController>();
        _combat = GetComponent<CharacterCombat>();
    }

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
        if (_handled) return;
        _handled = true;

        // Disable attack + AI
        if (_combat) _combat.enabled = false;
        if (_enemyController) _enemyController.enabled = false;

        // Stop movement
        if (_agent)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            _agent.enabled = false; // optional: hard-disable agent
        }

        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
