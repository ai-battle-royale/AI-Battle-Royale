using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour {

    public void Interact (BattleBotInterface bot) {
        OnInteract(bot);
        
        Destroy(gameObject);
    }

    public abstract void OnInteract(BattleBotInterface bot);
}