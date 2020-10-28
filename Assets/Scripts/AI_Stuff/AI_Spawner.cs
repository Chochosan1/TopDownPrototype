using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Spawner : MonoBehaviour
{
    //debug
    public bool isSpawning = true;

    [Header("Prefabs")]
    [SerializeField] private GameObject aiToSpawnPrefab;
    [Tooltip("This particle will be active only if the spawner has not been neutralized.")]
    [SerializeField] private GameObject isActiveParticle;
    [Tooltip("This particle will activate as a feedback for when the spawner gets neutralized.")]
    [SerializeField] private GameObject neutralizedFeedbackParticle;

    [Header("Properties")]
    [SerializeField] private float spawnCooldown;
    [Tooltip("The spawner will spawn enemies only every X days.")]
    [SerializeField] private int spawnEnemiesEveryXDays = 2;

    private GameObject currentTarget;
    private float spawnTimestamp;
    private bool isNeutralized = false;

    private void Start()
    {
        Progress_Manager.Instance.CurrentSpawnersToNeutralize++;
    }

    private void Update()
    {
        if (!isSpawning)
            return;
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

    public void MarkSpawnerAsNeutralized(bool value)
    {
        isNeutralized = value;
        isActiveParticle.SetActive(!value);
        neutralizedFeedbackParticle.SetActive(true);
        Progress_Manager.Instance.CurrentSpawnersToNeutralize--;
    }
}
