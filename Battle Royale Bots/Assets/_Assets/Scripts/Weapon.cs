using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    public float Damage;
    public float Range;
    public int Ammo;
    public float FireDelay;

    private bool canShoot = true;

    // Overridable for when we want to make other weapon types (explosive stuff for example)
    public virtual void OnHit (AIController enemy) {
        enemy.TakeDamage(Damage);
    }

    public void Shoot(Vector3 direction) {
        direction.Normalize();

        if (canShoot) {
            StartCoroutine(ShootCoroutine());
            var debugLineColor = Color.cyan;
            var debugLineEnd = transform.position + direction * Range;

            if (Physics.Raycast(transform.position + direction, direction, out RaycastHit hit, Range)) {
                debugLineEnd = hit.point;

                var enemy = hit.collider.gameObject?.GetComponent<AIController>();

                if (enemy != null) {
                    OnHit(enemy);

                    debugLineColor = Color.yellow;
                }
            }

            Debug.DrawLine(transform.position, debugLineEnd, debugLineColor, 5f);
        }
    }

    private IEnumerator ShootCoroutine() {
        canShoot = false;

        yield return new WaitForSeconds(FireDelay);

        canShoot = true;
    }
}
