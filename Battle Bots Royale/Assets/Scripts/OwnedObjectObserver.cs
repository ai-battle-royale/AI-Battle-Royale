using System;
using System.Collections.Generic;
using UnityEngine;

public class OwnedObjectObserver : MonoBehaviour {
    public static OwnedObjectObserver Instance;

    public List<OwnedObject> Objects;

    void Awake () {
        var observers = FindObjectsOfType<OwnedObjectObserver>();

        if (observers.Length > 1) {
            Debug.LogError("Too many OwnedObjectObserver components present in scene.");
        } else {
            Instance = this;
        }
    }

    void Update () {
        for (var i = 0; i < Objects.Count; i++) {
            var obj = Objects[i];

            // Unity does not fully get rid of ScriptableObjects when doing Destroy()
            // This means we have to remove them if manually
            if (obj == default(ScriptableObject)) {
                Objects.Remove(obj);
                continue;
            }

            obj.OnUpdate();
        }
    }
}
