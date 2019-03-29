using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBotEvents : MonoBehaviour
{
    public static event Action<BattleBotInterface, BattleBotInterface, float> BotDealtDamageToBot;
    public static event Action<BattleBotInterface, BattleBotInterface> BotKilledBot;


    public static void OnBotDealtDamageToBot(BattleBotInterface instigator, BattleBotInterface receiver, float damageAmount)
    {
        if (BotDealtDamageToBot != null) BotDealtDamageToBot.Invoke(instigator, receiver, damageAmount);
    }

    public static void OnBotKilledBot(BattleBotInterface instigator, BattleBotInterface receiver)
    {
        if (BotKilledBot != null) BotKilledBot.Invoke(instigator, receiver);
    }


}
