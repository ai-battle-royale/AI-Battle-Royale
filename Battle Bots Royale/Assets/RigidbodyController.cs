using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyController : MonoBehaviour
{
    public float smoothing = 0.5f;
    private Rigidbody rb;
    private Vector3 desiredMoveVector;
    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        velocity = Vector3.Lerp(velocity, desiredMoveVector, Time.deltaTime / smoothing);

        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        Debug.DrawLine(transform.position, transform.position + transform.forward, new Color(255,0,100), Time.deltaTime);
    }

    public void Move(Vector3 moveVector)
    {
        desiredMoveVector = moveVector * GameManager.instance.moveSpeed;
    }

    public void Stop()
    {
        desiredMoveVector = Vector3.zero;
    }
}
