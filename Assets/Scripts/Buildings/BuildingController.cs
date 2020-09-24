using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns a certain type of a villager. Attach to buildings like lumberyards, mines, etc.
/// </summary>
public class BuildingController : MonoBehaviour, ISpawnedAtWorld, ISelectable
{
    [Tooltip("Set to true if the building can be upgraded. Will be used to toggle the upgrade UI.")]
    [SerializeField] private bool isUpgradable = true;
    [Tooltip("Reference to the ScriptableObject that holds the cost requirements.")]
    [SerializeField] SO_CostRequirements costRequirements;
    [SerializeField] private GameObject villagerToSpawn;
    [SerializeField] private GameObject spawnPoint;
    [Tooltip("Villager will spawn after that many seconds.")]
    [SerializeField] private float spawnFirstVillageAfterSeconds = 2f;
    [SerializeField] private int maxBuildingLevel = 3;
    private int currentBuildingLevel = 1;
    [Tooltip("All villagers that have been spawned by this building.")]
    [SerializeField] private List<AI_Villager> assignedVillagersList;

    public void StartInitialSetup()
    {
        StartCoroutine(SpawnVillagerAfterTime());
    }

    private IEnumerator SpawnVillagerAfterTime()
    {
        yield return new WaitForSeconds(spawnFirstVillageAfterSeconds);
        GameObject tempVillager = Instantiate(villagerToSpawn, spawnPoint.transform.position, villagerToSpawn.transform.rotation);
        AI_Villager tempAIVillager = tempVillager.GetComponent<AI_Villager>();
        tempAIVillager.SetHomeBuilding(this);
        assignedVillagersList.Add(tempAIVillager);
    }

    private void UpgradeBuildingLevel()
    {
        if(maxBuildingLevel > currentBuildingLevel && PlayerInventory.Instance.IsHaveEnoughResources(costRequirements))
        {
            currentBuildingLevel++;
            Chochosan.EventManager.Instance.OnBuildingUpgraded?.Invoke(costRequirements);
            StartCoroutine(SpawnVillagerAfterTime());
        }      
    }

    public void ForceSetAgentArea(Vector3 destination)
    {
        Debug.Log("THIS IS NOT AN AGENT");
    }

    public string GetSelectedUnitInfo()
    {
        return "Building level: " + currentBuildingLevel;
    }

    public bool IsOpenUpgradePanel()
    {
        return isUpgradable;
    }

    public void UpgradeUnit()
    {
        UpgradeBuildingLevel();
    }
}
