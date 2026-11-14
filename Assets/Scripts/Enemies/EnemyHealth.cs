using System;
using UnityEngine;

public class EnemyHealth : DamageableEntity
{
    public event Action<EnemyHealth> Died;

    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField] private ParticleSystem deathParticles;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public override bool TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (currentHealth == 0)
        {
            Died?.Invoke(this);

            if (deathParticles != null)
            {
                ParticleSystem particles = Instantiate(
                    deathParticles,
                    transform.position,
                    transform.rotation
                );
                particles.Play();

                ParticleSystem.MainModule main = particles.main;
                Destroy(
                    particles.gameObject,
                    main.duration + main.startDelay.constantMax + main.startLifetime.constantMax
                );
            }

            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
