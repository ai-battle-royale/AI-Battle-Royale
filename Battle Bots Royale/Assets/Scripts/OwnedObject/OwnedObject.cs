using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OwnedObject : ScriptableObject
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public BattleBotInterface controller;

    void Awake () {
        //Debug.Log($"{this} awoke");


        if (!OwnedObjectObserver.instance) return;

        if (!OwnedObjectObserver.instance.objects.Contains(this))
            OwnedObjectObserver.instance.objects.Add(this);
    }

    /// <summary>
    /// Instantiates an OwnedObject of type T with the given owner.
    /// </summary>
    public static T Instantiate<T>(GameObject owner) where T : OwnedObject {
        var obj = CreateInstance<T>();

        obj.owner = owner;
        obj.controller = owner.GetComponent<BattleBotInterface>();

        OwnedObjectObserver.instance.objects.Add(obj);

        return obj;
    }

    /// <summary>
    /// Copies an OwnedObject of the given type with the given owner.
    /// </summary>
    public static OwnedObject Instantiate(OwnedObject reference, GameObject owner) {
        var obj = Instantiate(reference);

        obj.owner = owner;
        obj.controller = owner.GetComponent<BattleBotInterface>();

        OwnedObjectObserver.instance.objects.Add(obj);

        return obj;
    }

    public void StartCoroutine (IEnumerator method) {
        OwnedObjectObserver.instance.StartCoroutine(method);
    }

    // Gets called every frame.
    public virtual void OnUpdate() {}
}
