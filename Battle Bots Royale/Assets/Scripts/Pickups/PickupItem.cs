using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Pickup {
    public Item item;

    public override void OnInteract(BattleBotInterface bot) {
        bot.items.Add(item);
    }
}
