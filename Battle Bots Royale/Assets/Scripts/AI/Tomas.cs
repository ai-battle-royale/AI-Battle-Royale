using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tomas : MonoBehaviour
{
    BattleBotInterface Manager;
    private Vector3 direction;
    Pickup pickupTarget;
    int countHealthItems = 0 ;
    int countArmorItems = 0 ;
    bool safe;


    // Start is called before the first frame update
    void Start()
    {
        Manager = GetComponent<BattleBotInterface>();
        direction = new Vector3(Random.value, 0, Random.value);
        
    }
   
    // Update is called once per frame
    void Update()
    {
        var healingItem = Manager.FindItem<HealingItem>();
        var armorItem = Manager.FindItem<ArmorItem>();
        safe = true;

        //casting raycasts around the bot 
        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8)
        {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = Manager.Scan(dir);

            var enemy = scan.type == HitType.Enemy;
            if (!Manager.IsInRing)
            {
                Manager.Move(Manager.NextRingCenter);
            }
            //avoid collisions by moving to another direction
            if (scan.type == HitType.World)
            {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
            }
            //if the enemy is near the bot shoots
            else if (enemy)
            {
                safe = false;
                Manager.Shoot(dir);

                /// Always try to stay in weapon range 
                direction = scan.distance > Manager.weapon.range / 2 ? dir : -dir;
                
            }
            //item pickup logics
            else if (scan.type == HitType.Item)
            {
                pickupTarget = scan.pickup;

                //consumable item logic
                if (healingItem)
                {
                    countHealthItems++;
                   
                }
                if(countHealthItems == 3)
                {
                   Manager.Move(direction);
                }
                if (armorItem)
                {
                    countArmorItems++;
                    
                }
                if (countArmorItems == 3)
                {
                    Manager.Move(direction);

                }

                //weapon pickup logic
                if (pickupTarget is PickupWeapon pickupWeapon)
                {
                    if (pickupWeapon.weapon.damage / pickupWeapon.weapon.fireDelay > Manager.weapon.damage / Manager.weapon.fireDelay)
                    {
                        Manager.Pickup(pickupTarget);
                        direction = (pickupTarget.transform.position - transform.position).normalized;
                    }
                }
                else
                {
                    Manager.Pickup(pickupTarget);
                    direction = (pickupTarget.transform.position - transform.position).normalized;
                }

            }
            
        }

        WhatToUse(armorItem, healingItem);
        Manager.Move(direction);

    }
    void WhatToUse(ArmorItem armorItem, HealingItem healingItem)
    {
        if (Manager.armor <= 0 && safe == true || Manager.armor <= 70 && safe == true )
        {
            Manager.UseItem(armorItem);
        }
        //checking health and using item
        if (Manager.health <= 60 && safe == true)
        {
            Manager.UseItem(healingItem);
        }
    }
}
