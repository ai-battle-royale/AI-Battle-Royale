using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupWeapon : Pickup {
    public Weapon weapon;

    public override void OnInteract(BattleBotInterface bot) {
        var oldWeapon = bot.weapon;

        bot.weapon = weapon;

        weapon = oldWeapon;
    }
}
