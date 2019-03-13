using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor Item", menuName = "Items/New Armor Item")]
public class ArmorItem : Item {
    public float Amount;

    public override void OnUse() {
        controller.armor = GetResultValue();
    }

    public override void OnStartUse()
    {
        controller.botLabel.armorActionSlider.value = GetResultValue() / GameManager.instance.maxArmor;
    }

    public override void OnStopUse()
    {
        controller.botLabel.armorActionSlider.value = 0;
    }


    float GetResultValue () => Mathf.Min(controller.armor + Amount, GameManager.instance.maxArmor);
}