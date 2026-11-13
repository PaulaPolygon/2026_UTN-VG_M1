using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionProgress : MonoBehaviour
{
    public enum Objective
    {
        HunterDefeated,
        PeleteroDefeated,
        ButcherDefeated,
        StrawberriesCollected
    }

    [SerializeField, Min(1)] private int totalMissions = 5;
    [SerializeField] private Slider progressSlider;

    private readonly HashSet<Objective> completedObjectives = new HashSet<Objective>();

    private void Awake()
    {
        if (progressSlider == null)
        {
            Debug.LogError("MissionProgress requires a mission progress Slider.", this);
            enabled = false;
            return;
        }

        progressSlider.interactable = false;
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        UpdateSlider();

        Canvas canvas = progressSlider.GetComponentInParent<Canvas>(true);
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
            if (canvas.transform.localScale.sqrMagnitude < 0.0001f)
            {
                canvas.transform.localScale = Vector3.one;
            }
        }
    }

    public void CompleteMission(Objective objective)
    {
        if (completedObjectives.Add(objective))
        {
            UpdateSlider();
        }
    }

    private void UpdateSlider()
    {
        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01(
                (float)completedObjectives.Count / Mathf.Max(1, totalMissions)
            );
        }
    }
}
