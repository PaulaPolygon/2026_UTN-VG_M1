using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementClassicInput : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(horizontal, 0f, vertical).normalized;

        if (move.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(move);
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
    }
}
