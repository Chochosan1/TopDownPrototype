using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns a certain type of a villager. Attach to buildings like lumberyards, mines, etc.
/// </summary>
public class VillagerSpawner : MonoBehaviour, ISpawnedAtWorld
{
    [SerializeField] private GameObject villagerToSpawn;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float spawnFirstVillageAfterSeconds = 2f;

    public void StartInitialSetup()
    {
        StartCoroutine(SpawnVillager());
    }

    private IEnumerator SpawnVillager()
    {
        yield return new WaitForSeconds(spawnFirstVillageAfterSeconds);
        Instantiate(villagerToSpawn, spawnPoint.transform.position, villagerToSpawn.transform.rotation);
    }
}
