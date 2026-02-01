using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RangedAttack))]
public class RangedEnemyController : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float lookRadius = 15f;

    // Optional: if you want "shoot only when close enough"
    // If you want "shoot as soon as detected", set shootRadius = lookRadius.
    [SerializeField] private float shootRadius = 15f;
    [SerializeField] private float shootRadiusIncrement = 5f;

    private float defaultShootRadius;

    private Transform target;
    private NavMeshAgent agent;
    private RangedAttack ranged;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ranged = GetComponent<RangedAttack>();
        defaultShootRadius = shootRadius;
    }

    void Start()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
        {
            Debug.LogError("[RangedEnemyController] PlayerManager/player missing", this);
            enabled = false;
            return;
        }

        target = PlayerManager.instance.player.transform;

        // If you want them to keep chasing into melee range:
        // set a very small stopping distance

    }

    void Update()
    {
        if (!target || !agent || !ranged) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, target.position);

        //if (dist > lookRadius)
        //    return;

        //// Chase until we reach stoppingDistance, then stop
        //if (dist > agent.stoppingDistance)
        //{
        //    agent.isStopped = false;
        //    agent.SetDestination(target.position);
        //}
        //else
        //{
        //    agent.isStopped = true;
        //    agent.ResetPath();
        //    agent.velocity = Vector3.zero;
        //}

        // Shoot whenever detected (or use shootRadius if you want)
        if (dist <= shootRadius)
        {
            ranged.FireAt(target);
            FaceTarget();
        }
    }


    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    public void IncreaseShootRadius()
    {
        shootRadius += shootRadiusIncrement;
    }

    public void ResetShootRadius()
    {
        shootRadius = defaultShootRadius;
    }
}
