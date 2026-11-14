using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : DamageableEntity
{
    [SerializeField, Min(1)] private int maxHealth = 5;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject victoryPanel;

    private int currentHealth;
    private PlayerMovementInputSystem movement;
    private PlayerShooter shooter;
    private bool gameEnded;

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
        if (amount <= 0 || currentHealth <= 0 || gameEnded)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthText();

        if (currentHealth == 0)
        {
            if (healthText != null)
            {
                healthText.text = "DERROTADO";
            }

            EndGame(defeatPanel, "PlayerHealth requires the defeat panel.");
            return true;
        }

        return false;
    }

    public void MostrarVictoria()
    {
        EndGame(victoryPanel, "PlayerHealth requires the victory panel.");
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("0_Inicio");
    }

    public void VolverAJugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void EndGame(GameObject panel, string missingPanelMessage)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (shooter != null)
        {
            shooter.enabled = false;
        }

        if (panel == null)
        {
            Debug.LogError(missingPanelMessage, this);
        }
        else
        {
            panel.SetActive(true);
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"VIDA: {currentHealth}/{maxHealth}";
        }
    }
}
