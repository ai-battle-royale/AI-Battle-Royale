using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OwnedObjectObserver))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float MaxLookDistance = 5f;
    public float MoveSpeed = 1f;
    public float MaxHealth = 100f;
    public float MaxArmor = 100f;

    void Awake() {
        var managers = FindObjectsOfType<OwnedObjectObserver>();

        if (managers.Length > 1) {
            Debug.LogError("Too many GameManager components present in scene.");
        }
        else {
            Instance = this;
        }
    }
}
