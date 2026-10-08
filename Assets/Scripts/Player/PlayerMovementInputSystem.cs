using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementInputSystem : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSmoothTime = 0.08f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector2 moveInput;
    private float turnSmoothVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerMovementInputSystem requires a camera tagged MainCamera.", this);
            enabled = false;
            return;
        }

        cameraTransform = mainCamera.transform;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 move = cameraRight * moveInput.x + cameraForward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (move.sqrMagnitude >= 0.0001f)
        {
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = new Vector3(move.x * moveSpeed, verticalVelocity, move.z * moveSpeed);
        controller.Move(velocity * Time.deltaTime);
    }
}
