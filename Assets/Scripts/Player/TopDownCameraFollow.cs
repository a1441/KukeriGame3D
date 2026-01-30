using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Camera Settings")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private bool smoothFollow = true;

    private Vector3 initialOffset;
    private bool offsetCalculated = false;

    private void Start()
    {
        // Try to find player if target not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Calculate initial offset based on current positions
        if (target != null)
        {
            initialOffset = transform.position - target.position;
            offsetCalculated = true;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Calculate initial offset if not done yet (in case target was assigned after Start)
        if (!offsetCalculated)
        {
            initialOffset = transform.position - target.position;
            offsetCalculated = true;
        }

        // Calculate desired position maintaining the initial offset
        Vector3 desiredPosition = target.position + initialOffset;
        
        // Smooth follow or instant
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
        
        // Camera rotation is NOT changed - it maintains whatever rotation you set in the scene
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        // Recalculate offset when target changes
        if (target != null)
        {
            initialOffset = transform.position - target.position;
            offsetCalculated = true;
        }
    }
}
