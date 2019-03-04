using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SwitchTarget : MonoBehaviour
{
    List<GameObject> bots;
    Cinemachine.CinemachineVirtualCameraBase cvcb;

    int currentTarget;
    // Start is called before the first frame update
    void Start()
    {
        bots = new List<GameObject>(GameObject.FindGameObjectsWithTag("Bot"));
        cvcb = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();

        currentTarget = bots.FindIndex(x => x.transform == cvcb.LookAt);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentTarget = (currentTarget + 1) % bots.Count;
            cvcb.LookAt = bots[currentTarget].transform;

        }
    }
}
