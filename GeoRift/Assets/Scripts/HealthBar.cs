using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void InitializeHealthBar(int maxHealth)
    {
        slider.maxValue = maxHealth;
    }

    public void UpdateHealthBar(int currentHealth)
    {
        slider.value = currentHealth;
    }
}
