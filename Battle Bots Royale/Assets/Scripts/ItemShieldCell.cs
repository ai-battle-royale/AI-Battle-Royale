using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemShieldCell : Item {
    public float Amount => 25f;
    public override float ConsumptionTime => 1f;

    public override void OnUse() {
        Controller.Armor = Mathf.Min(Controller.Armor + Amount, GameManager.Instance.MaxArmor);
    }
}