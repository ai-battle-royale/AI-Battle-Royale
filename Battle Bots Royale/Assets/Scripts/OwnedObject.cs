using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OwnedObject : ScriptableObject
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public BattleBotInterface controller;

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

    public void StartCoroutine (IEnumerator method) {
        OwnedObjectObserver.instance.StartCoroutine(method);
    }

    // Gets called every frame.
    public virtual void OnUpdate() {}
}
