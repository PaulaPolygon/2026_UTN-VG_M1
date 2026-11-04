using System;
using TMPro;
using UnityEngine;

public class StrawberryCollector : MonoBehaviour
{
    private const float GroundContactTolerance = 0.2f;

    [SerializeField, Min(0.1f)] private float collectionRange = 5f;
    [SerializeField, Min(1)] private int strawberriesRequiredForMission = 3;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private ParticleSystem collectionParticles;
    [SerializeField] private AudioClip collectionSound;

    private MissionProgress missionProgress;
    private int strawberriesCollected;
    private bool missionCompleted;

    private void Awake()
    {
        missionProgress = GetComponent<MissionProgress>();
        if (missionProgress == null)
        {
            Debug.LogError("StrawberryCollector requires a MissionProgress component.", this);
            enabled = false;
            return;
        }

        if (counterText == null)
        {
            Debug.LogError("StrawberryCollector requires a strawberry counter TMP text.", this);
            enabled = false;
            return;
        }

        UpdateCounter();
    }

    public bool TryCollect(Ray aimRay)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            aimRay,
            collectionRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            Transform strawberry = GetStrawberryTransform(hit.collider);
            if (strawberry != null)
            {
                Collect(strawberry);
                return true;
            }

            if (!IsCollectionGround(hit) || aimRay.direction.y >= 0f)
            {
                return false;
            }

            Transform nearbyStrawberry = FindStrawberryImmediatelyAfter(hits, hit);
            if (nearbyStrawberry == null)
            {
                return false;
            }

            Collect(nearbyStrawberry);
            return true;
        }

        return false;
    }

    private void Collect(Transform strawberry)
    {
        if (collectionSound != null)
        {
            AudioSource.PlayClipAtPoint(collectionSound, strawberry.position);
        }

        if (collectionParticles != null)
        {
            ParticleSystem particles = Instantiate(
                collectionParticles,
                strawberry.position,
                strawberry.rotation
            );
            particles.Play();

            ParticleSystem.MainModule main = particles.main;
            Destroy(
                particles.gameObject,
                main.duration + main.startDelay.constantMax + main.startLifetime.constantMax
            );
        }

        Destroy(strawberry.gameObject);
        strawberriesCollected++;
        UpdateCounter();

        if (!missionCompleted && strawberriesCollected >= strawberriesRequiredForMission)
        {
            missionCompleted = true;
            missionProgress.CompleteMission(MissionProgress.Objective.StrawberriesCollected);
        }
    }

    private Transform FindStrawberryImmediatelyAfter(RaycastHit[] hits, RaycastHit groundHit)
    {
        foreach (RaycastHit nextHit in hits)
        {
            if (nextHit.distance <= groundHit.distance ||
                nextHit.transform == transform ||
                nextHit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (nextHit.distance - groundHit.distance > GroundContactTolerance)
            {
                return null;
            }

            return GetStrawberryTransform(nextHit.collider);
        }

        return null;
    }

    private static Transform GetStrawberryTransform(Collider hitCollider)
    {
        StrawberryCollectible strawberry =
            hitCollider.GetComponentInParent<StrawberryCollectible>();
        if (strawberry != null)
        {
            return strawberry.transform;
        }

        Transform current = hitCollider.transform;
        while (current != null)
        {
            if (current.CompareTag("Strawberry") ||
                current.name.StartsWith("strawberry", StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private static bool IsCollectionGround(RaycastHit hit)
    {
        string hitName = hit.collider.name;
        return hitName.IndexOf("land", StringComparison.OrdinalIgnoreCase) >= 0 ||
               hitName.IndexOf("ground", StringComparison.OrdinalIgnoreCase) >= 0 ||
               hitName.IndexOf("terrain", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void UpdateCounter()
    {
        counterText.text = $"Frutillas: {strawberriesCollected}";
    }
}
