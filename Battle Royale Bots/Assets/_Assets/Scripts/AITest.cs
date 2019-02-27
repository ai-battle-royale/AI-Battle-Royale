using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour {

    bool canSeeEnemy;
    Vector3 direction;
    AIController Controller;

    void Start()  {
        Controller = GetComponent<AIController>();

        direction = new Vector3(Random.value, 0, Random.value);
    }

    void Update() {
        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8) {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = Controller.Scan(dir);

            if (scan.Type == HitType.World) {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.Distance / Controller.MaxLookDistance) );
            } else if (scan.Type == HitType.Enemy) {
                //Controller.Shoot(dir);
                direction = scan.Distance > 2f ? dir : -dir;

                break;
            }
        }

        Controller.Shoot(Vector3.forward);
        //Controller.Move(direction);       
    }
}
