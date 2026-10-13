using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementInputSystem : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSmoothTime = 0.08f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Animator animator;
    private PlayerShooter shooter;
    private Transform cameraTransform;
    private Vector2 moveInput;
    private float turnSmoothVelocity;
    private float verticalVelocity;
    private bool jumpRequested;
    private bool jumpAvailable = true;
    private bool hasJumpTrigger;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        shooter = GetComponent<PlayerShooter>();
        hasJumpTrigger = animator != null && System.Array.Exists(
            animator.parameters,
            parameter => parameter.name == "Jump" &&
                         parameter.type == AnimatorControllerParameterType.Trigger
        );

        if (animator != null && !hasJumpTrigger)
        {
            Debug.LogWarning(
                "The player's Animator has no Trigger parameter named 'Jump'; jumping will be physical only.",
                this
            );
        }

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

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

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

        if (animator != null)
        {
            animator.SetBool("Run", moveInput.y > 0.1f);
            animator.SetBool("RunLeft", moveInput.x < -0.1f);
            animator.SetBool("RunRight", moveInput.x > 0.1f);
            animator.SetBool("RunBack", moveInput.y < -0.1f);
        }

        Vector3 facingDirection = move;
        if (shooter != null && shooter.IsAiming)
        {
            facingDirection = shooter.AimDirection;
            facingDirection.y = 0f;
        }

        if (facingDirection.sqrMagnitude >= 0.0001f)
        {
            float targetAngle = Mathf.Atan2(facingDirection.x, facingDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        bool isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
        }

        if (jumpRequested && jumpAvailable)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpAvailable = false;
            if (hasJumpTrigger)
            {
                animator.SetTrigger("Jump");
            }
        }

        jumpRequested = false;
        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = new Vector3(move.x * moveSpeed, verticalVelocity, move.z * moveSpeed);
        CollisionFlags collisionFlags = controller.Move(velocity * Time.deltaTime);

        if ((collisionFlags & CollisionFlags.Below) != 0 && verticalVelocity <= 0f)
        {
            jumpAvailable = true;
            verticalVelocity = -2f;
        }
    }
}
