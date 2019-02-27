using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{

    public Vector3 direction;
    AIController Controller;

    void Start()  {
        Controller = GetComponent<AIController>();

        direction = new Vector3(Random.value, 0, Random.value);
    }

    void Update() {
        Controller.Move(direction);

        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 4) {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = Controller.Scan(dir);

            if (scan.Type == HitType.World)
            {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.Distance / Controller.MaxLookDistance) );
            }
        }
    }
}
