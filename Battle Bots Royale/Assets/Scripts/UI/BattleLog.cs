using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class BattleLog : MonoBehaviour
{
    BattleBotInterface bbInterface;
    GameObject[] bots;
    Text text;
    public float moveAmount;
    public float moveSpeed;
    public float ttl = 4f;
    Vector3 moveDirection;
    bool canMove;
    string botName;
    Vector3 originalPos;
    string[] killMoveMessages =
    {
        "killed",
        "deleted",
        "obliterated",
        "set a null pointer to",
        "evaporated",
        "removed",
        "destroyed",
        "annihilated",
        "used his death stare on",
        "murdered",
        "slaughtered",
        "erased",
        "360 noscoped",
        "ran over",
        "finished",
        "demolished",
        "pulled a lag switch on",
        "massacred",
        "phaser po paser laserd",
        "shot",
        "brutalized",
        "oofed",
        "pull a quick one on",
        "pulled the plug on",
        "did some heavy damage to"
        //"used bdsm without a safe word",
        //"didn't use protection on"
    };

    // Start is called before the first frame update
    void Start()
    {
        originalPos = gameObject.transform.position;

        moveDirection = Vector3.up;

        text = GetComponent<Text>();

        bots = GameObject.FindGameObjectsWithTag("Bot");
    }

    private void OnEnable()
    {
        BattleBotEvents.BotKilledBot += BattleBotEvents_BotKilledBot; ;
    }
    private void OnDisable()
    {
        BattleBotEvents.BotKilledBot -= BattleBotEvents_BotKilledBot;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, moveAmount * (moveSpeed * Time.deltaTime));
        }

        /*foreach (GameObject bot in bots)
        {
            if (bot != null)
            {
                bbInterface = bot.GetComponent<BattleBotInterface>();
                botName = bbInterface.name;
                
                if (bbInterface.health <= 0)
                {
                    Debug.Log(bbInterface.killer);
                    if (bbInterface.killer != null)
                    {
                        Debug.Log("Bot was killed by killer");
                        KilledBot(bot.name, bbInterface.killer);
                    }
                    else
                    {
                        Debug.Log("Bot was killed by Ring");
                        Died(bot.name);
                    }
                    
                }
            }
        }*/
    }

    private void BattleBotEvents_BotKilledBot(BattleBotInterface instigator, BattleBotInterface receiver)
    {
        KilledBot(receiver.name, instigator.name);
    }

    void KilledBot(string receiverName, string killerName)
    {
        canMove = true;
        text.text += "<color=green>" + "<b>" + killerName + "</b>" + "</color> "
            + killMoveMessages[Random.Range(0, killMoveMessages.Length)]
            + " <color=red>" + "<b>" + receiverName + "</b>" + "</color>";

        Invoke("ClearText", ttl);
    }
    void Died(string bot)
    {
        text.text += bot;
        Invoke("ClearText", ttl);
    }
    void ClearText()
    {
        text.text = " ";
        transform.position = originalPos;
        canMove = false;
    }

}
