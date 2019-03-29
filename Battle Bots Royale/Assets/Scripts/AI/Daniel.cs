/* WIP Battle Bot Brain with different behaviour modes by Daniel
 * Last Udpate: 15.03.19
 * ***/


using System;
using System.Linq;
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

        public float GetDistanceTo(Vector3 position)
        {
            return Vector3.Distance(position, Position);
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

    [Serializable]
    class Priority
    {
        [ReadOnly, SerializeField] private PriorityType _type;
        [ReadOnly, SerializeField] private int _value;

        public int Value            { get => _value; set => _value = value; }
        public PriorityType Type    { get => _type; set => _type = value; }

        public Priority()
        {
        }

        public Priority(PriorityType type, int value) : this()
        {
            Value = value;
            Type = type;
        }
    }

    private BattleBotInterface bot;

    [Header("-----------------")]
    [Header("Behaviour Tweaking")]
    [Header("_________________")]
    [SerializeField] private Focus desiredState = Focus.Loot;
    [SerializeField] private bool enableDebug = true;

    [Header("Scanning")]
    [SerializeField] private int rayCount = 16;
    [Tooltip("Rotation step for each scan (higher is number means samller steps)")]
    [SerializeField] private int raySteps = 64;
    [SerializeField] private float obstacleAvoidanceRadius = .6f;
    [SerializeField] private int frontRayCount = 2;
    [Tooltip("How much distance should each ray be apart? (multiplied by 2 pi)")]
    [SerializeField] private float frontRaySpread = 0.02f;

    [Header("Items and Usage")]
    [Range(0, 1f)]
    [SerializeField] private float healthThreshold  = .5f;
    [Range(0, 1f)]
    [SerializeField] private float armorThreshold   = .75f;

    [SerializeField] private int minAmountHealItems     = 2;
    [SerializeField] private int minAmountArmorItems    = 2;

    [Header("Movement")]
    [SerializeField] private float maxDistanceToNextRing = 75f;
    [SerializeField] private float maxExploreTime = 3f;

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
    [ReadOnly, SerializeField] Vector3 desiredPosition;
    [ReadOnly, SerializeField] Vector3 lastDesiredPosition;
    [ReadOnly, SerializeField] Vector3 explorePosition;
    [ReadOnly, SerializeField] float newExploreLocationTimestamp;
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
    [ReadOnly, SerializeField] List<Priority> priorities;

    [Header("Sensors")]
    [ReadOnly, SerializeField] bool canSeeEnemy;
    [ReadOnly, SerializeField] bool canSeeItem;
    [ReadOnly, SerializeField] bool canSeeWeapon;
    [ReadOnly, SerializeField] bool movingTowardsObstacle;
    [ReadOnly, SerializeField] float currentAngleOffset;

    [Header("Memory")]
    [ReadOnly, SerializeField] List<EnemyInfo>      enemyInfos;     // Needs health/dmg tracking & player assosiation
    [ReadOnly, SerializeField] List<PickupInfo>     itemPickupInfos;      // needs item assosiation
    [ReadOnly, SerializeField] List<PickupInfo>     weaponPickupInfos;   // needs weapon type assosiation
    [ReadOnly, SerializeField] List<ObstacleInfo>   surroundingObstacles;
    [ReadOnly, SerializeField] List<ObstacleInfo>   obstaclesInFront;

    [ReadOnly, SerializeField] List<EnemyInfo>  lastKnowEnemyBots;     // Needs health/dmg tracking & player assosiation
    [ReadOnly, SerializeField] List<PickupInfo> lastKnowItemPickups;      // needs item assosiation
    [ReadOnly, SerializeField] List<PickupInfo> knowWeaponPickups;   // needs weapon type assosiation

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
    }

    void ResetBrain()
    {
        desiredDirection    = Vector3.zero;
        desiredDirection    = Vector3.negativeInfinity;

        avoidObstacle       = false;
        currentDirection    = Vector3.zero;

        desiredPosition     = transform.position;
        desiredPosition     = Vector3.zero;
        explorePosition     = Vector3.negativeInfinity;
        newExploreLocationTimestamp = 0;
        lootSurroundings    = false;

        kills               = 0;
        totalDamageTaken    = 0;
        totalDamageDealt    = 0;

        ClearPriorities();

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
        enemyInfos               = new List<EnemyInfo>();
        itemPickupInfos         = new List<PickupInfo>();
        weaponPickupInfos       = new List<PickupInfo>();

        surroundingObstacles    = new List<ObstacleInfo>();
        obstaclesInFront        = new List<ObstacleInfo>();

        if (clearInfoMemory)
        {
            lastKnowEnemyBots           = new List<EnemyInfo>();
            lastKnowItemPickups         = new List<PickupInfo>();
            knowWeaponPickups       = new List<PickupInfo>();
        }
    }

    void ClearPriorities()
    {
        weaponPriority = 0;
        healPriority = 0;
        armorPriority = 0;

        fightPriority = 0;
        fleePriority = 0;

        explorePriority = 0;
        centerPriority = 0;
    }

    #region Action prioritization
    void RebuildPriorities()
    {
        ClearPriorities();
        ApplyDesirePriorities();
        ApplyUrgePriorities();
        ApplyThresholdDesires();

        priorities = new List<Priority>()
        {
            // Looting Prios
            new Priority(PriorityType.Weapon,  weaponPriority),
            new Priority(PriorityType.Heal,    healPriority),
            new Priority(PriorityType.Armor,   armorPriority),

            // Enemy Encounter Prios
            new Priority(PriorityType.Fight,   fightPriority),
            new Priority(PriorityType.Flee,    fleePriority),

            // Move Prios
            new Priority(PriorityType.Explore, explorePriority),
            new Priority(PriorityType.Center,  centerPriority)
        };

        // Highest to lowest
        priorities.Sort((x, y) => y.Value.CompareTo(x.Value));
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
        var distanceToNextRingCenter = Vector3.Distance(transform.position, bot.NextRingCenter);
        /*print($"Current distance to ring: {distanceToRingCenter-maxDistanceOutsideOfRing} vs {bot.RingRadius}" +
            $"\n| Distance to next ring: {distanceToNextRingCenter-maxDistanceToNextRing} vs {bot.NextRingRadius}");
            */
        var isInNextRingThreshold = distanceToNextRingCenter - maxDistanceToNextRing < bot.NextRingRadius;

        return bot.IsInRing && (bot.IsInNextRing || isInNextRingThreshold);
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
        currentAngleOffset += 2 * Mathf.PI / raySteps;
        ClearInfo();
        AssignScanResults(ScanSurroundings(LayerMask.GetMask("Bot", "Pickup")));
        AssignScanResults(ScanSurroundings(~LayerMask.GetMask("Bot", "Pickup")));

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
        surroundingObstacles.Add(obstacleInfo);
    }

    void AddEnemyInfo(AdvancedScanInfo enemy, bool remember = false, bool checkAgainstMemory = false)
    {
        // +.5 for the radius
        var enemyInfo = new EnemyInfo(transform.position + enemy.Direction * (enemy.Distance + .5f));

        Debug_MarkPosition(enemyInfo.Position, Color.red);
        enemyInfos.Add(enemyInfo);
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

                if (remember) knowWeaponPickups.Add(pickupInfo);
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
                if (knowWeaponPickups?.Count > 0)
                {
                    var memorizedInfo = knowWeaponPickups?.Find(x => x.Pickup == pickup);
                    //print($"Trying to remember {pickupInfo.Pickup.name}: {memorizedInfo?.Pickup?.name}");

                    if (removeFromMemory && memorizedInfo != null)
                    {
                        //print("Updating memory");
                        knowWeaponPickups.Remove(memorizedInfo);
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
        // Where do I want to go? What are my priorities?
        Move();

        // Are there any items neraby? Loot them?
        Loot();

        // Do I need to heal up?
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

    private void Fight()
    {
        if (enemyInfos?.Count > 0)
        {
            enemyInfos.Sort((x, y) => y.DamageEstimation.CompareTo(x.DamageEstimation));
            bot.Shoot(enemyInfos[0].Position - transform.position);
        }
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
        // Where do I want to go?
        foreach (var priority in priorities)
        {
            var isPrioritized = false;
            switch (priority.Type)
            {
                case PriorityType.Weapon:
                    isPrioritized = MoveTowardsWeapon();
                    break;
                case PriorityType.Heal:
                    isPrioritized = MoveTowardsHeal();
                    break;
                case PriorityType.Armor:
                    isPrioritized = MoveTowardsArmor();
                    break;
                case PriorityType.Fight:
                    isPrioritized = MoveTowardsFight();
                    break;
                case PriorityType.Flee:
                    isPrioritized = FleeFromThreat();
                    break;
                case PriorityType.Explore:
                    isPrioritized = ExploreArea();
                    break;
                case PriorityType.Center:
                    isPrioritized = MoveTowardsCenter();
                    break;
            }
            if (isPrioritized) break;
        }

        if (lastDesiredPosition != desiredPosition)
        {
            lastDesiredPosition = desiredPosition;
            UpdateDesiredDirection();
        }

        Debug_MarkPosition(desiredPosition, Color.yellow);

        // Is there anything too close to me?
        foreach (var obstacle in surroundingObstacles)
        {
            // TODO check for obstacle avoidance distance
        }

        // Avoid anything that is blocking my way
        AvoidBlockingObstacles();

        bot.Move(currentDirection);

        Debug.DrawLine(transform.position, transform.position + currentDirection.normalized * 2, Color.magenta);
        Debug.DrawLine(transform.position, transform.position + desiredDirection.normalized * 2, Color.yellow);
    }

    private void UpdateDesiredDirection()
    {
        desiredDirection = desiredPosition - transform.position;
        desiredDirection.y = 0; // lock direction to 2d

        currentDirection = desiredDirection;
    }

    private void AvoidBlockingObstacles()
    {
        obstaclesInFront = new List<ObstacleInfo>();

        var layerMask = ~LayerMask.GetMask("Bot", "Pickup");

        // Is there anything in front of that needs to be avoided?
        var frontScan = bot.Scan(currentDirection, layerMask, true);
        if (frontScan.type == HitType.World && frontScan.distance <= obstacleAvoidanceRadius)
        {
            obstaclesInFront.Add(new ObstacleInfo(transform.position + currentDirection * frontScan.distance, frontScan.distance));
        }
        else
        {
            UpdateDesiredDirection();
            return;
        }
        
        // Scan for alternatives
        var angleStep = 2 * Mathf.PI * frontRaySpread;
        var angleOffset = angleStep * frontRaySpread;
        var botRotationOffset = Quaternion.LookRotation(Quaternion.AngleAxis(-90f, Vector3.up) * currentDirection);

        for (var i = -frontRayCount; i <= frontRayCount; i ++)
        {
            if (i == 0) continue;

            var direction = botRotationOffset * new Vector3(Mathf.Cos(angleStep * i), 0, Mathf.Sin(angleStep * i));
            var scan = bot.Scan(direction, layerMask, true);
            if (scan.type == HitType.World && scan.distance <= obstacleAvoidanceRadius)
            {
                obstaclesInFront.Add(new ObstacleInfo(transform.position + direction * scan.distance, scan.distance));
            }
            else
            {
                // change direction to unblocked path
                currentDirection = direction;
                return;
            }
        }

        // Couldn't find an unbocked alternative, so go in the direction with the highest distance to obstacle
        obstaclesInFront.Sort((x, y) => y.Distance.CompareTo(x.Distance));
        currentDirection = obstaclesInFront[0].Position - transform.position;

        //AvoidBlockingObstacles();
    }

    bool MoveTowardsWeapon()
    {
        // TODO compare dps per distance metric
        if (weaponPickupInfos?.Count > 0)
        {
            weaponPickupInfos.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = weaponPickupInfos[0].Pickup.transform.position;
            Log("M Weapon");
            return true;
        }
        else if (knowWeaponPickups?.Count > 0)
        {
            knowWeaponPickups.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = knowWeaponPickups[0].Pickup.transform.position;
            Log("M Weapon");
            return true;
        }
        return false;
    }

    bool MoveTowardsArmor()
    {

        // TODO compare amount
        var armorInfos = itemPickupInfos.FindAll(x => x.Type == PickupType.ArmorConsumable);
        if (armorInfos?.Count > 0)
        {
            armorInfos.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = armorInfos[0].Pickup.transform.position;
            Log("M Armor");
            return true;
        }

        var knownArmorInfos = lastKnowItemPickups.FindAll(x => x.Type == PickupType.ArmorConsumable);
        if (knownArmorInfos?.Count > 0)
        {
            knownArmorInfos.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = knownArmorInfos[0].Pickup.transform.position;
            Log("M Armor");
            return true;
        }
        return false;
    }

    bool MoveTowardsHeal()
    {
        // TODO compare amount
        var healInfos = itemPickupInfos.FindAll(x => x.Type == PickupType.HealingConsumable);
        if (healInfos?.Count > 0)
        {
            healInfos.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = healInfos[0].Pickup.transform.position;
            Log("M Heal");
            return true;
        }

        var knownHealInfos = lastKnowItemPickups.FindAll(x => x.Type == PickupType.HealingConsumable);
        if (knownHealInfos?.Count > 0)
        {
            knownHealInfos.Sort((x, y) => x.GetDistanceTo(transform.position).CompareTo(y.GetDistanceTo(transform.position)));
            desiredPosition = knownHealInfos[0].Pickup.transform.position;
            Log("M Heal");
            return true;
        }
        return false;
    }

    bool MoveTowardsFight()
    {
        // TODO - Get enemys locations from memory
        // TODO - Duck in and out of range/cover depending on weapon fire rate
        // Move in range of the enemy with the highest tracked damage taken
        if (enemyInfos?.Count > 0)
        {
            enemyInfos.Sort((x, y) => y.DamageEstimation.CompareTo(x.DamageEstimation));

            // Move into attack range
            if (Vector3.Distance(enemyInfos[0].Position, transform.position) < bot.weapon.range)
                desiredPosition = (enemyInfos[0].Position - transform.position).normalized;
            Log("M Fight");
            return true;
        }
        return false;
    }

    bool FleeFromThreat()
    {
        // Flee from enemy with highest threat level within vision
        if (enemyInfos.Count > 0)
        {
            enemyInfos.Sort((x, y) => y.ThreatLevel.CompareTo(x.ThreatLevel));
            desiredPosition = (transform.position - enemyInfos[0].Position).normalized;
            Log("M Flee");
            return true;
        }
        return false;
    }

    bool MoveTowardsCenter()
    {
        Log("M Ring");
        desiredPosition = bot.NextRingCenter;
        return true;
    }

    bool ExploreArea()
    {
        // If explore position is reached, set new position
        if (Vector3.Distance(transform.position, explorePosition) <= 5f || Time.time >= newExploreLocationTimestamp)
        {
            explorePosition = Vector3.negativeInfinity;
        }

        if (explorePosition.Equals(Vector3.negativeInfinity))
        {
            desiredPosition = explorePosition = GetRandomLocationInsideRing();
            newExploreLocationTimestamp = Time.time + maxExploreTime; // ten second timer
            Log("M Explore");
        }
        return true;
    }

    Vector3 GetRandomLocationInsideRing()
    {
        var random = (Random.insideUnitCircle - Vector2.one / 2f) * bot.RingRadius;
        var possibleLocation = bot.RingCenter + new Vector3(random.x, 0, random.y);

        possibleLocation.x = Mathf.Clamp(possibleLocation.x, -97f, 97f);
        possibleLocation.z = Mathf.Clamp(possibleLocation.z, -97f, 97f);

        return possibleLocation;
    }

    void Loot()
    {
        lootSurroundings = lookingForWeapon || lookingForArmor || lookingForHealth;
        if (lootSurroundings)
        {
            var pickupsInSight = new List<PickupInfo>();
            pickupsInSight.AddRange(weaponPickupInfos);
            pickupsInSight.AddRange(itemPickupInfos);

            foreach (var pickupInfo in pickupsInSight)
            {
                var distanceToPickup = Vector3.Distance(pickupInfo.Pickup.transform.position, transform.position);
                if (distanceToPickup <= GameManager.instance.pickupRange)
                {
                    if (pickupInfo.Pickup is PickupWeapon)
                    {
                        EquipWeapon((PickupWeapon)pickupInfo.Pickup);
                    }
                    else if (pickupInfo.Pickup is PickupItem)
                    {
                        AddItemToInventory((PickupItem)pickupInfo.Pickup);
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
        if (!enableDebug) return;
        Debug.DrawLine(position - Vector3.forward * scale, position + Vector3.forward * scale, color, Time.deltaTime);
        Debug.DrawLine(position - Vector3.right * scale, position + Vector3.right * scale, color, Time.deltaTime);
    }

    void Log(string text)
    {
        if (!enableDebug) return;
        Debug.Log(text);
    }
}
