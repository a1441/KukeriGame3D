using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterStats))]
public class EnemyDeathHandler : MonoBehaviour
{
    private float destroyDelay = 1f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isDeadBool = "isDead";

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

        if (!animator)
            animator = GetComponentInChildren<Animator>(true);
            

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
            _agent.enabled = false;
        }

        // Play death animation via bool
        if (animator)
            animator.SetBool(isDeadBool, true);

        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
