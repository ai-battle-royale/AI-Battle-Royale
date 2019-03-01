using System;
using System.Collections.Generic;
using UnityEngine;

public class OwnedObjectObserver : MonoBehaviour {
    public static OwnedObjectObserver instance;

    public List<OwnedObject> objects;

    void Awake () {
        var observers = FindObjectsOfType<OwnedObjectObserver>();

        if (observers.Length > 1) {
            Debug.LogError("Too many OwnedObjectObserver components present in scene.");
        } else {
            instance = this;
        }
    }

    void Update () {
        for (var i = 0; i < objects.Count; i++) {
            var obj = objects[i];

            // Unity does not fully get rid of ScriptableObjects when doing Destroy()
            // This means we have to remove them if manually
            if (obj == default(ScriptableObject)) {
                objects.Remove(obj);
                continue;
            }

            obj.OnUpdate();
        }
    }
}
