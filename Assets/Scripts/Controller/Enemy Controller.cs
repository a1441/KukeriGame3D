using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyController : MonoBehaviour
{
    public float lookRadius = 8f;
    Transform target;
    NavMeshAgent agent;
    CharacterCombat combat;

    [SerializeField] private LightController lightController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayerWithoutMask();
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void ChasePlayerWithoutMask()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        agent.SetDestination(target.position);

        if (distance <= agent.stoppingDistance)
        {
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                combat.Attack(targetStats);
            }
            FaceTarget();
        }
    }

    void ChasePlayerWithMask()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);
        }

        if (distance <= agent.stoppingDistance)
        {
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                combat.Attack(targetStats);
            }
            FaceTarget();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

}
