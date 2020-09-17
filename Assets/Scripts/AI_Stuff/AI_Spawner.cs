using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject aiToSpawnPrefab;
    [SerializeField] private float spawnCooldown;
    private float spawnTimestamp;

    // Update is called once per frame
    void Update()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (Time.time >= spawnTimestamp)
        {
            Instantiate(aiToSpawnPrefab, transform.position, aiToSpawnPrefab.transform.rotation);
            spawnTimestamp = Time.time + spawnCooldown;
        }
    }
}
