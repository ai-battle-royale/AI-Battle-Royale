using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupWeapon : Pickup {
    public Weapon weapon;

    void Start () {
        weapon = Instantiate(weapon);
    }

    public override void OnInteract(BattleBotInterface bot) {
        var oldWeapon = bot.weapon;

        var instance = Instantiate(weapon.prefab, bot.weaponHolder);
        instance.transform.localPosition = Vector3.zero;

        bot.weapon = weapon;
        bot.weapon.controller = bot;
        bot.weapon.owner = bot.gameObject;
        bot.weapon.prefabInstance = instance;

        Destroy(oldWeapon.prefabInstance);
    }
}
