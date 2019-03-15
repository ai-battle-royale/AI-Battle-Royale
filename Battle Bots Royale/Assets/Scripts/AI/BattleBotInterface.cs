using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ScanInfo {
    public Pickup pickup;
    public float distance;
    public HitType type;

    public ScanInfo (Pickup obj, float d, HitType t) {
        pickup = obj;
        distance = d;
        type = t;
    }
}

public enum HitType {
    None,
    Enemy,
    Item,
    World
}

[RequireComponent(typeof(RigidbodyController))]
public class BattleBotInterface : MonoBehaviour {
    public Weapon weapon;
    public Transform weaponHolder;
    public List<Item> items = new List<Item>();

    [ReadOnly] public float health;
    [ReadOnly] public float armor;
    public float            LookRange   => Mathf.Max(weapon.range, GameManager.instance.maxLookDistance);
    public int              Ammo        => weapon.Ammo;
    public bool             IsUsingItem;
    public BotLabel botLabel;

    private RigidbodyController rigidbodyController;
    private RectTransform labelObject;
    private Item lastUsedItem;

    void CreateLabel () {
        var canvas = GameObject.FindGameObjectWithTag("Canvas");

        // Instantiate a name label object and attach it to the canvas.
        labelObject = Instantiate(Resources.Load("Prefabs/BotLabel") as GameObject, canvas.transform, false).GetComponent<RectTransform>();

        botLabel = labelObject.GetComponent<BotLabel>();
        botLabel.SetText(gameObject.name);
    }

    void Start() {
        health = GameManager.instance.maxHealth;

        rigidbodyController = GetComponent<RigidbodyController>();

        weapon = (Weapon)OwnedObject.Instantiate(GameManager.instance.defaultWeapon, gameObject);

        CreateLabel();
    }

    void Update() {
        // Set the name label position on the canvas.
        if (labelObject != null)
        {
            labelObject.position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 50, 0);

            botLabel.SetSliders(health / 100, armor / 100);
        }
    }

    /// <summary>
    /// Damages the BattleBot's armor and health
    /// </summary>
    public void TakeDamage (float amount) {
        // How much damage will be subtracted from the health value.
        var damageToHealth = Mathf.Max(0, amount - armor);

        armor = Mathf.Max(0, armor - amount);
        health = Mathf.Max(0, health - damageToHealth);

        if (health == 0) {
            print($"Bot '{gameObject.name}' died!");

            Destroy(gameObject);
            Destroy(labelObject.gameObject);
        }
    }

    /// <summary>
    /// Checks if the BattleBot has an item of type T
    /// </summary>
    public T FindItem<T> () where T : Item {
        return (T)items.Find(x => x is T);
    }

    /// <summary>
    /// Checks if the BattleBot has an item of type T
    /// </summary>
    public void UseItem(Item item) {
        if (IsUsingItem) return;

        if (item == null) return;

        if (items.Contains(item)) {
            items.Remove(item);

            item.Use();
            lastUsedItem = item;
        }
    }

    public void CancelUseItem () {
        lastUsedItem?.Cancel();
    }

    /// <summary>
    /// Attempts to pickup the given pickup.
    /// </summary>
    public void Pickup(Pickup pickup) {
        var direction = (pickup.transform.position - transform.position).normalized;
        var debugLineColor = Color.magenta;
        var debugLineEnd = transform.position + direction * GameManager.instance.pickupRange;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, GameManager.instance.pickupRange)) {
            debugLineEnd = hit.point;

            if (hit.collider.gameObject == pickup.gameObject) {
                pickup.Interact(this);

                //print($"Interacted with pickup {pickup}");

                debugLineColor = Color.cyan;
            }
        }

        Debug.DrawLine(transform.position, debugLineEnd, debugLineColor, 5f);
    }

    /// <summary>
    /// Scans for objects in the given layermask and returns a ScanInfo result.
    /// </summary>
    /// <returns>A ScanInfo result.</returns>
    public ScanInfo Scan (Vector3 direction, LayerMask mask = default) {
        if (mask == default) mask = LayerMask.NameToLayer("Everything");

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, LookRange, mask)) {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            var hitType = HitType.World;

            // Check if the gameobject we hit has a BattleBotInterface interface, if it does, it means we hit an enemy.
            if (hit.collider.gameObject?.GetComponent<BattleBotInterface>() != null) {
                hitType = HitType.Enemy;
            }

            // Check if the gameobject we hit has a pickup component, if it does, it means we hit an item.
            var pickup = hit.collider.gameObject?.GetComponent<Pickup>();

            if (hit.collider.gameObject?.GetComponent<Pickup>() != null) {
                hitType = HitType.Item;
            }

            return new ScanInfo(pickup, hit.distance, hitType);
        }
        else {
            Debug.DrawLine(transform.position, transform.position + direction * LookRange, Color.green);

            return new ScanInfo(null, LookRange, HitType.None);
        }
    }

    /// <summary>
    /// Makes the BattleBot move in the given direction.
    /// </summary>
    public void Move (Vector3 direction) {
        if (IsUsingItem)
        {
            rigidbodyController.Stop();
            return;
        }

        rigidbodyController.Move(direction);
        if (direction.magnitude > 0) { 
            transform.rotation = Quaternion.LookRotation(Vector3.Scale(direction, new Vector3(1,0,1)), Vector3.up);
        }
    }

    /// <summary>
    /// Makes the BattleBot shoot in the given direction.
    /// </summary>
    public void Shoot(Vector3 direction) {
        if (IsUsingItem) return;

        weapon.Shoot(direction);
    }
}
