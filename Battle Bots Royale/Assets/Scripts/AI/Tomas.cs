using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tomas : MonoBehaviour
{
    BattleBotInterface Manager;
    private Vector3 direction;
    Pickup pickupTarget;


    // Start is called before the first frame update
    void Start()
    {
        Manager = GetComponent<BattleBotInterface>();
        direction = new Vector3(Random.value, 0, Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8)
        {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = Manager.Scan(dir);

            if (scan.type == HitType.World)
            {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
            }
            else if (scan.type == HitType.Enemy)
            {
                Manager.Shoot(dir);
                direction = scan.distance > 2f ? dir : -dir;

                break;
            }
            else if (scan.type == HitType.Item)
            {
                pickupTarget = scan.pickup;

                direction = (pickupTarget.transform.position - transform.position).normalized;

            }
        }
        Manager.Move(direction);
        
    }
}
