using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor Item", menuName = "Items/New Armor Item")]
public class ArmorItem : Item {
    public float Amount;

    public override void OnUse() {
        controller.Armor = Mathf.Min(controller.Armor + Amount, GameManager.instance.maxArmor);
    }
}