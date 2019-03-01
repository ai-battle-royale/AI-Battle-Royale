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
public class BattleBotInterface : MonoBehaviour {

    // These constants should be moved over to a game manager
    public float MaxLookDistance = 5f;
    public float MoveSpeed = 1f;
    public LayerMask DefaultLayerMask;

    public Weapon Weapon;
    public List<Item> Items = new List<Item>();

    public float    Health     { get; set; } = 100f;
    public float    Armor      { get; set; } = 0f;
    public float    LookRange   => Mathf.Max(Weapon.Range, MaxLookDistance);
    public int      Ammo        => Weapon.Ammo;

    private CharacterController characterController;
    private bool canShoot = true;
    private RectTransform labelObject;
    private BotLabel botLabel;

    void CreateLabel () {
        var canvas = GameObject.FindGameObjectWithTag("Canvas");

        // Instantiate a name label object and attach it to the canvas.
        labelObject = Instantiate(Resources.Load("Prefabs/BotLabel") as GameObject, canvas.transform, false).GetComponent<RectTransform>();

        botLabel = labelObject.GetComponent<BotLabel>();
        botLabel.SetText(gameObject.name);
    }

    void Start() {
        characterController = GetComponent<CharacterController>();

        Weapon = Weapon.Instantiate<WeaponSMG>(gameObject);

        CreateLabel();
    }

    void Update() {
        // Set the name label position on the canvas.
        labelObject.position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0,50,0);

        botLabel.SetSliders(Health / 100, Armor / 100);
    }

    /// <summary>
    /// Damages the BattleBot's armor and health
    /// </summary>
    public void TakeDamage (float amount) {
        // How much damage will be subtracted from the health value.
        var damageToHealth = Mathf.Max(0, amount - Armor);

        Armor = Mathf.Max(0, Armor - amount);
        Health = Mathf.Max(0, Health - damageToHealth);

        if (Health == 0) {
            print($"Bot '{gameObject.name}' died!");

            Destroy(gameObject);
            Destroy(labelObject.gameObject);
        }
    }

    /// <summary>
    /// Checks if the BattleBot has an item of type T
    /// </summary>
    public T HasItem<T> () where T : Item {
        return (T)Items.Find(x => x is T);
    }

    /// <summary>
    /// Checks if the BattleBot has an item of type T
    /// </summary>
    public void UseItem(Item item) {
        item.Use();
    }

    /// <summary>
    /// Scans for objects in the given layermask and returns a ScanInfo result.
    /// </summary>
    /// <returns>A ScanInfo result.</returns>
    public ScanInfo Scan(Vector3 direction) {
        return Scan(direction, DefaultLayerMask);
    }

    /// <summary>
    /// Scans for objects in the given layermask and returns a ScanInfo result.
    /// </summary>
    /// <returns>A ScanInfo result.</returns>
    public ScanInfo Scan (Vector3 direction, LayerMask mask) {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, LookRange, mask)) {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            var hitType = HitType.World;

            // Check if the gameobject we hit has a BattleBotInterface interface, if it does, it means we hit an enemy.
            if (hit.collider.gameObject?.GetComponent<BattleBotInterface>() != null) {
                hitType = HitType.Enemy;
            }

            return new ScanInfo(hit.distance, hitType);
        }
        else {
            Debug.DrawLine(transform.position, transform.position + direction * LookRange, Color.green);

            return new ScanInfo(LookRange, HitType.None);
        }
    }

    /// <summary>
    /// Makes the BattleBot move in the given direction.
    /// </summary>
    public void Move (Vector3 direction) {
        characterController.Move(Vector3.ClampMagnitude(direction, MoveSpeed) * MoveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Makes the BattleBot shoot in the given direction.
    /// </summary>
    public void Shoot(Vector3 direction) {
        Weapon.Shoot(direction);
    }
}