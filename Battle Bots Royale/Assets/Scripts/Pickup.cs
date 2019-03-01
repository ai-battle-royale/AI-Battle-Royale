using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour {
    public void Interact (BattleBotInterface bot) {
        var direction = (transform.position - bot.transform.position).normalized;
        var debugLineColor = Color.magenta;
        var debugLineEnd = transform.position + direction * GameManager.instance.pickupRange;

        if (Physics.Raycast(transform.position + direction, direction, out RaycastHit hit, GameManager.instance.pickupRange)) {
            debugLineEnd = hit.point;

            if (hit.collider.gameObject == bot.gameObject) {
                OnInteract(bot);

                print("Interacted with bot");

                debugLineColor = Color.cyan;
            }
        }
    }

    public abstract void OnInteract(BattleBotInterface bot);
}