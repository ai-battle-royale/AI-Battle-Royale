using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeScaleSlider : MonoBehaviour {

    private Slider slider;

    void Start() {
        slider = GetComponent<Slider>();
    }

    void Update() {
        Time.timeScale = slider.value;
    }
}
