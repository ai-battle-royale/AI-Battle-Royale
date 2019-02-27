using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScanInfo {
    public float Distance;
    public HitType Type;

    public ScanInfo (float d, HitType t) {
        Distance = d;
        Type = t;
    }
}

public enum HitType {
    None,
    Enemy,
    Item,
    World
}

[RequireComponent(typeof(CharacterController))]
public class AIController : MonoBehaviour {

    // These constants should be moved over to a game manager
    public float MaxLookDistance = 5f;
    public float MoveSpeed = 1f;
    public LayerMask DefaultLayerMask;

    public List<Item> Items = new List<Item>();

    public float    Health     { get; private set; } = 100f;
    public float    Armor      { get; private set; } = 0f;
    public bool     IsMoving   { get; private set; } = false;

    private CharacterController characterController;
    private float moveAngle;

    void Start() {
        characterController = GetComponent<CharacterController>();
    }

    void Update() {
        characterController.Move(GetDirectionFromAngle(moveAngle) * MoveSpeed * Time.deltaTime);
    }

    public bool HasItem<T> () where T : Item {
        return Items.Exists(x => x is T);
    }

    public void UseItem<T>() where T : Item {
        var item = Items.Find(x => x is T);

        item.Use();
    }

    // There's probably already a built-in function for this
    Vector3 GetDirectionFromAngle (float angle) {
        angle *= Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
    }
    
    public ScanInfo Scan (float angle, LayerMask mask = default) {

        if (mask == default) mask = DefaultLayerMask;

        if (Physics.Raycast(transform.position, GetDirectionFromAngle(angle), out RaycastHit hit, MaxLookDistance, mask)) {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            return new ScanInfo(hit.distance, HitType.World);
        }
        else {
            Debug.DrawLine(transform.position, transform.position + GetDirectionFromAngle(angle) * MaxLookDistance, Color.green);

            return new ScanInfo(MaxLookDistance, HitType.None);
        }
    }

    public void Move (float angle) {
        IsMoving = true;
        moveAngle = angle;
    }

    public void Stop() {
        IsMoving = false;
    }
}
