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

        weapon = oldWeapon;
        weapon.controller = null;
        weapon.owner = null;
    }
}
