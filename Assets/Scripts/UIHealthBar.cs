using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Fighter fighter;

    private float maxHealthValue;
    private float healthValue;

    private void Start()
    {
        // Set the slider's max value based on the fighter's stats
        maxHealthValue = fighter.GetMaxHealth();
        healthValue = fighter.GetHealth();

        // Subscribe to a health change event if you have one
        fighter.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(object sender, int newHealth)
    {
        float fillRatio = (float)newHealth / maxHealthValue;
        healthFillImage.fillAmount = fillRatio;
        Debug.Log("UpdateHealthBar to " + newHealth);
    }
}