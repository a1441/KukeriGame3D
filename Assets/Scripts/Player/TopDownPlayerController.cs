using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    private CharacterController characterController;
    private Vector3 inputVector;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // If camera not assigned, try to find main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        ReadInput();
        HandleGravity();
        Move();
        Rotate();
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
}

// Interface for damageable objects
public interface IDamageable
{
    void TakeDamage(float damage);
}
