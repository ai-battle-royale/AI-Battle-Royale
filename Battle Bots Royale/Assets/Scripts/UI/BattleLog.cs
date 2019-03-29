using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BattleLog : MonoBehaviour
{
    BattleBotInterface bbInterface;
    GameObject[] bots;
    Text text;
    public float moveAmount;
    public float moveSpeed;
    Vector3 moveDirection;
    bool canMove;
    string botName;
    Vector3 originalPos;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = gameObject.transform.position;

        moveDirection = Vector3.up;

        text = GetComponent<Text>();

        bots = GameObject.FindGameObjectsWithTag("Bot");
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirection, moveAmount * (moveSpeed * Time.deltaTime));
        }

        foreach (GameObject bot in bots)
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
        }
    }

    void KilledBot(string textString, string killer)
    {
        canMove = true;
        text.text += "<color=green>" + "<b>" + textString + "</b>" + "</color>" +" was killed by " + "<color=red>" + "<b>" + killer + "</b>" + "</color>";
        Invoke("ClearText", 2f);
    }
    void Died(string bot)
    {
        text.text += bot;
        Invoke("ClearText", 2f);
    }
    void ClearText()
    {
        text.text = " ";
        transform.position = originalPos;
        canMove = false;
    }

}
