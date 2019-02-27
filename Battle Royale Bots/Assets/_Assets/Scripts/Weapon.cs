using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : ScriptableObject {

    public GameObject Owner;

    public abstract AmmoType AmmoType { get; }
    public abstract float Damage { get; }
    public abstract float Range { get; }
    public abstract int BaseAmmo { get; }
    public abstract float FireDelay { get; }
    public abstract float Precision { get; }

    public int Ammo { get; set; }

    private float lastFireTime;

    private void Awake() {
        Ammo = BaseAmmo;

        // Ensure that the gun can fire when the game starts.
        lastFireTime = Time.time - FireDelay;
    }

    public static T Instantiate<T>(GameObject owner) where T : Weapon {
        var weapon = CreateInstance<T>();
        weapon.Owner = owner;

        return weapon;
    }

    // Overridable for when we want to make other weapon types (explosive stuff for example).
    public virtual void OnHit(AIController enemy) {
        enemy.TakeDamage(Damage);
    }

    // Shooting behaviour could also be overridden if needed.
    public virtual void Shoot(Vector3 direction) {
        direction.Normalize();

        if ((Time.time - lastFireTime) >= FireDelay && Ammo > 0) {
            lastFireTime = Time.time;

            // Randomize direction based on precision.
            direction = (Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0,(Random.value * 2 - 1) * Precision,0)) * Vector3.forward;

            var debugLineColor = Color.cyan;
            var debugLineEnd = Owner.transform.position + direction * Range;

            if (Physics.Raycast(Owner.transform.position + direction, direction, out RaycastHit hit, Range)) {
                debugLineEnd = hit.point;

                var enemy = hit.collider.gameObject?.GetComponent<AIController>();

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
