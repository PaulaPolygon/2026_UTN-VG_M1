using UnityEngine;

public class ParticleDistanceVisibility : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Min(0f)] private float visibleDistance = 10f;

    private ParticleSystemRenderer[] particleRenderers;
    private bool isVisible;

    private void Awake()
    {
        particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true);

        if (player == null)
        {
            Debug.LogError("Asigna el Transform del jugador en el Inspector.", this);
            enabled = false;
            return;
        }

        UpdateVisibility();
    }

    private void LateUpdate()
    {
        bool shouldBeVisible =
            (player.position - transform.position).sqrMagnitude
            <= visibleDistance * visibleDistance;

        if (shouldBeVisible != isVisible)
        {
            SetVisible(shouldBeVisible);
        }
    }

    private void UpdateVisibility()
    {
        bool shouldBeVisible =
            (player.position - transform.position).sqrMagnitude
            <= visibleDistance * visibleDistance;

        SetVisible(shouldBeVisible);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        foreach (ParticleSystemRenderer particleRenderer in particleRenderers)
        {
            particleRenderer.enabled = visible;
        }
    }
}