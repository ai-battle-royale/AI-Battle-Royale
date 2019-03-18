using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RingState
{
    public float radius;
    public float damage;
    public float time;
    public float moveTime;
}

public class RingManager : MonoBehaviour
{
    public static RingManager instance;

    public GameObject ring;
    public RingState[] ringStates;
    public int currentRingStateIndex;
    public Vector3 nextLocation;
    public RingState currentRingState;
    public RingState nextRingState;

    private BattleBotInterface[] bots;

    void Awake()
    {
        var managers = FindObjectsOfType<RingManager>();

        if (managers.Length > 1)
        {
            Debug.LogError("Too many GameManager components present in scene.");
        }
        else
        {
            instance = this;
        }
    }

    private void OnValidate()
    {
        ring.transform.localScale = new Vector3(ringStates[0].radius * 2, 10000, ringStates[0].radius * 2);
    }

    void Start()
    {
        StartCoroutine(RingMovement());
        StartCoroutine(DoDamage());
    }

    private void Update()
    {
        bots = FindObjectsOfType<BattleBotInterface>();
    }

    IEnumerator DoDamage ()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);

            foreach (var bot in bots)
            {
                var distance = Vector3.Distance(bot.transform.position, ring.transform.position);

                if (distance > currentRingState.radius)
                    bot.TakeDamage(currentRingState.damage);
            }
        }
    }

    IEnumerator RingMovement ()
    {
        currentRingState = ringStates[currentRingStateIndex];

        print($"Ring closing in {currentRingState.time} seconds! Radius {currentRingState.radius}");

        if (currentRingStateIndex < ringStates.Length - 1)
        {
            nextRingState = ringStates[currentRingStateIndex + 1];
        }
        else
        {
            nextRingState = new RingState
            {
                radius = 1,
                damage = currentRingState.damage,
                moveTime = 10
            };
        }

        ring.transform.localScale = new Vector3(currentRingState.radius * 2, 10000, currentRingState.radius * 2);

        var r = Random.Range(0, Mathf.PI * 2);
        var locationOffset = new Vector3(Mathf.Sin(r), 0, Mathf.Cos(r));

        nextLocation = ring.transform.position + locationOffset * (currentRingState.radius - nextRingState.radius);

        var currentLocation = ring.transform.position;

        yield return new WaitForSeconds(currentRingState.time);

        print("Moving ring!");

        var i = 0f;

        while (i < 1)
        {
            i += Time.deltaTime / currentRingState.moveTime;

            var radius = currentRingState.radius * (1 - i) + nextRingState.radius * i;

            ring.transform.localScale = new Vector3(radius * 2, 10000, radius * 2);
            ring.transform.position = Vector3.Lerp(currentLocation, nextLocation,i);

            yield return new WaitForEndOfFrame();
        }

        currentRingStateIndex++;

        if (currentRingStateIndex < ringStates.Length) {
            StartCoroutine(RingMovement());
        }
    }
}
