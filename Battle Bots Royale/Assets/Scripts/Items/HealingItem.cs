using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Healing Item", menuName = "Items/New Healing Item")]
public class HealingItem : Item {
    public float Amount;

    public override void OnUse()
    {
        controller.health = GetResultValue();
    }

    public override void OnStartUse()
    {
        controller.botLabel.healthActionSlider.value = GetResultValue() / GameManager.instance.maxHealth;
    }

    public override void OnStopUse()
    {
        controller.botLabel.healthActionSlider.value = 0;
    }

    float GetResultValue() => Mathf.Min(controller.health + Amount, GameManager.instance.maxHealth);
}
