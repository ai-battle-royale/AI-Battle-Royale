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
            if ((Time.time - useTime) >= ConsumptionTime) {
                OnUse();

                Destroy(this);

                controller.items.Remove(this);

                controller.IsUsingItem = false;
            }
        }
    }

    public void Cancel () {
        IsBeingUsed = false;
        controller.IsUsingItem = false;
    }

    public void Use () {
        IsBeingUsed = true;
        controller.IsUsingItem = true;

        useTime = Time.time;
    }

    public abstract void OnUse();
}
