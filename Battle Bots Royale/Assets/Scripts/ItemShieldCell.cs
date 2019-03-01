using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemShieldCell : Item {
    public float Amount => 25f;
    public override float ConsumptionTime => 1f;

    public override void OnUse() {
        controller.Armor = Mathf.Min(controller.Armor + Amount, GameManager.instance.maxArmor);
    }
}