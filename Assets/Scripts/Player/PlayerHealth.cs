using TMPro;
using UnityEngine;

public class PlayerHealth : DamageableEntity
{
    [SerializeField, Min(1)] private int maxHealth = 5;
    [SerializeField] private TMP_Text healthText;

    private int currentHealth;
    private PlayerMovementInputSystem movement;
    private PlayerShooter shooter;

    private void Awake()
    {
        currentHealth = maxHealth;
        movement = GetComponent<PlayerMovementInputSystem>();
        shooter = GetComponent<PlayerShooter>();

        if (healthText == null)
        {
            Debug.LogError("PlayerHealth requires a health text on the HUD Canvas.", this);
        }

        UpdateHealthText();
    }

    public override bool TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthText();

        if (currentHealth == 0)
        {
            if (movement != null)
            {
                movement.enabled = false;
            }

            if (shooter != null)
            {
                shooter.enabled = false;
            }

            if (healthText != null)
            {
                healthText.text = "DERROTADO";
            }

            return true;
        }

        return false;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"VIDA: {currentHealth}/{maxHealth}";
        }
    }
}
