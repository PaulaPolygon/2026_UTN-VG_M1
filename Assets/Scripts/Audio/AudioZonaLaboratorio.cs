using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioZonaLaboratorio : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private AudioMixerSnapshot snapshotLaboratorio;
    [SerializeField] private float duracionTransicion = 1.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
        {
            if (snapshotLaboratorio != null)
                snapshotLaboratorio.TransitionTo(duracionTransicion);
            else
                Debug.LogWarning("No se asignó un Audio Mixer Snapshot para esta zona.", this);

            audioSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
            audioSource.Stop();
    }
}