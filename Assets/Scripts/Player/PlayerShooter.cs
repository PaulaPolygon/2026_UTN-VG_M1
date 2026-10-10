using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float fireInterval = 0.25f;
    [SerializeField, Min(0.01f)] private float range = 100f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.01f)] private float tracerDuration = 0.25f;
    [SerializeField, Min(0.001f)] private float tracerWidth = 0.18f;
    [SerializeField, Min(0.01f)] private float muzzleFlashSize = 0.45f;

    private Camera aimCamera;
    private Animator animator;
    private LineRenderer tracer;
    private LineRenderer muzzleFlash;
    private Material tracerMaterial;
    private Coroutine hideTracerCoroutine;
    private float nextFireTime;
    private bool hasAttackParameter;

    private void Awake()
    {
        aimCamera = Camera.main;
        animator = GetComponentInChildren<Animator>();
        hasAttackParameter = animator != null && Array.Exists(
            animator.parameters,
            parameter => parameter.name == "Attack" &&
                         parameter.type == AnimatorControllerParameterType.Bool
        );

        if (aimCamera == null)
        {
            Debug.LogError("PlayerShooter requires a camera tagged MainCamera.", this);
            enabled = false;
            return;
        }

        CreateTracer();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireInterval;
        if (hasAttackParameter)
        {
            animator.SetBool("Attack", true);
            CancelInvoke(nameof(ResetAttackAnimation));
            Invoke(nameof(ResetAttackAnimation), 0.2f);
        }

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit[] hits = Physics.RaycastAll(ray, range);
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        Vector3 tracerEnd = ray.GetPoint(range);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            tracerEnd = hit.point;
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            break;
        }

        ShowTracer(ray.origin, tracerEnd);
    }

    private void CreateTracer()
    {
        GameObject tracerObject = new GameObject("Shot Tracer");
        tracerObject.transform.SetParent(transform, false);
        tracer = tracerObject.AddComponent<LineRenderer>();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.startWidth = tracerWidth;
        tracer.endWidth = tracerWidth * 0.2f;
        tracer.startColor = new Color(1f, 0.9f, 0.35f, 1f);
        tracer.endColor = new Color(1f, 0.45f, 0.1f, 0.15f);
        tracer.numCapVertices = 4;
        tracer.alignment = LineAlignment.View;
        tracer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tracer.receiveShadows = false;
        tracer.enabled = false;

        GameObject flashObject = new GameObject("Muzzle Flash");
        flashObject.transform.SetParent(transform, false);
        muzzleFlash = flashObject.AddComponent<LineRenderer>();
        muzzleFlash.useWorldSpace = true;
        muzzleFlash.positionCount = 17;
        muzzleFlash.startWidth = tracerWidth * 0.8f;
        muzzleFlash.endWidth = tracerWidth * 0.8f;
        muzzleFlash.startColor = new Color(1f, 1f, 0.8f, 1f);
        muzzleFlash.endColor = new Color(1f, 0.55f, 0.05f, 1f);
        muzzleFlash.numCapVertices = 4;
        muzzleFlash.alignment = LineAlignment.View;
        muzzleFlash.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        muzzleFlash.receiveShadows = false;
        muzzleFlash.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            Debug.LogError("PlayerShooter could not find a shader for the shot tracer.", this);
            Destroy(tracerObject);
            tracer = null;
            return;
        }

        tracerMaterial = new Material(shader);
        tracer.material = tracerMaterial;
        muzzleFlash.material = tracerMaterial;
    }

    private void ShowTracer(Vector3 start, Vector3 end)
    {
        if (tracer == null)
        {
            return;
        }

        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);
        tracer.enabled = true;

        Vector3 direction = (end - start).normalized;
        Vector3 center = start + direction * muzzleFlashSize;
        Vector3 right = aimCamera.transform.right * muzzleFlashSize;
        Vector3 up = aimCamera.transform.up * muzzleFlashSize;
        Vector3[] flashPoints =
        {
            center - right, center, center + right, center,
            center - up, center, center + up, center,
            center - (right + up) * 0.7f, center, center + (right + up) * 0.7f, center,
            center + (right - up) * 0.7f, center, center - (right - up) * 0.7f, center,
            center - right
        };
        muzzleFlash.SetPositions(flashPoints);
        muzzleFlash.enabled = true;

        if (hideTracerCoroutine != null)
        {
            StopCoroutine(hideTracerCoroutine);
        }

        hideTracerCoroutine = StartCoroutine(HideTracerAfterDelay());
    }

    private IEnumerator HideTracerAfterDelay()
    {
        yield return new WaitForSeconds(tracerDuration);
        if (tracer != null)
        {
            tracer.enabled = false;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = false;
        }

        hideTracerCoroutine = null;
    }

    private void ResetAttackAnimation()
    {
        if (animator != null && hasAttackParameter)
        {
            animator.SetBool("Attack", false);
        }
    }

    private void OnDestroy()
    {
        if (tracerMaterial != null)
        {
            Destroy(tracerMaterial);
        }
    }
}
