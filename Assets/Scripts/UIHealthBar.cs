using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Fighter fighter;

    private void Start()
    {
        // Set the slider's max value based on the fighter's stats
        //healthSlider.maxValue = fighter.MaxHealth;
        //healthSlider.value = fighter.CurrentHealth;

        // Subscribe to a health change event if you have one
        //fighter.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(int newHealth)
    {
        healthSlider.value = newHealth;
    }
}