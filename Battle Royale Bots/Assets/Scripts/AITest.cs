using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{

    AIController Controller;

    void Start()  {
        Controller = GetComponent<AIController>();
    }

    void Update() {       
        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI/10) {
            Controller.Scan(i);
        }

        Controller.Move(45);
    }
}
