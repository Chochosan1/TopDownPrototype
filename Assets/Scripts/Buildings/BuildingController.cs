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
    [SerializeField] private string buildingName;
    [SerializeField] private UpgradeToUnlock upgradeToUnlock;
    [SerializeField] private Buildings buildingType;
    //  [SerializeField] private string[] buildingRequirementsBeforeBuilding;
    [Tooltip("Reference to the ScriptableObject that holds the cost requirements.")]
    [SerializeField] SO_CostRequirements costRequirements;
    [Tooltip("The part of the prefab that holds the finally built building.")]
    [SerializeField] private GameObject mainBuilding;
    [Tooltip("The part of the prefab that holds the part of the building that should be targettable by builders.")]
    [SerializeField] private GameObject buildingInProgress;
    [SerializeField] private GameObject villagerToSpawn;
    [SerializeField] private GameObject spawnPoint;
    [Tooltip("Villager will spawn after that many seconds.")]
    [SerializeField] private float spawnFirstVillageAfterSeconds = 2f;
    [SerializeField] private int maxBuildingLevel = 3;
    //default starting level
    private int currentBuildingLevel = 1; 
    [Header("Stats")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float buildingMaxHP = 100;
    [SerializeField] private int housingSpace;
    [SerializeField] private float charismaOnBuilt = 5f;
    [Tooltip("After the building is built, it will consume that many wood for its upkeep.")]
    [SerializeField] private float woodUpkeepAfterBuilding = 0f;
    public float WoodUpkeep
    {
        get
        {
            return buildingProgress >= 100 ? woodUpkeepAfterBuilding : 0;
        }

        private set { }
    }

    //initial HP before being finally built
    private float buildingCurrentHP = 1; 
    private bool isBuildingComplete; 
    [Tooltip("The current building progress. Set to 100 if the building must be built as a part of the level instead of requiring player input. ")]
    [SerializeField] private float buildingProgress;
    [Tooltip("How much progress should be added to the building per single progress iteration (e.g when the builder swings with his hammer although progress adding will not always match with the animation)")]
    [SerializeField] private float progressPerBuildIteration = 25f;

    //[Tooltip("All villagers that have been spawned by this building.")]
    //[SerializeField] private List<AI_Villager> assignedVillagersList;

    private void Start()
    {
        CheckBuildingStatus();
    }

    private void CheckBuildingStatus()
    {
        if (buildingProgress < 100)
        {
            isBuildingComplete = false;
            mainBuilding.SetActive(false);
        //    GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            FinishBuildingAndSpawn();
        }
    }

    public void FinishBuildingAndSpawn()
    {
        mainBuilding.SetActive(true);
        Destroy(buildingInProgress);       
        buildingProgress = 100;
     //   GetComponent<BoxCollider>().enabled = true;
        UnlockUpgradeWhenBuilt();
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally?.Invoke(this, buildingType);

        //isBuildingComplete becomes true the moment the building is done; if saved and then loaded this block will not execute again
        if (!isBuildingComplete) 
        {
            StartInitialSetup();        
            isBuildingComplete = true;
        }
    }

    public void AddBuildProgress()
    {
        if (!isBuildingComplete)
        {
            buildingProgress += progressPerBuildIteration;
            CheckBuildingStatus();
        }
    }

    public void SetIsBuildingComplete(bool value)
    {
        isBuildingComplete = value;
    }

    public bool GetIsBuildingComplete()
    {
        return isBuildingComplete;
    }

    //called when the building is first instantiated by the player(not when loading data)
    public void StartInitialSetup()
    {      
        SetInitialHP();
        PlayerInventory.Instance.MaxPopulation += housingSpace;
        PlayerInventory.Instance.CurrentVillageCharisma += charismaOnBuilt;
        
        //if (villagerToSpawn != null)
        //{           
        //    StartCoroutine(SpawnVillagerAfterTime());
        //}
    }

    private void UnlockUpgradeWhenBuilt()
    {
        switch (upgradeToUnlock)
        {
            case UpgradeToUnlock.WoodHarvesting:
                Progress_Manager.Instance.EnableSpecificHarvesting(UpgradeToUnlock.WoodHarvesting);
                break;
            case UpgradeToUnlock.GoldHarvesting:
                Progress_Manager.Instance.EnableSpecificHarvesting(UpgradeToUnlock.GoldHarvesting);
                break;
            case UpgradeToUnlock.IronHarvesting:
                Progress_Manager.Instance.EnableSpecificHarvesting(UpgradeToUnlock.IronHarvesting);
                break;
            case UpgradeToUnlock.FoodHarvesting:
                Progress_Manager.Instance.EnableSpecificHarvesting(UpgradeToUnlock.FoodHarvesting);
                break;
        }
    }

    public IEnumerator SpawnVillagerAfterTime()
    {
        yield return new WaitForSeconds(spawnFirstVillageAfterSeconds);
        GameObject tempVillager = Instantiate(villagerToSpawn, spawnPoint.transform.position, villagerToSpawn.transform.rotation);
        AI_Villager tempAIVillager = tempVillager.GetComponent<AI_Villager>();
        tempAIVillager.SetHomeBuilding(this);
        Unit_Controller.Instance.AddVillagerToList(tempAIVillager);
      //  assignedVillagersList.Add(tempAIVillager);
    }

    private void UpgradeBuildingLevel()
    {
        if (maxBuildingLevel > currentBuildingLevel && PlayerInventory.Instance.IsHaveEnoughResources(costRequirements))
        {
            currentBuildingLevel++;
            Chochosan.EventManager.Instance.OnBuildingUpgraded?.Invoke(costRequirements);
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);
           // StartCoroutine(SpawnVillagerAfterTime());
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

    public void SetBuildingProgress(float buildingProgress)
    {
        this.buildingProgress = buildingProgress;
    }

    public void TakeDamage(float damage)
    {
        buildingCurrentHP -= damage;

        //send a message that a displayable UI value has been changed (in this case the HP of the building)
        Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);

        if (buildingCurrentHP <= 0)
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

    public void ForceSetSpecificTarget(GameObject target)
    {
        Debug.Log("THIS IS NOT AN AGENT X2");
    }

    public float GetCustomAgentStoppingDistance()
    {
        return customAgentStoppingDistance;
    }

    public string GetSelectedUnitInfo()
    {
        string info = $"{buildingName}\nBuilding level: {currentBuildingLevel}\nHP: {buildingCurrentHP}/{buildingMaxHP}";
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

    //public void SpawnSpecificVillager(Vector3 position, string villagerType)
    //{
    //    if (villagerToSpawn != null)
    //    {
    //        GameObject tempVillagerGameobject = Instantiate(villagerToSpawn, position, villagerToSpawn.transform.rotation);
    //        AI_Villager tempVillagerController = tempVillagerGameobject.GetComponent<AI_Villager>();
    //        tempVillagerController.SwitchVillagerType(villagerType);
    //        //very important to assign the villager to the building after spawning (useful for loading/saving data later on because the list must not be empty)
    //        //   assignedVillagersList.Add(tempVillagerGameobject.GetComponent<AI_Villager>());
    //        Unit_Controller.Instance.AddVillagerToList(tempVillagerController);
    //        Debug.Log("LOADED AND ADDED TO LIST ONE VILLAGER");
    //    }
    //}

    #region DataSaving
    //used in ObjectSpawner.cs when retrieving the info for every spawned building in the list
    public BuildingControllerSerializable GetBuildingData()
    {
        //create a serializable copy
        BuildingControllerSerializable bcs = new BuildingControllerSerializable();

        //save specific stats that must be loaded later on
        bcs.isBuildingComplete = isBuildingComplete;
        bcs.buildingProgress = buildingProgress;
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

        ////this many villagers will spawn later on when loading data
        //bcs.numberOfVillagersAssigned = assignedVillagersList.Count;

        ////initialize the arrays from the serializable copy
        //bcs.villagerXpositions = new float[assignedVillagersList.Count];
        //bcs.villagerYpositions = new float[assignedVillagersList.Count];
        //bcs.villagerZpositions = new float[assignedVillagersList.Count];
        //bcs.villagerTypeStrings = new string[assignedVillagersList.Count];

        ////store each villager's X, Y, Z positions in arrays
        //for (int i = 0; i < assignedVillagersList.Count; i++)
        //{
        //    bcs.villagerXpositions[i] = assignedVillagersList[i].transform.position.x;
        //    bcs.villagerYpositions[i] = assignedVillagersList[i].transform.position.y;
        //    bcs.villagerZpositions[i] = assignedVillagersList[i].transform.position.z;
        //    switch (assignedVillagersList[i].GetCurrentVillagerType())
        //    {
        //        case Villager_Type.WoodWorker:
        //            bcs.villagerTypeStrings[i] = "Wood";
        //            break;
        //        case Villager_Type.GoldWorker:
        //            bcs.villagerTypeStrings[i] = "Gold";
        //            break;
        //        case Villager_Type.IronWorker:
        //            bcs.villagerTypeStrings[i] = "Iron";
        //            break;
        //        case Villager_Type.Builder:
        //            bcs.villagerTypeStrings[i] = "Builder";
        //            break;
        //    }
        //    Debug.Log("SAVED ONE VILLAGER");
       // }
        return bcs;
    }
    #endregion
}
