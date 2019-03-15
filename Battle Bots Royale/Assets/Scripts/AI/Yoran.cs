using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yoran : MonoBehaviour {

    bool canSeeEnemy;
    Vector3 direction;
    BattleBotInterface Controller;
    float angleOffset;
    bool isPickingUpItem = false;
    Pickup pickupTarget;

    void Start()  {
        Controller = GetComponent<BattleBotInterface>();

        direction = new Vector3(Random.value, 0, Random.value);
    }

    void Update() {
        angleOffset += Mathf.PI / 32;

        if (angleOffset > Mathf.PI / 8) {
            angleOffset = 0;
        }

        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8) {
            var dir = new Vector3(Mathf.Cos(i + angleOffset), 0, Mathf.Sin(i + angleOffset));
            var scan = Controller.Scan(dir);

            if (scan.type == HitType.World) {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
            } else if (scan.type == HitType.Enemy) {
                Controller.Shoot(dir);
                direction = scan.distance > 2f ? dir : -dir;

                break;
            }
            else if (scan.type == HitType.Item) {
                pickupTarget = scan.pickup;

                direction = (pickupTarget.transform.position - transform.position).normalized;

                isPickingUpItem = true;
            }
        }

        Controller.Move(direction);

        if (pickupTarget != null) {
            if (isPickingUpItem && Vector3.Distance(pickupTarget.transform.position, transform.position) < GameManager.instance.pickupRange) {
                Controller.Pickup(pickupTarget);
            }
        }

        var armorItem = Controller.FindItem<ArmorItem>();
        var healthItem = Controller.FindItem<HealingItem>();

        if (Controller.armor < 50 && armorItem != null)
        {
            Controller.UseItem(armorItem);
        }
        else if (Controller.health < 50 && healthItem != null)
        {
            Controller.UseItem(healthItem);
        }
    }
}
