using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public Rect spawnArea;

    public bool spawnAtStart = true;
    public int itemAmount = 50;
    public int weaponAmount = 6;

    public bool spawnDuringGameplay = false;
    public float spawnInterval = 5;

    public GameObject[] spawnableItemPickups;
    public GameObject[] spawnableWeaponPickups;
    public GameObject[] playerPrefabs; // we wanted random spawn points and the pickup spawner fits the solution perfectly

    float elapsedTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Spawn specified amount of items at start
        if (spawnAtStart)
        {
            for (int i = 0; i < itemAmount; i++)
            {
                StartCoroutine(SpawnPickup(spawnableItemPickups));
            }
            for (int i = 0; i < weaponAmount; i++)
            {
                StartCoroutine(SpawnPickup(spawnableWeaponPickups));
            }

            for (int i = 0; i < playerPrefabs.Length; i++)
            {
                Spawn(playerPrefabs[i], true);
            }
        }

        // Start spawning coroutine to spawn more itmes during gameplay
        if (spawnDuringGameplay)
            StartCoroutine(ItemSpawner());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator ItemSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            StartCoroutine(SpawnPickup(spawnableItemPickups));
        }
    }

    private IEnumerator SpawnPickup(GameObject[] spawnpool)
    {
        Spawn(spawnpool[Random.Range(0, spawnpool.Length)]);
        yield return null;
    }

    private void Spawn(GameObject prefab, bool moveInstead = false)
    {
        while (true)
        {
            Vector3 spawnPoint = new Vector3(Random.Range(spawnArea.xMin, spawnArea.xMax), 0, Random.Range(spawnArea.yMin, spawnArea.yMax));
            if (prefab != null && !Physics.SphereCast(spawnPoint, 1, Vector3.up, out RaycastHit hit, 0, LayerMask.NameToLayer("Ground")))
            {
                if (!moveInstead)
                {
                    Instantiate(prefab, spawnPoint, Quaternion.identity, transform);
                }
                else
                {
                    prefab.transform.position = spawnPoint;
                }
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(new Vector3(spawnArea.center.x, 2.5f, spawnArea.center.y), new Vector3(spawnArea.size.x, 5, spawnArea.size.y));
    }
}
