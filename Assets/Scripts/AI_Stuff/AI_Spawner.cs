using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Spawner : MonoBehaviour
{
    [SerializeField] private int spawnEnemiesEveryXDays = 2;
    [SerializeField] private GameObject aiToSpawnPrefab;
    [SerializeField] private float spawnCooldown;
    private GameObject currentTarget;
    private float spawnTimestamp;
    private bool isNeutralized = false;

    private void Start()
    {
        Progress_Manager.Instance.CurrentSpawnersToNeutralize++;
    }

    void Update()
    {
        if (!isNeutralized && PlayerInventory.Instance.CurrentDay % spawnEnemiesEveryXDays == 0)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (Time.time >= spawnTimestamp && Progress_Manager.Instance.GetTownHall() != null)
        {
            GameObject tempEnemy = Instantiate(aiToSpawnPrefab, transform.position, aiToSpawnPrefab.transform.rotation);
            AI_Attacker tempController = tempEnemy.GetComponent<AI_Attacker>();
            tempController.SetInitialStateNotLoadedFromSave();
            currentTarget = Progress_Manager.Instance.GetTownHall().gameObject;
            if (currentTarget != null)
                tempController.SetDefaultTarget(currentTarget);
               
            spawnTimestamp = Time.time + spawnCooldown;
        }
    }

    public void MarkSpawnerAsNeutralized()
    {
        isNeutralized = true;
        Progress_Manager.Instance.CurrentSpawnersToNeutralize--;
    }
}
