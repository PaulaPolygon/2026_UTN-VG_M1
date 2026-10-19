using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioZonaBosque : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
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