using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OwnedObject : ScriptableObject
{
    public GameObject Owner;
    public BattleBotInterface Controller;

    /// <summary>
    /// Instantiates an OwnedObject of type T with the given owner.
    /// </summary>
    public static T Instantiate<T>(GameObject owner) where T : OwnedObject {
        var obj = CreateInstance<T>();

        obj.Owner = owner;
        obj.Controller = owner.GetComponent<BattleBotInterface>();

        OwnedObjectObserver.Instance.Objects.Add(obj);

        return obj;
    }

    public void StartCoroutine (IEnumerator method) {
        OwnedObjectObserver.Instance.StartCoroutine(method);
    }

    // Gets called every frame.
    public virtual void OnUpdate() {}
}
