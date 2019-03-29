using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotKillSound : MonoBehaviour {
    public AudioSource audioSource;
    public AudioSource audioSourceMusic;

    void OnEnable() {
        BattleBotEvents.BotKilledBot += PlaySound;
    }

    void OnDisabled () {
        BattleBotEvents.BotKilledBot -= PlaySound;
    }

    private void PlaySound(BattleBotInterface arg1, BattleBotInterface arg2) {
        audioSource.Play();
    }

    void Update () {
        audioSourceMusic.pitch = Time.timeScale;
    }
}
