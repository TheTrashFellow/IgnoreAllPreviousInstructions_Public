using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    public void ChangerVieMax(int vie)
    {
        slider.maxValue = vie;
        slider.value = vie;

        fill.color = gradient.Evaluate(1f);
    }

    public void ChangerVie(int vie)
    {
        slider.value = vie;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
