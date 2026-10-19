using UnityEngine;

public class HunterAim : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("HunterAim needs a target Transform assigned.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }
}
