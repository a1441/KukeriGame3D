using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public enum EnemyType
    {
        Ranged,
        Melee
    }

    private enum AIState
    {
        IdlePatrol,
        Chase,
        MeleeAttack,
        RangedAttack
    }

    [Header("General")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    [SerializeField] private Transform player;
    [SerializeField] private bool playerHas = true;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Melee")]
    [SerializeField] private float meleeAttackRange = 2f;
    [SerializeField] private float attackCooldown = 1.25f;

    [Header("Ranged")]
    [SerializeField] private float rangedFireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Idle Patrol")]
    [SerializeField] private List<Transform> idlePoints = new List<Transform>();
    [SerializeField] private float idleWaitTime = 1f;

    private AIState currentState = AIState.IdlePatrol;
    private NavMeshAgent navMeshAgent;
    private int currentIdleIndex;
    private float idleWaitTimer;
    private float attackTimer;
    private float rangedTimer;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
        }

        attackTimer = attackCooldown;
        rangedTimer = 1f / Mathf.Max(0.01f, rangedFireRate);
    }

    private void Update()
    {
        bool playerDetected = IsPlayerDetected();

        if (!playerDetected)
        {
            if (currentState != AIState.IdlePatrol)
            {
                TransitionToState(AIState.IdlePatrol);
            }
        }
        else
        {
            if (enemyType == EnemyType.Melee)
            {
                if (currentState == AIState.IdlePatrol || currentState == AIState.RangedAttack)
                {
                    TransitionToState(AIState.Chase);
                }
            }
            else
            {
                if (currentState != AIState.RangedAttack)
                {
                    TransitionToState(AIState.RangedAttack);
                }
            }
        }

        switch (currentState)
        {
            case AIState.IdlePatrol:
                HandleIdlePatrol();
                break;
            case AIState.Chase:
                HandleChase(playerDetected);
                break;
            case AIState.MeleeAttack:
                HandleMeleeAttack(playerDetected);
                break;
            case AIState.RangedAttack:
                HandleRangedAttack(playerDetected);
                break;
        }
    }

    private bool IsPlayerDetected()
    {
        if (!playerHas || player == null)
        {
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }

    private void HandleIdlePatrol()
    {
        if (idlePoints.Count == 0)
        {
            StopMovement();
            return;
        }

        Transform targetPoint = idlePoints[currentIdleIndex];
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (distance <= 0.25f)
        {
            StopMovement();
            idleWaitTimer += Time.deltaTime;

            if (idleWaitTimer >= idleWaitTime)
            {
                idleWaitTimer = 0f;
                currentIdleIndex = (currentIdleIndex + 1) % idlePoints.Count;
            }
        }
        else
        {
            MoveTo(targetPoint.position);
        }
    }

    private void HandleChase(bool playerDetected)
    {
        if (!playerDetected)
        {
            TransitionToState(AIState.IdlePatrol);
            return;
        }

        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= meleeAttackRange)
        {
            TransitionToState(AIState.MeleeAttack);
            return;
        }

        MoveTo(player.position);
    }

    private void HandleMeleeAttack(bool playerDetected)
    {
        if (!playerDetected)
        {
            TransitionToState(AIState.IdlePatrol);
            return;
        }

        if (player == null)
        {
            TransitionToState(AIState.IdlePatrol);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > meleeAttackRange)
        {
            TransitionToState(AIState.Chase);
            return;
        }

        StopMovement();
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            PerformMeleeAttack();
        }
    }

    private void HandleRangedAttack(bool playerDetected)
    {
        if (!playerDetected)
        {
            TransitionToState(AIState.IdlePatrol);
            return;
        }

        if (player == null)
        {
            TransitionToState(AIState.IdlePatrol);
            return;
        }

        StopMovement();
        rangedTimer += Time.deltaTime;

        float fireInterval = 1f / Mathf.Max(0.01f, rangedFireRate);
        if (rangedTimer >= fireInterval)
        {
            rangedTimer = 0f;
            FireProjectile();
        }
    }

    private void MoveTo(Vector3 destination)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.SetDestination(destination);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        }
    }

    private void StopMovement()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
    }

    private void PerformMeleeAttack()
    {
        Debug.Log("Enemy performed melee attack.");
        DealDamageToPlayer();
    }

    private void DealDamageToPlayer()
    {
        Debug.Log("DealDamageToPlayer() called.");
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned.");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + transform.forward;
        Quaternion spawnRotation = Quaternion.LookRotation((player.position - spawnPosition).normalized, Vector3.up);

        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        ProjectilePlaceholder projectile = projectileInstance.GetComponent<ProjectilePlaceholder>();
        if (projectile != null)
        {
            projectile.SetSpeed(projectileSpeed);
        }
    }

    private void TransitionToState(AIState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
        Debug.Log($"Enemy state changed to {newState}.");

        if (newState == AIState.IdlePatrol)
        {
            idleWaitTimer = 0f;
        }

        if (newState == AIState.Chase)
        {
            attackTimer = attackCooldown;
        }
    }
}
