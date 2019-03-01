using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Pickup {
    public Item item;

    void Start () {
        item = Instantiate(item);
    }
    
    public override void OnInteract(BattleBotInterface bot) {
        var addedItem = item;
        addedItem.controller = bot;
        addedItem.owner = bot.gameObject;

        bot.items.Add(addedItem);
    }
}
