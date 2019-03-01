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
                Debug.Log($"Used item {this}");

                OnUse();

                Destroy(this);
            }
        }
    }

    public void Cancel () {
        IsBeingUsed = false;
    }

    public void Use () {
        IsBeingUsed = true;

        useTime = Time.time;
    }

    public abstract void OnUse();
}
