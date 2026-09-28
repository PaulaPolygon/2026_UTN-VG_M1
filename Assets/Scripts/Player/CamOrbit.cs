using UnityEngine;
using UnityEngine.InputSystem;

public class CamOrbit : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float targetHeight = 1.5f;

    private float yaw;
    private float pitch = 20f;

    private void LateUpdate()
    {
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -20f, 70f);
        }

        Vector3 focus = target.position + Vector3.up * targetHeight;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);

        transform.position = focus + orbit * new Vector3(0f, 0f, -distance);
        transform.rotation = orbit;
    }
}