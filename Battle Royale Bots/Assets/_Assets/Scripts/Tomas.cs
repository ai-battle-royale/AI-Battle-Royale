using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tomas : MonoBehaviour
{
    BattleBotInterface Manager;
    private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        Manager = GetComponent<BattleBotInterface>();
        direction = new Vector3(Random.value, 0, Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        Manager.Scan(direction);
        var scanned = Manager.Scan(direction);
        if (scanned.Type == HitType.World)
        { 
            direction = new Vector3(Random.value, 0, Random.value);
        }
        Manager.Move(direction);
    }
}
