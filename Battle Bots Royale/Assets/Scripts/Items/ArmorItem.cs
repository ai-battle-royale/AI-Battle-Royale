using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor Item", menuName = "Items/New Armor Item")]
public class ArmorItem : Item {
    public float Amount;

    public override void OnUse() {
        controller.armor = Mathf.Min(controller.armor + Amount, GameManager.instance.maxArmor);
    }
}