using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to buildings that must support saving. Can spawn a certain type of a villager if not left null. Attach to buildings like lumberyards, mines, etc.
/// </summary>

public class BuildingController : MonoBehaviour, ISpawnedAtWorld, ISelectable, IDamageable
{
    private int buildingIndex;
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
    [Header("Stats")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float buildingMaxHP = 100;
    private float buildingCurrentHP;
    [Tooltip("All villagers that have been spawned by this building.")]
    [SerializeField] private List<AI_Villager> assignedVillagersList;

    private void Start()
    {
       // assignedVillagersList = new List<AI_Villager>();
        if (!Chochosan.SaveLoadManager.IsSaveExists())
        {
            SetInitialHP();
        }
    }

    public void StartInitialSetup()
    {
        if(villagerToSpawn != null)
        {
            StartCoroutine(SpawnVillagerAfterTime());
        }    
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

    public void SetInitialHP()
    {
        buildingCurrentHP = buildingMaxHP;
    }

    public void SetBuildingHP(float hp)
    {
        buildingCurrentHP = hp > buildingMaxHP ? buildingMaxHP : hp;
    }

    public void TakeDamage(float damage)
    {
        buildingCurrentHP -= damage;

        //send a message that a displayable UI value has been changed (in this case the HP of the building)
        Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);

        if(buildingCurrentHP <= 0)
        {
            DestroyBuilding();
        }
    }

    public void DestroyBuilding()
    {
        ObjectSpawner.Instance.RemoveBuildingFromList(gameObject);
        Destroy(gameObject);
    }

    public void ForceSetAgentArea(Vector3 destination)
    {
        Debug.Log("THIS IS NOT AN AGENT");
    }

    public float GetCustomAgentStoppingDistance()
    {
        return customAgentStoppingDistance;
    }

    public string GetSelectedUnitInfo()
    {
        string info = $"Building level: {currentBuildingLevel}\nHP: {buildingCurrentHP}/{buildingMaxHP}";
        return info;
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
    }

    public void SetBuildingIndex(int index)
    {
        buildingIndex = index;
    }

    public void SpawnSpecificVillager(Vector3 position)
    {
        if (villagerToSpawn != null)
        {
            GameObject tempVillager = Instantiate(villagerToSpawn, position, villagerToSpawn.transform.rotation);

            //very important to assign the villager to the building after spawning (useful for loading/saving data later on because the list must not be empty)
            assignedVillagersList.Add(tempVillager.GetComponent<AI_Villager>());
            Debug.Log("LOADED AND ADDED TO LIST ONE VILLAGER");
        }
    }

    #region DataSaving
    //used in ObjectSpawner.cs when retrieving the info for every spawned building in the list
    public BuildingControllerSerializable GetBuildingData()
    {
        //create a serializable copy
        BuildingControllerSerializable bcs = new BuildingControllerSerializable();

        //save specific stats that must be loaded later on
        bcs.currentBuildingLevel = currentBuildingLevel;
        bcs.buildingCurrentHP = buildingCurrentHP;
        bcs.buildingIndex = buildingIndex;
        bcs.x = transform.position.x;
        bcs.y = transform.position.y;
        bcs.z = transform.position.z;
        bcs.rotX = transform.rotation.x;
        bcs.rotY = transform.rotation.y;
        bcs.rotZ = transform.rotation.z;
        bcs.rotW = transform.rotation.w;

        //this many villagers will spawn later on when loading data
        bcs.numberOfVillagersAssigned = assignedVillagersList.Count;

        //initialize the arrays from the serializable copy
        bcs.villagerXpositions = new float[assignedVillagersList.Count];
        bcs.villagerYpositions = new float[assignedVillagersList.Count];
        bcs.villagerZpositions = new float[assignedVillagersList.Count];

        //store each villager's X, Y, Z positions in arrays
        for (int i = 0; i < assignedVillagersList.Count; i++)
        {
            bcs.villagerXpositions[i] = assignedVillagersList[i].transform.position.x;
            bcs.villagerYpositions[i] = assignedVillagersList[i].transform.position.y;
            bcs.villagerZpositions[i] = assignedVillagersList[i].transform.position.z;
            Debug.Log("SAVED ONE VILLAGER");
        }
        return bcs;
    }
    #endregion
}
