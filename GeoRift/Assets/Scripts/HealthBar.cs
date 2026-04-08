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
        Vector2 size = slider.GetComponent<RectTransform>().sizeDelta;
        size.x = maxHealth * 1.6f;
        slider.GetComponent<RectTransform>().sizeDelta = size;
    }

    public void UpdateHealthBar(int currentHealth)
    {
        slider.value = currentHealth;
    }
}
