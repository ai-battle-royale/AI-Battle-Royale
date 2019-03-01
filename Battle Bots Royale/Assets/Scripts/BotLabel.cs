using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotLabel : MonoBehaviour
{
    public Slider HealthSlider;
    public Slider ArmorSlider;
    public Text Text;

    public void SetText (string text) {
        Text.text = text;
    }

    public void SetSliders(float health, float armor) {
        ArmorSlider.value = armor;
        HealthSlider.value = health;
    }
}
