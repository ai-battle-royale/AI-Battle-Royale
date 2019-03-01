using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geert : MonoBehaviour
{
    BattleBotInterface Manager;

    // Start is called before the first frame update
    void Start()
    {
        Manager = GetComponent<BattleBotInterface>();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
