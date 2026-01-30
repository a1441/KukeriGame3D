using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform attackPoint;

    private CharacterController characterController;
    private Vector3 inputVector;
    private Vector3 velocity;
    private float lastAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // If camera not assigned, try to find main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Create attack point if not assigned
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(0f, 1f, 0.5f);
            attackPoint = attackPointObj.transform;
        }
    }

    private void Update()
    {
        ReadInput();
        HandleGravity();
        Move();
        Rotate();
        HandleAttack();
    }

    private void ReadInput()
    {
        // WASD input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);
        inputVector = Vector3.ClampMagnitude(rawInput, 1f);
        
        // Convert input to camera-relative direction for 3D RPG movement
        if (playerCamera != null && inputVector.magnitude > 0.1f)
        {
            Vector3 cameraForward = playerCamera.transform.forward;
            Vector3 cameraRight = playerCamera.transform.right;
            
            // Project camera forward onto horizontal plane
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate movement direction relative to camera
            inputVector = (cameraForward * vertical + cameraRight * horizontal).normalized;
        }
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
        
        velocity.y += gravity * Time.deltaTime;
    }

    private void Move()
    {
        Vector3 moveDirection = inputVector * moveSpeed;
        moveDirection.y = velocity.y;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void Rotate()
    {
        if (inputVector.sqrMagnitude <= 0.1f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(inputVector, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleAttack()
    {
        // Left mouse button attack
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        
        // Perform attack - check for enemies in range
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);
        
        foreach (Collider enemy in hitEnemies)
        {
            // Try to get a health component or damageable interface
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
            else
            {
                // Fallback: try to find a health script
                MonoBehaviour[] components = enemy.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour comp in components)
                {
                    if (comp.GetType().Name.Contains("Health") || comp.GetType().Name.Contains("Enemy"))
                    {
                        // Use reflection or add proper interface later
                        Debug.Log($"Attacked {enemy.name} for {attackDamage} damage");
                        break;
                    }
                }
            }
        }
        
        // Visual feedback (you can add animation triggers here)
        Debug.Log("Player attacked!");
        
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw attack range in editor
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}

// Interface for damageable objects
public interface IDamageable
{
    void TakeDamage(float damage);
}
