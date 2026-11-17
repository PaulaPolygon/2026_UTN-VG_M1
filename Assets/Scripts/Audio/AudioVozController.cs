using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioVozController : MonoBehaviour
{
    public static AudioVozController Instance { get; private set; }

    [SerializeField] private AudioClip vozBosque;
    [SerializeField] private AudioClip vozCiudad;
    [SerializeField] private AudioClip vozFrutilla;

    private AudioSource audioSource;
    private readonly Queue<AudioClip> vocesEnCola = new Queue<AudioClip>();
    private bool vozBosqueReproducida;
    private bool vozCiudadReproducida;
    private bool vozFrutillaReproducida;
    private Coroutine reproduccionEnCurso;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Solo debe existir un AudioVozController en la escena.", this);
            enabled = false;
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ReproducirVozBosque()
    {
        ReproducirUnaVez(vozBosque, ref vozBosqueReproducida, "vozBosque");
    }

    public void ReproducirVozCiudad()
    {
        ReproducirUnaVez(vozCiudad, ref vozCiudadReproducida, "vozCiudad");
    }

    public void ReproducirVozFrutilla()
    {
        ReproducirUnaVez(vozFrutilla, ref vozFrutillaReproducida, "vozFrutilla");
    }

    private void ReproducirUnaVez(AudioClip clip, ref bool yaReproducida, string nombreVoz)
    {
        if (yaReproducida)
        {
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning($"No se asignó el clip {nombreVoz}.", this);
            return;
        }

        yaReproducida = true;
        vocesEnCola.Enqueue(clip);

        if (reproduccionEnCurso == null)
        {
            reproduccionEnCurso = StartCoroutine(ReproducirVocesEnCola());
        }
    }

    private IEnumerator ReproducirVocesEnCola()
    {
        while (vocesEnCola.Count > 0)
        {
            audioSource.PlayOneShot(vocesEnCola.Dequeue());
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        reproduccionEnCurso = null;
    }
}
