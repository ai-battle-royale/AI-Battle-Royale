using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScript : MonoBehaviour
{

    Text text;
    GameObject[] bots;
    public GameObject victoryText;
    public GameObject killText;
    BattleBotInterface manager;
    
    


    // Start is called before the first frame update
    void Start()
    {
        text = killText.GetComponent<Text>();
        victoryText.SetActive(false);
        bots = GameObject.FindGameObjectsWithTag("Bot");
    }

    // Update is called once per frame
    void Update()
    {

        bots = GameObject.FindGameObjectsWithTag("Bot");
        if (bots.Length == 1)
        {
            victoryText.SetActive(true);
            //victoryText.GetComponent<Renderer>().material.
        }
        Debug.Log(bots.Length);
        foreach (GameObject bot in bots)
        {
            manager = bot.GetComponent<BattleBotInterface>();
            if (manager.health == 0)
            {
                text.text = "LOL";
            }
        }
    }
}
