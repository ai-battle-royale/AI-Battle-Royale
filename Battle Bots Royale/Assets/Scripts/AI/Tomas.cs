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

        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8)
        {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = Manager.Scan(dir);

            var enemy = scan.type == HitType.Enemy;

            if (scan.type == HitType.World)
            {
                direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
            }

            else if (enemy)
            {
                Manager.Shoot(dir);
                if (Manager.weapon == null && Manager.health < 70)
                {
                    direction = scan.distance > 2f ? dir : -dir;
                }
                else
                {
                    direction = Vector3.Slerp(direction, -dir, 1 - (scan.distance / GameManager.instance.maxLookDistance));
                }
                

                break;
            }

            else if (scan.type == HitType.Item)
            {
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

                pickupTarget = scan.pickup;

                direction = (pickupTarget.transform.position - transform.position).normalized;
                Manager.Pickup(pickupTarget.GetComponent<Pickup>());

            }
            
        }
        //checking armors and using the item
        if (Manager.armor <= 0 || Manager.armor <= 70 )
        {
            Manager.UseItem(armorItem);
        }
        else if (Manager.armor == 100)
        {
            Manager.Move(direction);
        }
        //checking health and using item
        if (Manager.health <= 60)
        {
            Manager.UseItem(healingItem);
        }
        else if (Manager.health == 100)
        {
            Manager.Move(direction);
        }

        Manager.Move(direction);

    }
}
