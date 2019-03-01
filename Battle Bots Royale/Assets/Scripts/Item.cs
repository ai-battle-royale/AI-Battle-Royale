using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public GameObject Owner;
    public BattleBotInterface Controller;

    public abstract float ConsumptionTime { get; }

    public bool IsBeingUsed { get; private set; }

    private float useTime;

    public static T Instantiate<T>(GameObject owner) where T : Item
    {
        var item = CreateInstance<T>();

        item.Owner = owner;
        item.Controller = owner.GetComponent<BattleBotInterface>();

        return item;
    }

    private void Awake() {
        // Ensure that the item can be used as soon as it's spawned.
        useTime = Time.time - ConsumptionTime;
    }

    private void Update () {
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
