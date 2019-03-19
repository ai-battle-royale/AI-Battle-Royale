/* WIP Battle Bot Brain with different behaviour modes by Daniel
 * Last Udpate: 15.03.19
 * ***/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Daniel : MonoBehaviour
{
    enum Focus
    {
        Loot,
        Hunt,
        Survive
    }

    enum BrainState
    {
        Looting,
        Chasing,
        Evading,
        Victory
    }

    [Serializable]
    class AdvancedScanInfo
    {
        [ReadOnly, SerializeField] private Vector3 _direction;
        [ReadOnly, SerializeField] private Pickup _pickup;
        [ReadOnly, SerializeField] private float _distance;
        [ReadOnly, SerializeField] private HitType _type;

        //public ScanInfo ScanInfo    { get; set; }
        public Vector3 Direction    { get => _direction; set => _direction = value; }
        public Pickup Pickup        { get => _pickup; set => _pickup = value; }
        public float Distance       { get => _distance; set => _distance = value; }
        public HitType Type         { get => _type; set => _type = value; }

        public AdvancedScanInfo()
        {
        }

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

    enum PickupType
    {
        Weapon,
        HealingConsumable,
        ArmorConsumable
    }
    [Serializable]
    class PickupInfo
    {
        [ReadOnly, SerializeField] private Vector3 _position;
        [ReadOnly, SerializeField] private Pickup _pickup;
        [ReadOnly, SerializeField] private PickupType _type;

        public Vector3 Position { get => _position; set => _position = value; }
        public Pickup Pickup    { get => _pickup; set => _pickup = value; }
        public PickupType Type  { get => _type; set => _type = value; }

        public PickupInfo()
        {
        }

        public PickupInfo(Pickup pickup) : this()
        {
            Position = pickup.transform.position;
            Pickup = pickup;
        }
    }
    [Serializable]
    class EnemyInfo
    {
        [ReadOnly, SerializeField] private Vector3 _position;
        [ReadOnly, SerializeField] private Vector3 _predictedDirection;
        [ReadOnly, SerializeField] private int _damageEstimation;
        [ReadOnly, SerializeField] private int _threatLevel;

        public Vector3 Position             { get => _position; set => _position = value; }
        public int DamageEstimation         { get => _damageEstimation; set => _damageEstimation = value; }
        public int ThreatLevel              { get => _threatLevel; set => _threatLevel = value; }
        public Vector3 PredictedDirection   { get => _predictedDirection; set => _predictedDirection = value; }

        public EnemyInfo()
        {
        }

        public EnemyInfo(Vector3 position, int damageEstimation = 0, int threatLevel = 0) : this()
        {
            Position = position;
            DamageEstimation = damageEstimation;
            ThreatLevel = threatLevel;
        }
    }

    [Serializable]
    class ObstacleInfo
    {
        [ReadOnly, SerializeField] private Vector3 _position;
        [ReadOnly, SerializeField] private float _distance;

        public Vector3 Position { get => _position; set => _position = value; }
        public float Distance   { get => _distance; set => _distance = value; }

        public ObstacleInfo()
        {
        }

        public ObstacleInfo(Vector3 position, float distance) : this()
        {
            Position = position;
            Distance = distance;
        }
    }

    enum PriorityType
    {
        Weapon,
        Fight,
        Heal,
        Armor,
        Flee,
        Explore,
        Center
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
    [SerializeField] private float obstacleAvoidanceRadius = .6f;

    [Header("Items and Usage")]
    [Range(0, 1f)]
    [SerializeField] private float healthThreshold  = .5f;
    [Range(0, 1f)]
    [SerializeField] private float armorThreshold   = .75f;

    [SerializeField] private int minAmountHealItems     = 2;
    [SerializeField] private int minAmountArmorItems    = 2;

    [Header("Movement")]
    [SerializeField] private float maxDistanceToNextRing = 75f;
    [SerializeField] private float maxDistanceOutsideOfRing = 10f;

    [Header("Prioritiy Bonuses")]
    [SerializeField] private int desirePriorityBonus = 1;
    [SerializeField] private int urgePriorityBonus = 5;
    [SerializeField] private int ThresholdBonus = 6;

    [Header("-----------------")]
    [Header("Brain Information")]
    [Header("_________________")]
    [ReadOnly, SerializeField] BrainState brainState;
    [ReadOnly, SerializeField] Vector3 desiredDirection;

    [Header("Actions")]
    [ReadOnly, SerializeField] Vector3 currentDirection;
    [ReadOnly, SerializeField] bool avoidObstacle;
    [ReadOnly, SerializeField] List<Pickup> pickupTargets;
    [ReadOnly, SerializeField] bool lootSurroundings;

    [Header("Stats")]
    [ReadOnly, SerializeField] int kills;
    [ReadOnly, SerializeField] int totalDamageTaken;
    [ReadOnly, SerializeField] int totalDamageDealt;
    [ReadOnly, SerializeField] private Weapon defaultWeapon;

    [Header("Priorities")]
    [ReadOnly, SerializeField] int weaponPriority;
    [ReadOnly, SerializeField] int fightPriority;
    [ReadOnly, SerializeField] int healPriority;
    [ReadOnly, SerializeField] int armorPriority;
    [ReadOnly, SerializeField] int fleePriority;
    [ReadOnly, SerializeField] int explorePriority;
    [ReadOnly, SerializeField] int centerPriority;
    [ReadOnly, SerializeField] Dictionary<PriorityType, int> priorities;

    [Header("Sensors")]
    [ReadOnly, SerializeField] bool canSeeEnemy;
    [ReadOnly, SerializeField] bool canSeeItem;
    [ReadOnly, SerializeField] bool canSeeWeapon;
    [ReadOnly, SerializeField] bool movingTowardsObstacle;
    [ReadOnly, SerializeField] private float currentAngleOffset = 0;

    [Header("Memory")]
    [ReadOnly, SerializeField] List<EnemyInfo>      enemyBots;     // Needs health/dmg tracking & player assosiation
    [ReadOnly, SerializeField] List<PickupInfo>     itemPickupInfos;      // needs item assosiation
    [ReadOnly, SerializeField] List<PickupInfo>     weaponPickupInfos;   // needs weapon type assosiation
    [ReadOnly, SerializeField] List<ObstacleInfo>   surroundingObstacles;
    [ReadOnly, SerializeField] List<ObstacleInfo>   obstaclesInFront;

    [ReadOnly, SerializeField] List<EnemyInfo>  lastKnowEnemyBots;     // Needs health/dmg tracking & player assosiation
    [ReadOnly, SerializeField] List<PickupInfo> lastKnowItemPickups;      // needs item assosiation
    [ReadOnly, SerializeField] List<PickupInfo> lastKnowWeaponPickups;   // needs weapon type assosiation

    [Header("Urges")]
    [ReadOnly, SerializeField] bool lookingForHealth;
    [ReadOnly, SerializeField] bool lookingForArmor;
    [ReadOnly, SerializeField] bool lookingForWeapon;
    [ReadOnly, SerializeField] bool shouldFight;
    [ReadOnly, SerializeField] bool shouldFlee;
    [ReadOnly, SerializeField] bool shouldExplore;
    [ReadOnly, SerializeField] bool shouldCenter;
    
    void Start()
    {
        bot = GetComponent<BattleBotInterface>();
        currentDirection = Random.insideUnitSphere;
        currentDirection.y = 0;
        currentDirection.Normalize();

        ResetBrain();
        RebuildPriorities();
    }

    void ResetBrain()
    {
        desiredDirection    = Vector3.zero;

        avoidObstacle       = false;
        currentDirection    = Vector3.zero;

        pickupTargets       = new List<Pickup>();
        lootSurroundings    = false;

        kills               = 0;
        totalDamageTaken    = 0;
        totalDamageDealt    = 0;

        RebuildPriorities();

        canSeeEnemy     = false;
        canSeeItem      = false;
        canSeeWeapon    = false;

        ClearInfo(true);

        lookingForHealth    = false;
        lookingForArmor     = false;
        lookingForWeapon    = false;
        shouldFight         = false;
        shouldFlee          = false;
        shouldExplore       = false;
        shouldCenter        = false;
    }

    void ClearInfo(bool clearInfoMemory = false)
    {
        enemyBots               = new List<EnemyInfo>();
        itemPickupInfos         = new List<PickupInfo>();
        weaponPickupInfos       = new List<PickupInfo>();

        surroundingObstacles    = new List<ObstacleInfo>();
        obstaclesInFront        = new List<ObstacleInfo>();

        if (clearInfoMemory)
        {
            lastKnowEnemyBots           = new List<EnemyInfo>();
            lastKnowItemPickups         = new List<PickupInfo>();
            lastKnowWeaponPickups       = new List<PickupInfo>();
        }
    }

    #region Action prioritization
    void RebuildPriorities()
    {
        weaponPriority  = 0;
        healPriority    = 0;
        armorPriority   = 0;

        fightPriority   = 0;
        fleePriority    = 0;

        explorePriority = 0;
        centerPriority  = 0;

        ApplyDesirePriorities();
        ApplyUrgePriorities();
        ApplyThresholdDesires();

        priorities = new Dictionary<PriorityType, int>()
        {
            // Looting Prios
            { PriorityType.Weapon,  weaponPriority  },
            { PriorityType.Heal,    healPriority    },
            { PriorityType.Armor,   armorPriority   },

            // Enemy Encounter Prios
            { PriorityType.Fight,   fightPriority   },
            { PriorityType.Flee,    fleePriority    },

            // Move Prios
            { PriorityType.Explore, explorePriority },
            { PriorityType.Center,  centerPriority  }
        };
    }
    void ApplyDesirePriorities()
    {
        // Set the base priorities
        switch (desiredState)
        {
            // Focus on looting and using items while evading
            // Fight with enough items
            // Flee when in danger
            case Focus.Loot:
                weaponPriority += desirePriorityBonus;
                healPriority += desirePriorityBonus;
                armorPriority += desirePriorityBonus;
                //fightPriority   = 0;
                //fleePriority    = 0;
                explorePriority += desirePriorityBonus;
                //centerPriority  = 0;
                break;

            // Focus on gearing up to minimum and looking 
            //  for enemies to fight
            // Stop fighting and heal up when reaching thresholds
            // Look for minimum items when none are available 
            //  to reach threshold
            case Focus.Hunt:
                weaponPriority += desirePriorityBonus;
                //healPriority    = 0;
                armorPriority += desirePriorityBonus;
                fightPriority += desirePriorityBonus;
                //fleePriority    = 0;
                explorePriority += desirePriorityBonus;
                //centerPriority  = 0;
                break;

            // Run away like a little **** until geared up, 
            //  enough time passed and fewer enemies remain
            case Focus.Survive:
                //weaponPriority  = 0;
                healPriority += desirePriorityBonus;
                armorPriority += desirePriorityBonus;
                //fightPriority   = 0;
                fleePriority += desirePriorityBonus;
                //explorePriority = 0;
                centerPriority += desirePriorityBonus;
                break;
        }
    }
    void ApplyUrgePriorities()
    {
        if (lookingForHealth)
        {
            healPriority    += urgePriorityBonus;
        }
        if (lookingForArmor)
        {
            armorPriority   += urgePriorityBonus;
        }
        if (lookingForWeapon)
        {
            weaponPriority  += urgePriorityBonus;
        }
        if (shouldFight)
        {
            fightPriority   += urgePriorityBonus;
        }
        if (shouldFlee)
        {
            fleePriority    += urgePriorityBonus;
        }
        if (shouldExplore)
        {
            explorePriority += urgePriorityBonus;
        }
        if (shouldCenter)
        {
            centerPriority  += urgePriorityBonus;
        }
    }
    void ApplyThresholdDesires()
    {
        if (CheckHealthThreshold())
        {
            healPriority += ThresholdBonus;
        }
        if (CheckArmorThreshold())
        {
            armorPriority += ThresholdBonus;
        }
    }
    #endregion

    #region Checking for urges
    void CheckUrges()
    {
        lookingForWeapon    = !CheckForEquippedWeapon();
        lookingForHealth    = CheckHeldHealItems();
        lookingForArmor     = CheckHeldArmorItems();

        //TODO - Add damage check to see if under attack
        if (!lookingForWeapon && !lookingForHealth && !lookingForArmor)
        {
            shouldFight     = true;
            shouldFlee      = false;
        }
        else
        {
            shouldFight     = false;
            shouldFlee      = true;
        }

        if (CheckInsideRingDistanceTreshold())
        {
            shouldExplore   = true;
            shouldCenter    = false;
        }
        else
        {
            shouldExplore   = false;
            shouldCenter    = true;
        }
    }
    bool CheckInsideRingDistanceTreshold()
    {
        var distanceToRingCenter = Vector3.Distance(transform.position, bot.RingCenter);
        var distanceToNextRingCenter = Vector3.Distance(transform.position, bot.NextRingCenter);
        /*print($"Current distance to ring: {distanceToRingCenter-maxDistanceOutsideOfRing} vs {bot.RingRadius}" +
            $"\n| Distance to next ring: {distanceToNextRingCenter-maxDistanceToNextRing} vs {bot.NextRingRadius}");
            */
        var isInRingThreshold = distanceToRingCenter - maxDistanceOutsideOfRing < bot.RingRadius;
        var isInNextRingThreshold = distanceToNextRingCenter - maxDistanceToNextRing < bot.NextRingRadius;

        return (bot.IsInRing || isInRingThreshold) && (bot.IsInNextRing || isInNextRingThreshold);
    }
    bool CheckHeldHealItems()
    {
        return CountHealingItems() < minAmountHealItems;
    }
    bool CheckHeldArmorItems()
    {
        return CountArmorItems() < minAmountArmorItems;
    }
    bool CheckForEquippedWeapon()
    {
        if (defaultWeapon == null)
            defaultWeapon = bot.weapon;

        return bot.weapon != null && bot.weapon != defaultWeapon;
    }
    #endregion

    void Update()
    {
        // Update behaviour base on desires, surroundings and self
        UpdateBrainState();

        // Create a list of pickups, enemies and possible obstacles
        //  and loop over all scan results and remember them
        AssignScanResults(ScanSurroundings());// LayerMask.GetMask("Bot", "Pickup")));

        // Act upon brain and memory
        TakeAction();
    }

    void UpdateBrainState()
    {
        CheckUrges();

        // Check for priorities to update brain state
        desiredDirection = transform.forward;

        RebuildPriorities();
    }

    List<AdvancedScanInfo> ScanSurroundings(LayerMask layerMask = default)
    {
        currentAngleOffset += 2 * Mathf.PI / rayStep;
        var angleStep = 2 * Mathf.PI / rayCount;

        var scans = new List<AdvancedScanInfo>();

        if (currentAngleOffset > angleStep)
        {
            currentAngleOffset = 0;
        }

        for (var i = 0f; i < Mathf.PI * 2; i += angleStep)
        {
            var direction = new Vector3(Mathf.Cos(i + currentAngleOffset), 0, Mathf.Sin(i + currentAngleOffset));
            var scan = bot.Scan(direction, layerMask);
            scans.Add(new AdvancedScanInfo(scan, direction));
        }

        return scans;
    }

    void AssignScanResults(List<AdvancedScanInfo> scans)
    {
        var direction = currentDirection;

        ClearInfo();

        foreach (var scanHit in scans)
        {
            if (scanHit.Type == HitType.Item)
            {
                // Remember pickup
                AddItemInfo(scanHit, true, true);
            }
            else if (scanHit.Type == HitType.Enemy)
            {
                // Remember enemy
                AddEnemyInfo(scanHit);
            }
            else if (scanHit.Type == HitType.World)
            {
                // Note obstacles
                AddObstacleInfo(scanHit);
            }
        }
    }

    void AddObstacleInfo(AdvancedScanInfo obstacle)
    {
        // Check if obstacle is in the way of the desired direction
        // TODO - Use bots collider diameter to check if it can fit in between two obstacles at any distance
        var obstacleInfo = new ObstacleInfo(transform.position + obstacle.Direction * obstacle.Distance,
            obstacle.Distance);
        if (AreObstaclesBlockingDesiredDirection(obstacle.Direction, obstacle.Distance))
        {
            obstaclesInFront.Add(obstacleInfo);
        }
        else
        {
            surroundingObstacles.Add(obstacleInfo);
        }
    }

    bool AreObstaclesBlockingDesiredDirection(Vector3 direction, float distance)
    {
        Debug.DrawLine(transform.position, transform.position + desiredDirection.normalized*2, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + direction.normalized*2, Color.yellow);
        
        var dot = Vector3.Dot(desiredDirection.normalized, direction.normalized);
        return dot > 0.95 && distance <= obstacleAvoidanceRadius;
    }

    void AddEnemyInfo(AdvancedScanInfo enemy, bool remember = false, bool checkAgainstMemory = false)
    {
        // +.5 for the radius
        var enemyInfo = new EnemyInfo(transform.position + enemy.Direction * (enemy.Distance + .5f));

        Debug_MarkPosition(enemyInfo.Position, Color.red);
        enemyBots.Add(enemyInfo);
        canSeeEnemy = true;

        // TODO - prediction like a check for vicinty (has the bot move from last known location)
        if (checkAgainstMemory)
        {
            enemyInfo = new EnemyInfo();
        }
        if (remember) lastKnowEnemyBots.Add(enemyInfo);
    }

    void AddItemInfo(AdvancedScanInfo pickup, bool remember = false, bool checkAgainstMemory = false)
    {
        var pickupInfo = new PickupInfo(pickup.Pickup);
        if (pickupInfo.Pickup is PickupWeapon)
        {
            
            pickupInfo.Type = PickupType.Weapon;
        }
        else if (pickupInfo.Pickup is PickupItem)
        {
            if (((PickupItem)pickupInfo.Pickup).item is HealingItem)
            {
                pickupInfo.Type = PickupType.HealingConsumable;
            }
            else if (((PickupItem)pickupInfo.Pickup).item is ArmorItem)
            {
                pickupInfo.Type = PickupType.ArmorConsumable;
            }
        }

        switch (pickupInfo.Type)
        {
            case PickupType.Weapon:

                // Add weapon to direct vision if not already seen to avoid duplicate vision
                if (weaponPickupInfos?.Find(x => x.Pickup == pickupInfo.Pickup) == null)
                {
                    weaponPickupInfos.Add(pickupInfo);
                    canSeeWeapon = true;
                }
                // Check to find if the weapon already exists in memory
                if (checkAgainstMemory)
                {
                    IsPickupMemorized(pickupInfo.Pickup, pickupInfo.Type, true);
                }

                if (remember) lastKnowWeaponPickups.Add(pickupInfo);
                break;

            case PickupType.HealingConsumable:
            case PickupType.ArmorConsumable:

                // Add item to direct vision if not already seen to avoid duplicate vision
                if (itemPickupInfos?.Find(x => x.Pickup == pickupInfo.Pickup) == null)
                {
                    itemPickupInfos.Add(pickupInfo);
                    canSeeItem = true;
                }

                // Check to find if the weapon already exists in memory
                if (checkAgainstMemory)
                {
                    IsPickupMemorized(pickupInfo.Pickup, pickupInfo.Type, true);
                }

                if (remember) lastKnowItemPickups.Add(pickupInfo);
                break;
        }
    }

    bool IsPickupMemorized(Pickup pickup, PickupType type, bool removeFromMemory = false)
    {
        switch (type)
        {
            case PickupType.Weapon:
                if (lastKnowWeaponPickups?.Count > 0)
                {
                    var memorizedInfo = lastKnowWeaponPickups?.Find(x => x.Pickup == pickup);
                    //print($"Trying to remember {pickupInfo.Pickup.name}: {memorizedInfo?.Pickup?.name}");

                    if (removeFromMemory && memorizedInfo != null)
                    {
                        //print("Updating memory");
                        lastKnowWeaponPickups.Remove(memorizedInfo);
                    }
                    return memorizedInfo != null;
                }
                return false;

            case PickupType.HealingConsumable:
            case PickupType.ArmorConsumable:
                if (lastKnowItemPickups?.Count > 0)
                {
                    var memorizedInfo = lastKnowItemPickups?.Find(x => x.Pickup == pickup);
                    //print($"Trying to remember {pickupInfo.Pickup.name}: {memorizedInfo?.Pickup?.name}");

                    if (removeFromMemory && memorizedInfo != null)
                    {
                        //print("Updating memory");
                        lastKnowItemPickups.Remove(memorizedInfo);
                    }
                    return memorizedInfo != null;
                }
                return false;
            default:
                return false;
        }
    }

    float WeaponDPS(Weapon weapon)
    {
        // TODO integrate desired range with prescision to calculate average hit rate
        return weapon.damage / weapon.fireDelay;
    }

    void TakeAction()
    {
        PrioritizeSituation();

        Move();
        Loot();

        if (CheckHealthThreshold())
        {
            HealUp();
        }
        else if (CheckArmorThreshold())
        {
            ArmorUp();
        }

        Fight();
    }

    void PrioritizeSituation()
    {
        
    }
    
    bool CheckHealthThreshold()
    {
        return bot.health < Mathf.Ceil(100 * healthThreshold);
    }

    int CountHealingItems(int minHeal = 0)
    {
        var count = 0;
        foreach (var item in bot.items)
        {
            if (item is HealingItem && ((HealingItem)item).Amount >= minHeal)
                count++;
        }
        //print($"Heal items held: {count}");

        return count;
    }

    bool CheckArmorThreshold()
    {
        return bot.armor < Mathf.Ceil(100 * armorThreshold);
    }

    int CountArmorItems(int minArmor = 0)
    {
        var count = 0;
        foreach (var item in bot.items)
        {
            if (item is ArmorItem && ((ArmorItem)item).Amount >= minArmor)
                count++;
        }
        //print($"Armor items held: {count}");

        return count;
    }

    void Move()
    {
        var direction = new Vector3();
        foreach (var obstacle in obstaclesInFront)
        {

        }

        foreach (var obstacle in surroundingObstacles)
        {
            if (obstacle.Distance <= obstacleAvoidanceRadius)
            {
                /*direction = Vector3.Slerp(currentDirection, -scanHit.Direction,
                    (obstacleAvoidanceRadius - scanHit.Distance) / obstacleAvoidanceRadius);
                print($"Avoiding obstacle: {direction.ToString()}");*/
            }
        }
        currentDirection = direction;
        shouldExplore = true;

        if (shouldExplore) bot.Move(currentDirection);
    }

    void Loot()
    {
        if (lootSurroundings)
        {
            foreach (var pickup in pickupTargets)
            {
                if (Vector3.Distance(pickup.transform.position, transform.position) <= GameManager.instance.pickupRange)
                {
                    if (pickup is PickupWeapon)
                    {
                        EquipWeapon((PickupWeapon)pickup);
                    }
                    else if (pickup is PickupItem)
                    {
                        AddItemToInventory((PickupItem)pickup);
                    }
                }
            }
        }
    }

    void EquipWeapon(PickupWeapon pickup)
    {
        // Ignore lower dps weapons
        // TODO - Add class specific preferences (fighter -> favors high dps/evader & looter -> favor high range)
        if (lookingForWeapon || WeaponDPS(pickup.weapon) > WeaponDPS(bot.weapon))
        {
            print($"Equipping new weapon: {pickup.weapon.weaponName}");
            lookingForWeapon = false;
            
            // Remove from memory since it was just picked up
            IsPickupMemorized(pickup, PickupType.Weapon, true);
            bot.Pickup(pickup);
        }
    }

    void AddItemToInventory(PickupItem pickup)
    {
        bot.Pickup(pickup);

        // Remove from memory since it was just picked up
        // Type doesn't matter, since they are both in the same list
        IsPickupMemorized(pickup, PickupType.HealingConsumable, true);

        if (pickup.item is HealingItem)
        {
            // TODO remember healing amount
            // ...
        }
        else if (pickup.item is ArmorItem)
        {
            // TODO remember armor amount
            // ...
        }
    }

    void Fight()
    {

    }

    void Flee()
    {

    }

    void Explore ()
    {

    }

    void Center()
    {

    }

    bool HealUp()
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

    bool ArmorUp()
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

    void Debug_MarkPosition(Vector3 position, Color color, float scale = 1)
    {
        Debug.DrawLine(position - Vector3.forward * scale, position + Vector3.forward * scale, color, Time.deltaTime);
        Debug.DrawLine(position - Vector3.right * scale, position + Vector3.right * scale, color, Time.deltaTime);
    }
}
