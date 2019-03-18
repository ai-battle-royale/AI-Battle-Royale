using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OwnedObjectObserver))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Weapon defaultWeapon;
    public float maxLookDistance = 5f;
    public float moveSpeed = 1f;
    public float maxHealth = 100f;
    public float maxArmor = 100f;
    public float pickupRange = 1f;
    public float timeScale = 1f;

    void Awake() {
        var managers = FindObjectsOfType<GameManager>();

        if (managers.Length > 1) {
            Debug.LogError("Too many GameManager components present in scene.");
        }
        else {
            instance = this;
        }
    }

    void Update () {
        Time.timeScale = timeScale;
    }
}
