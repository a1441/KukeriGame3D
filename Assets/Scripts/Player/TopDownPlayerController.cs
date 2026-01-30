using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController characterController;
    private Vector3 inputVector;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ReadInput();
        Move();
        Rotate();
    }

    private void ReadInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);
        inputVector = Vector3.ClampMagnitude(rawInput, 1f);
    }

    private void Move()
    {
        Vector3 velocity = inputVector * moveSpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void Rotate()
    {
        if (inputVector.sqrMagnitude <= 0f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(inputVector, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
