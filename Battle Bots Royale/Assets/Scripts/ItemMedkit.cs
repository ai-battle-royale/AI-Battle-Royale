using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMedkit : Item {
    public float Amount => 50f;
    public override float ConsumptionTime => 3f;

    public override void OnUse() {
        controller.Health = Mathf.Min(controller.Health + Amount, GameManager.instance.maxHealth);
    }
}
