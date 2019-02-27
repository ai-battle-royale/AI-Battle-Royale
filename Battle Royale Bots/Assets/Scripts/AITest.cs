using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{

    public float moveAngle;
    AIController Controller;

    void Start()  {
        Controller = GetComponent<AIController>();

        moveAngle = Random.value * 360;
    }

    void Update() {
        Controller.Move(moveAngle);

        for (var i = 0f; i < 360; i += 10) {
            var scan = Controller.Scan(i);

            if (scan.Type == HitType.World)
            {
                moveAngle += (i - moveAngle) * 0.25f;
            }
        }
    }
}
