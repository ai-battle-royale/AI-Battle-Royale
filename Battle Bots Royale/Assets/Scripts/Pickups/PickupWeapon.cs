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

        bot.weapon = weapon;
        bot.weapon.controller = bot;
        bot.weapon.owner = bot.gameObject;
        var instance = Instantiate(weapon.prefab, bot.weaponHolder);
        instance.transform.localPosition = Vector3.zero;
    }
}
