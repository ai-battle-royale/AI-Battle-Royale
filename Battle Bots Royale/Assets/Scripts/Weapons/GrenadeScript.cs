using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeScript : MonoBehaviour
{
    public float delay = 3f;
    public float radius = 5f;
    public float explosionDuration = 2f;
    public float explosionForce = 500f;
    float countdown;
    bool hasExploded = false;

    // Start is called before the first frame update
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown < 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, 1 << 11);

        foreach (Collider nearbyObject in colliders)
        {
            print(LayerMask.NameToLayer("Bot"));
            Rigidbody rb = nearbyObject.gameObject.GetComponent<Rigidbody>();
            CapsuleCollider collider = nearbyObject.gameObject.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = nearbyObject.gameObject.AddComponent<CapsuleCollider>();
            }
            if (rb == null)
            {
                rb = nearbyObject.gameObject.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
            }
            nearbyObject.gameObject.GetComponent<CharacterController>().enabled = false;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            rb.AddExplosionForce(explosionForce, transform.position, radius);
        }
        StartCoroutine(ExplosionCleanUp(colliders));
    }
    IEnumerator ExplosionCleanUp(Collider[] colliders)
    {
        yield return new WaitForSeconds(explosionDuration);
        foreach (Collider col in colliders)
        {
            Destroy(col.gameObject.GetComponent<Rigidbody>());
            Destroy(col.gameObject.GetComponent<CapsuleCollider>());
            col.gameObject.GetComponent<CharacterController>().enabled = true;
        }
        Destroy(gameObject); 
    }
}
