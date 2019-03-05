using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : OwnedObject
{
    public float ConsumptionTime;

    public bool IsBeingUsed { get; private set; }

    private float useTime;

    public override void OnUpdate () {
        if (IsBeingUsed) {
            controller.botLabel.progressSlider.value = (Time.time - useTime) / ConsumptionTime;

            if ((Time.time - useTime) >= ConsumptionTime) {
                OnUse();

                Destroy(this);

                controller.items.Remove(this);

                controller.IsUsingItem = false;

                OnStopUse();
                controller.botLabel.progressSlider.value = 0;
            }
        }
    }

    public void Cancel () {
        IsBeingUsed = false;
        controller.IsUsingItem = false;

        OnStopUse();
        controller.botLabel.progressSlider.value = 0;
    }

    public void Use () {
        IsBeingUsed = true;
        controller.IsUsingItem = true;

        useTime = Time.time;

        OnStartUse();
    }

    public virtual void OnStopUse() { }
    public virtual void OnStartUse() { }
    public abstract void OnUse();
}
