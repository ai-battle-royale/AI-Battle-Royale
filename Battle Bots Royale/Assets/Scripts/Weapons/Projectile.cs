using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public LayerMask mask;
    public float startVelocity = 20f;
    public float range = -1f;
    public int timeSteps = 6;
    public float damage = 0f;

    private float coveredDistance;
    private bool hasFired;

    private Vector3 position;
    private Vector3 velocity;

    private BattleBotInterface owner;

    public void Fire (Vector3 direction, BattleBotInterface instigator) {
        position = transform.position;
        owner = instigator;
        velocity = direction * startVelocity;

        StartCoroutine(DestroyAfterLifeTime(range / startVelocity));

        hasFired = true;
    }

    void FixedUpdate() {
        if (!hasFired) return;

        var position = transform.position;

        for (var i = 0; i < timeSteps; i++) {
            position = Simulate(position);
        }

        transform.position = position;
    }

    Vector3 Simulate (Vector3 pos) {
        var rayCast = Physics.Raycast(pos, velocity.normalized, out RaycastHit hit, velocity.magnitude / timeSteps, mask);

        if (rayCast) {
            var enemy = hit.collider.gameObject?.GetComponent<BattleBotInterface>();

            if (enemy != null) {
                enemy.TakeDamage(damage, owner);
            }

            Destroy(gameObject);
        } else {
            pos += velocity * Time.fixedDeltaTime / timeSteps;
        }

        return pos;
    }

    IEnumerator DestroyAfterLifeTime (float lifeTime) {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }
}
