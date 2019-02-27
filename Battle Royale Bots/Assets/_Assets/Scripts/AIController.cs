using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Weapon Weapon;
    public List<Item> Items = new List<Item>();

    public float    Health     { get; private set; } = 100f;
    public float    Armor      { get; private set; } = 0f;
    public float    LookRange   => Mathf.Max(Weapon.Range, MaxLookDistance);
    public int      Ammo        => Weapon.Ammo;

    private CharacterController characterController;
    private bool canShoot = true;
    private RectTransform labelObject;
    private BotLabel botLabel;

    void Start() {
        characterController = GetComponent<CharacterController>();

        Weapon = Weapon.Instantiate<WeaponSMG>(gameObject);

        var canvas = GameObject.FindGameObjectWithTag("Canvas");

        labelObject = Instantiate(Resources.Load("Prefabs/BotLabel") as GameObject, canvas.transform, false).GetComponent<RectTransform>();

        botLabel = labelObject.GetComponent<BotLabel>();
        botLabel.SetText(gameObject.name);
    }

    void Update() {
        labelObject.position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0,50,0);

        botLabel.SetSliders(Health / 100, Armor / 100);
    }

    public void TakeDamage (float amount) {
        var damageToHealth = Mathf.Max(0, amount - Armor);

        Armor = Mathf.Max(0, Armor - amount);
        Health = Mathf.Max(0, Health - damageToHealth);

        if (Health == 0) {
            print($"Bot '{gameObject.name}' died!");

            Destroy(gameObject);
            Destroy(labelObject.gameObject);
        }
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

    public ScanInfo Scan(Vector3 direction) {
        return Scan(direction, DefaultLayerMask);
    }

    public ScanInfo Scan (Vector3 direction, LayerMask mask) {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, LookRange, mask)) {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            var hitType = HitType.World;

            var isEnemy = hit.collider.gameObject?.GetComponent<AIController>() != null;

            if (isEnemy) {
                hitType = HitType.Enemy;
            }

            return new ScanInfo(hit.distance, hitType);
        }
        else {
            Debug.DrawLine(transform.position, transform.position + direction * LookRange, Color.green);

            return new ScanInfo(LookRange, HitType.None);
        }
    }

    public void Move (Vector3 direction) {
        characterController.Move(Vector3.ClampMagnitude(direction, MoveSpeed) * MoveSpeed * Time.deltaTime);
    }

    public void Shoot(Vector3 direction) {
        Weapon.Shoot(direction);
    }
}