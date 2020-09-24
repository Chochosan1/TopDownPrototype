using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Spawns a certain type of a villager. Attach to buildings like lumberyards, mines, etc.
/// </summary>

public enum BuildingType { Wood, Iron, Gold }
public class BuildingController : MonoBehaviour, ISpawnedAtWorld, ISelectable
{
    public BuildingType buildingType;
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

    public void SetBuildingLevel(int level)
    {
        currentBuildingLevel = level;
        Debug.Log("BUILDING LEVEL: " + currentBuildingLevel);
    }

    public void SpawnSpecificVillager(Vector3 position)
    {
        Instantiate(villagerToSpawn, position, villagerToSpawn.transform.rotation);
        Debug.Log("VILLAGER SPAWNED");
    }

    //used in ObjectSpawner.cs when retrieving the info for every spawned building in the list
    public BuildingControllerSerializable GetBuildingData(int buildingIndex)
    {
        BuildingControllerSerializable bcs = new BuildingControllerSerializable();
        bcs.currentBuildingLevel = currentBuildingLevel;
        Debug.Log(bcs.currentBuildingLevel);
        bcs.buildingType = buildingType;
        bcs.buildingIndex = buildingIndex;
        bcs.x = transform.position.x;
        bcs.y = transform.position.y;
        bcs.z = transform.position.z;
        bcs.rotX = transform.rotation.x;
        bcs.rotY = transform.rotation.y;
        bcs.rotZ = transform.rotation.z;
        bcs.rotW = transform.rotation.w;
        bcs.numberOfVillagersAssigned = assignedVillagersList.Count;
        bcs.villagerXpositions = new float[assignedVillagersList.Count];
        bcs.villagerYpositions = new float[assignedVillagersList.Count];
        bcs.villagerZpositions = new float[assignedVillagersList.Count];
        for(int i = 0; i < assignedVillagersList.Count; i++)
        {
            bcs.villagerXpositions[i] = assignedVillagersList[i].transform.position.x;
            bcs.villagerYpositions[i] = assignedVillagersList[i].transform.position.y;
            bcs.villagerZpositions[i] = assignedVillagersList[i].transform.position.z;
            Debug.Log("SAVING POSITIONS");
        }
        

        return bcs;
    }
}
