using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    Text text;

    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60F);
        int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad - minutes * 60);
        text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
