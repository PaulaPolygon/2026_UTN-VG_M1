using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioZonaCiudad : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private AudioMixerSnapshot snapshotCiudad;
    [SerializeField] private float duracionTransicion = 1.5f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
        {
            if (snapshotCiudad != null)
                snapshotCiudad.TransitionTo(duracionTransicion);
            else
                Debug.LogWarning("No se asignó un Audio Mixer Snapshot para esta zona.", this);

            audioSource.Play();

            if (AudioVozController.Instance != null)
                AudioVozController.Instance.ReproducirVozCiudad();
            else
                Debug.LogWarning("No se encontró un AudioVozController en la escena.", this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
            audioSource.Stop();
    }
}