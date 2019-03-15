using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geert : MonoBehaviour
{
    BattleBotInterface BBInterface;
    private Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        BBInterface = GetComponent<BattleBotInterface>();
        direction = new Vector3(Random.value, 0, Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0f; i < Mathf.PI * 2; i += Mathf.PI / 8)
        {
            var dir = new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i));
            var scan = BBInterface.Scan(dir);

            if (scan.type == HitType.World)
            {

            }
            else if (scan.type == HitType.Enemy)
            {
                BBInterface.Shoot(dir);
            }
            else if (scan.type == HitType.Item)
            {

            }

        }


        BBInterface.Move(direction);
    }

}
