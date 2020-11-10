using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles the logic for the unit spawners.
/// </summary>
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
    [SerializeField] private float spawnCooldownMin;
    [SerializeField] private float spawnCooldownMax;
    [Tooltip("The spawner will spawn enemies only every X days.")]
    [SerializeField] private int spawnEnemiesEveryXDays = 2;
    [SerializeField] private int maxEnemiesToSpawnInADay = 10;
    private int enemiesCurrentlySpawned = 0;
    private float currentSpawnCooldown;

    private GameObject currentTarget;
    private float spawnTimestamp;
    private bool isNeutralized = false;

    private void Awake()
    {
        Chochosan.EventManager.Instance.OnNewDay += ResetEnemiesCurrentlySpawned;
    }

    private void OnDestroy()
    {
        Chochosan.EventManager.Instance.OnNewDay -= ResetEnemiesCurrentlySpawned;
    }

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
            if (enemiesCurrentlySpawned >= maxEnemiesToSpawnInADay)
                return;

            enemiesCurrentlySpawned++;
            GameObject tempEnemy = Instantiate(aiToSpawnPrefab, transform.position, aiToSpawnPrefab.transform.rotation);
            AI_Attacker tempController = tempEnemy.GetComponent<AI_Attacker>();
            tempController.SetInitialStateNotLoadedFromSave();
            currentTarget = Progress_Manager.Instance.GetTownHall().gameObject;
            if (currentTarget != null)
                tempController.SetDefaultTarget(currentTarget);

            currentSpawnCooldown = Random.Range(spawnCooldownMin, spawnCooldownMax);
            spawnTimestamp = Time.time + currentSpawnCooldown;
        }
    }


    public void MarkSpawnerAsNeutralized(bool value)
    {
        isNeutralized = value;
        isActiveParticle.SetActive(!value);
        neutralizedFeedbackParticle.SetActive(true);
        Progress_Manager.Instance.CurrentSpawnersToNeutralize--;
    }

    private void ResetEnemiesCurrentlySpawned()
    {
        enemiesCurrentlySpawned = 0;
    }
}
