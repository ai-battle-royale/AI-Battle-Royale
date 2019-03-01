﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public GameObject Owner;
    public AIController Controller;

    public abstract float Amount { get; }
    public abstract float ConsumptionTime { get; }

    public static T Instantiate<T>(GameObject owner) where T : Item
    {
        var item = CreateInstance<T>();

        item.Owner = owner;
        item.Controller = owner.GetComponent<AIController>();

        return item;
    }

    public abstract void Use();
}
