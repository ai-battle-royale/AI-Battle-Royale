/* WIP Battle Bot Brain with different behaviour modes by Daniel
 * Last Udpate: 15.03.19
 * ***/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Daniel : MonoBehaviour
{
    enum Focus
    {
        Loot,
        Hunt,
        Flee
    }

    enum BrainState
    {
        Looting,
        Chasing,
        Evading,
        Victory
    }

    struct AdvancedScanInfo
    {
        //public ScanInfo ScanInfo    { get; set; }
        public Vector3 Direction    { get; set; }
        public Pickup Pickup        { get; set; }
        public float Distance       { get; set; }
        public HitType Type         { get; set; }

        public AdvancedScanInfo(ScanInfo scan, Vector3 direction) 
            : this(direction, scan.pickup, scan.distance, scan.type) { }

        public AdvancedScanInfo(Vector3 direction, Pickup pickup, float distance, HitType type) : this()
        {
            Direction = direction;
            Pickup = pickup;
            Distance = distance;
            Type = type;
        }
    }

    private BattleBotInterface bot;

    [Header("-----------------")]
    [Header("Behaviour Tweaking")]
    [Header("_________________")]
    [SerializeField] private Focus desiredState = Focus.Loot;

    [Header("Scanning")]
    [SerializeField] private int rayCount = 16;
    [SerializeField, Tooltip("Rotation step for each scan (higher is number means samller steps)")]
    private int rayStep = 64;

    [Header("Items and Usage")]
    [Range(0, 1f)]
    [SerializeField] private float healthThreshold = .5f;
    [Range(0, 1f)]
    [SerializeField] private float armorThreshold = .75f;

    [Header("Priorities")]
    [SerializeField] private int desirePriorityBonus = 1;
    [SerializeField] private int urgePriorityBonus = 5;
    [SerializeField] private int threshHoldBonus = 6;

    [Header("-----------------")]
    [Header("Brain Information")]
    [Header("_________________")]
    [ReadOnly, SerializeField] BrainState brainState;

    [Header("Actions")]
    [ReadOnly, SerializeField] Pickup pickupTarget;
    [ReadOnly, SerializeField] bool pickupItem;
    [ReadOnly, SerializeField] Vector3 currentDirection;

    [Header("Priorities")] // TODO assign structs to priorities to add them to a listing/change priorities to dict
    [ReadOnly, SerializeField] int lootPriority;
    [ReadOnly, SerializeField] int fightPriority;
    [ReadOnly, SerializeField] int healPriority;
    [ReadOnly, SerializeField] int armorPriority;
    [ReadOnly, SerializeField] int fleePriority;
    [ReadOnly, SerializeField] int movePriority;

    [Header("Sensors")]
    [ReadOnly, SerializeField] bool canSeeEnemy;
    [ReadOnly, SerializeField] bool canSeeItem;
    [ReadOnly, SerializeField] bool canSeeWeapon;

    [Header("Memory")]
    [ReadOnly, SerializeField] int kills = 0;
    [ReadOnly, SerializeField] int totalDamageTaken = 0;
    [ReadOnly, SerializeField] int totalDamageDealt = 0;

    [ReadOnly, SerializeField] List<Vector3> lastKnowEnemyPosition;     // Needs health/dmg tracking & player assosiation
    [ReadOnly, SerializeField] List<Vector3> lastKnowItemPositons;      // needs item assosiation
    [ReadOnly, SerializeField] List<Vector3> lastKnowWeaponPositions;   // needs weapon type assosiation

    [Header("Urges")]
    [ReadOnly, SerializeField] bool lookingForHealth;
    [ReadOnly, SerializeField] bool lookingForArmor;
    [ReadOnly, SerializeField] bool lookingForWeapon;
    [ReadOnly, SerializeField] bool shouldFight;
    [ReadOnly, SerializeField] bool shouldFlee;
    [ReadOnly, SerializeField] bool shouldMove;

    float currentAngleOffset;
    
    void Start()
    {
        bot = GetComponent<BattleBotInterface>();

        ResetBrain();
        ApplyDesires();
    }

    void ResetBrain()
    {
        pickupItem = false;
        currentDirection = Vector3.zero;

        lootPriority    = 0;
        fightPriority   = 0;
        healPriority    = 0;
        armorPriority  = 0;
        fleePriority    = 0;
        movePriority    = 0;

        canSeeEnemy     = false;
        canSeeItem      = false;
        canSeeWeapon    = false;

        kills               = 0;
        totalDamageTaken    = 0;
        totalDamageDealt    = 0;

        lastKnowEnemyPosition   = new List<Vector3>();
        lastKnowItemPositons    = new List<Vector3>();
        lastKnowWeaponPositions = new List<Vector3>();

        lookingForHealth    = false;
        lookingForArmor    = false;
        lookingForWeapon    = false;
        shouldFight         = false;
        shouldFlee          = false;
        shouldMove          = false;
    }

    void ApplyDesires()
    {
        // Set the base priorities
        switch (desiredState)
        {
            // Focus on looting and using items while evading
            // Fight with enough items
            // Flee when in danger
            case Focus.Loot:
                lootPriority    = 0;
                fightPriority   = 0;
                healPriority    = 0;
                armorPriority  = 0;
                fleePriority    = 0;
                movePriority    = 0;
                break;

            // Focus on gearing up to minimum and looking 
            //  for enemies to fight
            // Stop fighting and heal up when reaching thresholds
            // Look for minimum items when none are available 
            //  to reach threshold
            case Focus.Hunt:
                lootPriority    = 0;
                fightPriority   = 0;
                healPriority    = 0;
                armorPriority  = 0;
                fleePriority    = 0;
                movePriority    = 0;
                break;

            // Run away like a little **** until geared up, 
            //  enough time passed and fewer enemier remain
            case Focus.Flee:
                lootPriority    = 0;
                fightPriority   = 0;
                healPriority    = 0;
                armorPriority  = 0;
                fleePriority    = 0;
                movePriority    = 0;
                break;
        }
    }

    void UpdateBrainState()
    {
        // Check for priorities to update brain state


    }

    void UpdateBotBehaviour()
    {
        // Set behaviour base on brain state
    }

    void Update()
    {
        UpdateBrainState();
        UpdateBotBehaviour();

        var scans = ScanSurroundings();

        // Loop over all scan hits and prioritize
        foreach (var scanHit in scans)
        {
            var direction = Vector3.zero;
            if (scanHit.Type == HitType.World)
            {
                direction = Vector3.Slerp(direction, -scanHit.Direction, 1 - (scanHit.Distance / GameManager.instance.maxLookDistance));
            }
            else if (scanHit.Type == HitType.Enemy)
            {
                bot.Shoot(scanHit.Direction);
                direction = scanHit.Distance > 2f ? scanHit.Direction : -scanHit.Direction;
                break;
            }
            else if (scanHit.Type == HitType.Item)
            {
                pickupTarget = scanHit.Pickup;
                direction = (pickupTarget.transform.position - transform.position).normalized;
                pickupItem = true;
            }


            // FIXME apply priorities
            currentDirection = direction;
            shouldMove = true;
            break;
        }

        Loot();

        if (bot.health < Mathf.Ceil(100 * healthThreshold))
        {
            lookingForHealth = Heal();
        }
        else if (bot.armor < Mathf.Ceil(100 * armorThreshold))
        {
            lookingForArmor = Armor();
        }

        Move();
    }

    List<AdvancedScanInfo> ScanSurroundings()
    {
        // TODO apply "looking for" filters
        var scans = new List<AdvancedScanInfo>();
        currentAngleOffset += 2 * Mathf.PI / rayStep;
        var angleStep = 2 * Mathf.PI / rayCount;

        if (currentAngleOffset > angleStep)
        {
            currentAngleOffset = angleStep;
        }

        for (var i = 0f; i < Mathf.PI * 2; i += angleStep)
        {
            var direction = new Vector3(Mathf.Cos(i + currentAngleOffset), 0, Mathf.Sin(i + currentAngleOffset));
            var scan = bot.Scan(direction);

            scans.Add(new AdvancedScanInfo(scan, direction));
        }

        return scans;
    }

    bool LookFor(HitType type)
    {

        return false;
    }

    bool LookFor<T>() where T : Item
    {

        return false;
    }

    void Chase()
    {

    }

    void Fight()
    {

    }

    void Evade()
    {

    }

    void Move()
    {
        if (shouldMove) bot.Move(currentDirection);
    }

    bool Heal()
    {
        HealingItem healing = bot.FindItem<HealingItem>();
        if (healing != null)
        {
            // TODO Check for heal strength
            bot.UseItem(healing);
            return true;
        }

        return false;
    }

    bool Armor()
    {
        ArmorItem armor = bot.FindItem<ArmorItem>();
        if (armor != null)
        {
            // TODO Check for armor strength
            bot.UseItem(armor);
            return true;
        }
        return false;
    }

    void Loot()
    {
        if (pickupTarget != null)
        {
            if (pickupItem && Vector3.Distance(pickupTarget.transform.position, transform.position) 
                < GameManager.instance.pickupRange)
            {
                bot.Pickup(pickupTarget);
            }
        }
    }
}
