using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioZonaCiudad : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
            audioSource.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() != null)
            audioSource.Stop();
    }
}