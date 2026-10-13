using System;
using TMPro;
using UnityEngine;

public class StrawberryCollector : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float collectionRange = 5f;
    [SerializeField, Min(1)] private int strawberriesRequiredForMission = 3;
    [SerializeField] private TMP_Text counterText;

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

            StrawberryCollectible strawberry =
                hit.collider.GetComponentInParent<StrawberryCollectible>();
            if (strawberry == null)
            {
                return false;
            }

            Destroy(strawberry.gameObject);
            strawberriesCollected++;
            UpdateCounter();

            if (!missionCompleted && strawberriesCollected >= strawberriesRequiredForMission)
            {
                missionCompleted = true;
                missionProgress.CompleteMission(MissionProgress.Objective.StrawberriesCollected);
            }

            return true;
        }

        return false;
    }

    private void UpdateCounter()
    {
        counterText.text = $"Frutillas: {strawberriesCollected}";
    }
}
