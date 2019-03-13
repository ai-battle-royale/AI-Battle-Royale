using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotLabel : MonoBehaviour
{
    public Text     textObject;
    public Slider   healthSlider;
    public Slider   armorSlider;
    public Slider   healthActionSlider;
    public Slider   armorActionSlider;
    public Slider   progressSlider;

    public void SetText (string text) {
        textObject.text = text;
    }

    public void SetSliders(float health, float armor) {
        armorSlider.value = armor;
        healthSlider.value = health;
    }
}
