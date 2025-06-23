using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows Player health on the healthbar
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    private Slider healthBar;
    private EntityHealth playerHealth;

    void Awake()
    {
        healthBar = GetComponent<Slider>();

        if (healthBar == null)
        {
            Debug.LogError("Slider component missing from " + gameObject.name);
            return;
        }
    }

    public void SetPlayer(EntityHealth player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerHealthUI: Assigned player is null!");
            return;
        }

        playerHealth = player;
        playerHealth.currentHealth.OnValueChanged += UpdateHealthUI;
        UpdateHealthUI(0, playerHealth.currentHealth.Value);
    }

    private void UpdateHealthUI(int oldValue, int newValue)
    {
        if (healthBar != null && playerHealth != null)
        {
            healthBar.value = (float)newValue / playerHealth.maxHealth;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.currentHealth.OnValueChanged -= UpdateHealthUI;
        }
    }
}
