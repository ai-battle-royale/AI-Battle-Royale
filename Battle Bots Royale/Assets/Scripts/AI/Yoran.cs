using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Yoran : MonoBehaviour {

    Vector3 direction;
    BattleBotInterface Controller;
    float angleOffset;
    bool isPickingUpItem = false;
    Pickup pickupTarget;

    Dictionary<string, int> weaponWeights = new Dictionary<string, int> {
        {"rifle", 5},
        {"pistol", 4},
        {"fists", 0}
    };

    void Start()  {
        Controller = GetComponent<BattleBotInterface>();

        direction = new Vector3(Random.value, 0, Random.value);
    }

    void Update() {
        angleOffset += Mathf.PI / 32;

        if (angleOffset > Mathf.PI / 8) {
            angleOffset = 0;
        }

        var canSeeEnemy = false;
        var isDangerous = false;
        var lowHealth = Controller.health < 50;

        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8) {
            var dir = new Vector3(Mathf.Cos(i + angleOffset), 0, Mathf.Sin(i + angleOffset));
            var scan = Controller.Scan(dir);

            if (scan.type == HitType.World)
            {

                // Move away from walls
                if (scan.distance < 3)
                {
                    direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
                }
            }
            else if (scan.type == HitType.Enemy)
            {

                if (Controller.IsUsingItem)
                {
                    Controller.CancelUseItem();
                }

                Controller.Shoot(dir);

                /// Always try to stay weapon.range / 2 units away from the enemy.
                direction = scan.distance > Controller.weapon.range / 2 ? dir : -dir;

                // Run away from the enemy if we are low on health
                if (lowHealth)
                {
                    direction = -dir;
                }

                canSeeEnemy = true;
                isDangerous = scan.distance < 5 && canSeeEnemy;

                break;
            }
            else if (scan.type == HitType.Item)
            {
                pickupTarget = scan.pickup;

                var tryToPickup = false;

                // Is the pickup for an item or weapon?
                if (pickupTarget is PickupItem)
                {
                    var healthItemCount = 0;
                    var armorItemCount = 0;

                    foreach (var item in Controller.items)
                    {
                        if (item is HealingItem) healthItemCount++;
                        else if (item is ArmorItem) armorItemCount++;
                    }

                    // Only try to pickup items if we need them.
                    if (healthItemCount < 3 || armorItemCount < 3)
                    {
                        tryToPickup = true;
                    }

                }
                else if (pickupTarget is PickupWeapon pickupWeapon)
                {
                    if (weaponWeights.ContainsKey(pickupWeapon.weapon.name) && (weaponWeights[pickupWeapon.weapon.name] > weaponWeights[Controller.weapon.name]))
                    {
                        tryToPickup = true;
                    }
                }

                // Don't pick up an item when there's an enemy nearby
                tryToPickup = tryToPickup && !isDangerous;

                if (tryToPickup)
                {
                    direction = (pickupTarget.transform.position - transform.position).normalized;

                    isPickingUpItem = true;
                }
            }
            else if (!Controller.IsInRing)
            {
                direction = (Controller.RingCenter - transform.position).normalized;
            }
        }

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

        Controller.Move(direction);
    }
}
