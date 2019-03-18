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


    [Header("Priorities")]
    [SerializeField] private int desirePriorityBonus = 1;
    [SerializeField] private int urgePriorityBonus = 5;
    [SerializeField] private int threshHoldBonus = 6;

    [Header("-----------------")]
    [Header("Brain Information")]
    [Header("_________________")]
    [ReadOnly, SerializeField] BrainState brainState;
    [ReadOnly, SerializeField] Vector3 desiredDirection;

    [Header("Actions")]
    [ReadOnly, SerializeField] Vector3 currentDirection;
    [ReadOnly, SerializeField] bool avoidObstacle;
    [ReadOnly, SerializeField] Pickup pickupTarget;
    [ReadOnly, SerializeField] bool shouldPickupItem;

    [Header("Stats")]
    [ReadOnly, SerializeField] int kills;
    [ReadOnly, SerializeField] int totalDamageTaken;
    [ReadOnly, SerializeField] int totalDamageDealt;

    [Header("Priorities")]
    [ReadOnly, SerializeField] Dictionary<PriorityType, int> priorities;
    [ReadOnly, SerializeField] int weaponPriority;
    [ReadOnly, SerializeField] int fightPriority;
    [ReadOnly, SerializeField] int healPriority;
    [ReadOnly, SerializeField] int armorPriority;
    [ReadOnly, SerializeField] int fleePriority;
    [ReadOnly, SerializeField] int explorePriority;
    [ReadOnly, SerializeField] int centerPriority;

    [Header("Sensors")]
    [ReadOnly, SerializeField] bool canSeeEnemy;
    [ReadOnly, SerializeField] bool canSeeItem;
    [ReadOnly, SerializeField] bool canSeeWeapon;
    [ReadOnly, SerializeField] bool movingTowardsObstacle;

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

    private float currentAngleOffset = 0;
    
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
        desiredDirection = Vector3.zero;

        avoidObstacle = false;
        currentDirection = Vector3.zero;

        pickupTarget = null;
        shouldPickupItem      = false;

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
        ApplyThreshholdDesires();

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
                weaponPriority += desirePriorityBonus * 2;
                healPriority += desirePriorityBonus * 2;
                armorPriority += desirePriorityBonus * 2;
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
                fightPriority += desirePriorityBonus * 2;
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
                fleePriority += desirePriorityBonus * 2;
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
    void ApplyThreshholdDesires()
    {
        if (CheckHealth())
        {
            healPriority += threshHoldBonus;
        }
        if (CheckArmor())
        {
            armorPriority += threshHoldBonus;
        }
    }

    void CheckSelf()
    {
        lookingForWeapon    = CheckEquippedWeapon();
        lookingForHealth    = CheckHeldHealItems();
        lookingForArmor     = CheckHeldArmorItems();

        if (!lookingForWeapon && !lookingForHealth && !lookingForArmor)
        {
            shouldFight = true;
            shouldCenter = true;
        }
    }

    bool CheckHeldHealItems()
    {
        return CountHealingItems() < minAmountHealItems;
    }

    bool CheckHeldArmorItems()
    {
        return CountArmorItems() < minAmountArmorItems;
    }

    bool CheckEquippedWeapon()
    {
        return bot.weapon == null;
    }

    void UpdateBrainState()
    {
        CheckSelf();

        // Check for priorities to update brain state
        desiredDirection = transform.forward;


        RebuildPriorities();
    }

    void UpdateBotBehaviour()
    {
        // Set behaviour base on brain state
    }

    void Update()
    {
        // Update behaviour base on desires, surroundings and self
        UpdateBrainState();
        UpdateBotBehaviour();

        // Create a list of pickups, enemies and possible obstacles
        var scans = ScanSurroundings(); // LayerMask.GetMask("Bot", "Pickup"));

        // Loop over all scan results and remember them
        AssignScanResults(scans);

        // Act upon brain and memory
        TakeAction();
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

        // TODO - prediction
        if (checkAgainstMemory)
        {
            //TODO - Check for vicinty (has the bot move from last known location)
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

                weaponPickupInfos.Add(pickupInfo);
                canSeeWeapon = true;

                // Check to find if the weapon already exists in memory
                if (checkAgainstMemory && lastKnowWeaponPickups?.Count > 0)
                {
                    var memorizedInfo = lastKnowWeaponPickups?.Find(x => x.Pickup == pickupInfo.Pickup);
                    //print($"Trying to remember {pickupInfo.Pickup.name}: {memorizedInfo?.Pickup?.name}");

                    if (memorizedInfo != null)
                    {
                        print("Updating memory");
                        lastKnowWeaponPickups.Remove(memorizedInfo);
                    }
                }

                if (remember) lastKnowWeaponPickups.Add(pickupInfo);
                break;

            case PickupType.HealingConsumable:
            case PickupType.ArmorConsumable:

                itemPickupInfos.Add(pickupInfo);
                canSeeItem = true;

                // Check to find if the weapon already exists in memory
                if (checkAgainstMemory && lastKnowItemPickups?.Count > 0)
                {
                    var memorizedInfo = lastKnowItemPickups?.Find(x => x.Pickup == pickupInfo.Pickup);
                    //print($"Trying to remember {pickupInfo.Pickup.name}: {memorizedInfo?.Pickup?.name}");

                    if (memorizedInfo != null)
                    {
                        print("Updating memory");
                        lastKnowItemPickups.Remove(memorizedInfo);
                    }
                }

                if (remember) lastKnowItemPickups.Add(pickupInfo);
                break;
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

        if (CheckHealth())
        {
            lookingForHealth = Heal();
        }
        else if (CheckArmor())
        {
            lookingForArmor = Armor();
        }

        Fight();
    }

    void PrioritizeSituation()
    {
        // Behave normally
        if (bot.IsInRing)
        {
            
        }
    }
    
    bool CheckHealth()
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
        return count;
    }

    bool CheckArmor()
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
        if (shouldPickupItem && pickupTarget != null)
        {
            if (Vector3.Distance(pickupTarget.transform.position, transform.position) < GameManager.instance.pickupRange)
            {
                if (pickupTarget is PickupWeapon)
                {
                    // Ignore lower dps weapons
                    if (lookingForWeapon || WeaponDPS(((PickupWeapon)pickupTarget).weapon) > WeaponDPS(bot.weapon))
                    {
                        // TODO - Remove weapon from memory
                        lookingForWeapon = false;
                        bot.Pickup(pickupTarget);
                    }
                }
                else if (pickupTarget is PickupItem)
                {
                    if (((PickupItem)pickupTarget).item is HealingItem)
                    {
                        
                    }
                    else if (((PickupItem)pickupTarget).item is ArmorItem)
                    {
                        
                    }
                }
                
            }
        }
    }

    void Fight()
    {

    }

    void Explore ()
    {

    }

    void Evade()
    {

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

    

    void Debug_MarkPosition(Vector3 position, Color color, float scale = 1)
    {
        Debug.DrawLine(position - Vector3.forward * scale, position + Vector3.forward * scale, color, Time.deltaTime);
        Debug.DrawLine(position - Vector3.right * scale, position + Vector3.right * scale, color, Time.deltaTime);
    }
}
