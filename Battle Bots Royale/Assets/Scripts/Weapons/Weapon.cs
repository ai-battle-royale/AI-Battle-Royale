﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon")]
public class Weapon : OwnedObject {

    public string weaponName;
    public GameObject pickupPrefab;
    public GameObject prefab;
    public AmmoType ammoType;
    public float damage;
    public float range;
    public int baseAmmo;
    public float fireDelay;
    public float precision;
    public GameObject projectilePrefab;
    [HideInInspector] public GameObject prefabInstance;

    public int Ammo { get; set;  }

    private float lastFireTime;

    private void Awake() {
        Ammo = baseAmmo;

        // Ensure that the gun can fire when the game starts.
        lastFireTime = Time.time - fireDelay;
    }

    /// <summary>
    /// Gets called when the weapon hits an enemy in the default Shoot() method.
    /// </summary>
    public virtual void OnHit(BattleBotInterface enemy) {
        enemy.TakeDamage(damage, controller);
    }

    /// <summary>
    /// Shooting behaviour
    /// </summary>
    public virtual void Shoot(Vector3 direction) {
        direction.Normalize();

        // Only allow shooting if we have ammo and if enough time has passed.
        if ((Time.time - lastFireTime) >= fireDelay && Ammo > 0) {
            lastFireTime = Time.time;

            // Randomize direction based on precision.
            direction = 
                (Quaternion.LookRotation(direction, Vector3.up) * 
                Quaternion.Euler(0,(Random.value * 2 - 1) * precision,0))   // Add a randomized angle offset based on the weapon's precision
                * Vector3.forward;                                          // Convert to vector

            var debugLineColor = Color.cyan;
            var debugLineEnd = owner.transform.position + direction * range;

            if (projectilePrefab == null) {
                if (Physics.Raycast(owner.transform.position + direction, direction, out RaycastHit hit, range)) {
                    debugLineEnd = hit.point;

                    var enemy = hit.collider.gameObject?.GetComponent<BattleBotInterface>();

                    if (enemy != null) {
                        OnHit(enemy);

                        debugLineColor = Color.yellow;
                    }
                }
            } else {
                var projectileObject = Instantiate(projectilePrefab, owner.transform.position + direction, Quaternion.LookRotation(direction));
                var projectile = projectileObject.GetComponent<Projectile>();

                projectile.range = range;
                projectile.damage = damage;

                projectile.Fire(direction, controller);
            }

            Debug.DrawLine(owner.transform.position, debugLineEnd, debugLineColor, 5f);

            Ammo--;
        }
    }
}
