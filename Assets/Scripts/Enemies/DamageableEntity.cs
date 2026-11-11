using UnityEngine;

public abstract class DamageableEntity : MonoBehaviour
{
    public abstract bool TakeDamage(int amount);
}
