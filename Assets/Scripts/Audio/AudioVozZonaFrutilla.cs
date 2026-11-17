using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioVozZonaFrutilla : MonoBehaviour
{
    private void Awake()
    {
        Collider zoneCollider = GetComponent<Collider>();
        if (!zoneCollider.isTrigger)
        {
            Debug.LogError("El collider de la zona de frutillas debe tener Is Trigger activado.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementInputSystem>() == null)
        {
            return;
        }

        if (AudioVozController.Instance != null)
        {
            AudioVozController.Instance.ReproducirVozFrutilla();
        }
        else
        {
            Debug.LogWarning("No se encontró un AudioVozController en la escena.", this);
        }
    }
}
