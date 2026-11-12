using System;
using System.Collections;
using UnityEngine;

public class HunterShooter : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.1f)] private float range = 30f;
    [SerializeField, Min(0.1f)] private float fireInterval = 2.5f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float targetHeight = 1f;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField, Min(0.01f)] private float tracerDuration = 0.12f;
    [SerializeField, Min(0.001f)] private float tracerWidth = 0.04f;

    private PlayerHealth targetHealth;
    private LineRenderer tracer;
    private Material tracerMaterial;
    private Coroutine hideTracerCoroutine;
    private float nextShotTime;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("HunterShooter needs the player's Transform assigned.", this);
            enabled = false;
            return;
        }

        targetHealth = target.GetComponentInParent<PlayerHealth>();
        if (targetHealth == null)
        {
            Debug.LogError("HunterShooter target needs a PlayerHealth component.", this);
            enabled = false;
            return;
        }

        CreateTracer();
    }

    private void Start()
    {
        nextShotTime = Time.time + fireInterval;
    }

    private void Update()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + fireInterval;

        Vector3 origin = transform.position + Vector3.up * targetHeight;
        Vector3 targetPoint = target.position + Vector3.up * targetHeight;
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;
        if (distance > range || distance < 0.001f)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction / distance,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            PlayerHealth hitPlayer = hit.collider.GetComponentInParent<PlayerHealth>();
            if (hitPlayer != targetHealth)
            {
                return;
            }

            targetHealth.TakeDamage(damage);
            ShowShotParticles(origin, direction);
            ShowTracer(origin, hit.point);
            return;
        }
    }

    private void ShowShotParticles(Vector3 position, Vector3 direction)
    {
        if (shotParticles == null)
        {
            return;
        }

        ParticleSystem particles = Instantiate(
            shotParticles,
            position,
            Quaternion.LookRotation(direction)
        );
        particles.Play();

        ParticleSystem.MainModule main = particles.main;
        Destroy(
            particles.gameObject,
            main.duration + main.startDelay.constantMax + main.startLifetime.constantMax
        );
    }

    private void CreateTracer()
    {
        GameObject tracerObject = new GameObject("Hunter Shot Tracer");
        tracerObject.transform.SetParent(transform, false);
        tracer = tracerObject.AddComponent<LineRenderer>();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.startWidth = tracerWidth;
        tracer.endWidth = tracerWidth * 0.2f;
        tracer.startColor = new Color(1f, 0.8f, 0.25f, 1f);
        tracer.endColor = new Color(1f, 0.35f, 0.1f, 0.2f);
        tracer.alignment = LineAlignment.View;
        tracer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tracer.receiveShadows = false;
        tracer.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            Debug.LogError("HunterShooter could not find a shader for its shot tracer.", this);
            Destroy(tracerObject);
            tracer = null;
            return;
        }

        tracerMaterial = new Material(shader);
        tracer.material = tracerMaterial;
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

        hideTracerCoroutine = null;
    }

    private void OnDestroy()
    {
        if (tracerMaterial != null)
        {
            Destroy(tracerMaterial);
        }
    }
}
