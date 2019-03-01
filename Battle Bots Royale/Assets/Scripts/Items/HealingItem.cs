using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Healing Item", menuName = "Items/New Healing Item")]
public class HealingItem : Item {
    public float Amount;

    public override void OnUse() {
        controller.health = Mathf.Min(controller.health + Amount, GameManager.instance.maxHealth);
    }
}
