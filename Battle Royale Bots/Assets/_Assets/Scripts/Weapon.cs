using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : ScriptableObject {

    public abstract AmmoType    AmmoType { get; } 
    public abstract float       Damage { get; }
    public abstract float       Range { get; }
    public abstract int         BaseAmmo { get; }
    public abstract float       FireDelay { get; }
    public abstract float       Precision { get; }

    public int Ammo;
    public GameObject Owner;
    public BattleBotInterface Controller;

    private float lastFireTime;

    private void Awake() {
        Ammo = BaseAmmo;

        // Ensure that the gun can fire when the game starts.
        lastFireTime = Time.time - FireDelay;
    }

    /// <summary>
    /// Instantiates a weapon of type T with the given owner.
    /// </summary>
    public static T Instantiate<T>(GameObject owner) where T : Weapon {
        var weapon = CreateInstance<T>();

        weapon.Owner = owner;
        weapon.Controller = owner.GetComponent<BattleBotInterface>();

        return weapon;
    }

    /// <summary>
    /// Gets called when the weapon hits an enemy in the default Shoot() method.
    /// </summary>
    public virtual void OnHit(BattleBotInterface enemy) {
        enemy.TakeDamage(Damage);
    }

    /// <summary>
    /// Shooting behaviour
    /// </summary>
    public virtual void Shoot(Vector3 direction) {
        direction.Normalize();

        // Only allow shooting if we have ammo and if enough time has passed.
        if ((Time.time - lastFireTime) >= FireDelay && Ammo > 0) {
            lastFireTime = Time.time;

            // Randomize direction based on precision.
            direction = 
                (Quaternion.LookRotation(direction, Vector3.up) * 
                Quaternion.Euler(0,(Random.value * 2 - 1) * Precision,0))   // Add a randomized angle offset based on the weapon's precision
                * Vector3.forward;                                          // Convert to vector

            var debugLineColor = Color.cyan;
            var debugLineEnd = Owner.transform.position + direction * Range;

            if (Physics.Raycast(Owner.transform.position + direction, direction, out RaycastHit hit, Range)) {
                debugLineEnd = hit.point;

                var enemy = hit.collider.gameObject?.GetComponent<BattleBotInterface>();

                if (enemy != null) {
                    OnHit(enemy);

                    debugLineColor = Color.yellow;
                }
            }

            Debug.DrawLine(Owner.transform.position, debugLineEnd, debugLineColor, 5f);

            Ammo--;
        }
    }
}
