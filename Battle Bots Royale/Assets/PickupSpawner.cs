using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public Rect spawnArea;
    public float spawnInterval = 5;

    public GameObject[] spawnablePickups;

    float elapsedTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsedTime >= spawnInterval)
        {
            elapsedTime = 0;
            StartCoroutine(SpawnPickup());
        }

        elapsedTime += Time.deltaTime;
    }

    private IEnumerator SpawnPickup()
    {
        while (true)
        {
            Vector3 spawnPoint = new Vector3(Random.Range(spawnArea.xMin, spawnArea.xMax), 1, Random.Range(spawnArea.yMin, spawnArea.yMax));
            if (!Physics.SphereCast(spawnPoint, 1, Vector3.up, out RaycastHit hit, 0, LayerMask.NameToLayer("Ground")))
            {
                Instantiate(spawnablePickups[Random.Range(0, spawnablePickups.Length)], spawnPoint, Quaternion.identity, GameObject.Find("Pickups").transform);
                break;
            }
        }
        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(new Vector3(spawnArea.center.x, 2.5f, spawnArea.center.y), new Vector3(spawnArea.size.x, 5, spawnArea.size.y));
    }
}
