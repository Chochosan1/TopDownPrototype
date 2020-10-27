using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to buildings that must support saving. Can spawn a certain type of a villager if not left null. Attach to buildings like lumberyards, mines, etc.
/// </summary>

public class BuildingController : MonoBehaviour, ISpawnedAtWorld, ISelectable, IDamageable
{
    private int buildingIndex;
    private bool isSelected = false;
    [Tooltip("Set to true if the building can be upgraded. Will be used to toggle the upgrade UI.")]
    [SerializeField] private bool isUpgradable = true;
    [SerializeField] private string buildingName;
    [SerializeField] private UpgradeToUnlock upgradeToUnlock;
    [SerializeField] private Buildings buildingType;
    //  [SerializeField] private string[] buildingRequirementsBeforeBuilding;
    [Header("Prefabs")]
    [Tooltip("Reference to the ScriptableObject that holds the cost requirements.")]
    [SerializeField] SO_CostRequirements costRequirements;
    [Tooltip("The part of the prefab that holds the finally built building.")]
    [SerializeField] private GameObject mainBuilding;
    [Tooltip("The part of the prefab that holds the part of the building that should be targettable by builders.")]
    [SerializeField] private GameObject buildingInProgress;
    [SerializeField] private GameObject buildingDoneParticle;
    [SerializeField] private GameObject villagerToSpawn;
    [SerializeField] private GameObject[] attackersToSpawn;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject[] attackPoints;
    private int[] attackPointsStates;
    [Tooltip("Villager will spawn after that many seconds.")]
    [SerializeField] private float spawnFirstVillageAfterSeconds = 2f;
    [SerializeField] private int maxBuildingLevel = 3;
    [SerializeField] private GameObject selectedIndicator;
    //default starting level
    private int currentBuildingLevel = 1;
    [Header("Stats")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float buildingMaxHP = 100;
    [SerializeField] private int housingSpace;
    [SerializeField] private float charismaOnBuilt = 5f;
    [Tooltip("After the building is built, it will consume that many wood per day for its upkeep.")]
    [SerializeField] private float woodUpkeepAfterBuilding = 0f;
    [Tooltip("After the building is built, it will autogenerate that much food per day.")]
    [SerializeField] private float foodGeneration = 0f;
    private Camera mainCamera;
    public float WoodUpkeep
    {
        get
        {
            return buildingProgress >= 100 ? woodUpkeepAfterBuilding : 0;
        }

        private set { }
    }

    public float FoodGeneration
    {
        get
        {
            return foodGeneration;
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

    private void Start()
    {
        mainCamera = Camera.main;
        CheckBuildingStatus();
        attackPointsStates = new int[attackPoints.Length];
    }

    private void CheckBuildingStatus()
    {
        if (buildingProgress < 100)
        {
            isBuildingComplete = false;
            mainBuilding.SetActive(false);
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

    public Buildings GetBuildingType()
    {
        return buildingType;
    }

    //called when the building is first instantiated by the player(not when loading data)
    public void StartInitialSetup()
    {
        SetInitialHP();
        PlayerInventory.Instance.MaxPopulation += housingSpace;
        PlayerInventory.Instance.CurrentVillageCharisma += charismaOnBuilt;
        PlayerInventory.Instance.AddBuildingsProgress(this, buildingType);
        Instantiate(buildingDoneParticle, transform.position + new Vector3(0, 2f, 0), buildingDoneParticle.transform.rotation);
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

    public void SpawnAttackerUnit(int attackerIndex)
    {
        if (attackersToSpawn == null || !PlayerInventory.Instance.IsHaveEnoughHousingSpace())
            return;
        GameObject tempUnit = Instantiate(attackersToSpawn[attackerIndex], spawnPoint.transform.position, attackersToSpawn[attackerIndex].transform.rotation);
        tempUnit.GetComponent<AI_Attacker>().SetInitialStateNotLoadedFromSave();
      //  AI_Attacker_Loader.AddAttackerToList(tempUnit.GetComponent<AI_Attacker>());
    }

    public IEnumerator SpawnVillagerAfterTime()
    {
        yield return new WaitForSeconds(spawnFirstVillageAfterSeconds);
        GameObject tempVillager = Instantiate(villagerToSpawn, spawnPoint.transform.position, villagerToSpawn.transform.rotation);
        AI_Villager tempAIVillager = tempVillager.GetComponent<AI_Villager>();
        tempAIVillager.SetInitialHP();
        Unit_Controller.Instance.AddVillagerToList(tempAIVillager);
    }

    public void UpgradeBuildingLevel()
    {
        if (maxBuildingLevel > currentBuildingLevel && PlayerInventory.Instance.IsHaveEnoughResources(costRequirements))
        {
            currentBuildingLevel++;
            Chochosan.EventManager.Instance.OnBuildingUpgraded?.Invoke(costRequirements);
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);
        }
    }

    public float GetMaxHP()
    {
        return buildingMaxHP;
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

    public void TakeDamage(float damage, AI_Attacker attacker)
    {
        buildingCurrentHP -= damage;

        if(isSelected)
        {
            //send a message that a displayable UI value has been changed (in this case the HP of the building)
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);
        }

        if (buildingCurrentHP <= 0)
        {
            DestroyBuilding();
        }
    }

    public void DestroyBuilding()
    {
        ObjectSpawner.Instance.RemoveBuildingFromList(gameObject);
        if(buildingType == Buildings.TownHall)
        {
            Chochosan.UI_Manager.Instance.DisplayWarningMessage("GAME LOST!!!");
        }
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

    public Vector3 GetFirstFreeSpot()
    {
        Vector3 des = attackPoints[0].transform.position;
        for(int i = 0; i < attackPointsStates.Length; i++)
        {
            if(attackPointsStates[i] != 1)
            {
                attackPointsStates[i] = 1;
                return attackPoints[i].transform.position;
            }
        }
        return des;
    }

    public Vector3 GetRandomAttackSpot()
    {
        int randomNum = Random.Range(0, attackPoints.Length);
        return attackPoints[randomNum].transform.position;
    }

    public float GetCustomAgentStoppingDistance()
    {
        return customAgentStoppingDistance;
    }

    public string GetSelectedUnitInfo()
    {
       // string info = $"{buildingName}\nBuilding level: {currentBuildingLevel}\nHP: {buildingCurrentHP}/{buildingMaxHP}";
        string info = $"Level: {currentBuildingLevel}\nHP: {buildingCurrentHP}/{buildingMaxHP}";
        return info;
    }

    public void UpgradeUnit()
    {
        UpgradeBuildingLevel();
    }

    public void SetBuildingLevel(int level)
    {
        currentBuildingLevel = level;
    }


    public int GetBuildingLevel()
    {
        return currentBuildingLevel;
    }

    public void SetBuildingIndex(int index)
    {
        buildingIndex = index;
    }

    public void CheckIfSelectedBySelector()
    {
        //if (renderer.isVisible)
        //{
        //    Vector3 camPos = mainCamera.WorldToScreenPoint(transform.position);
        //    camPos.y = Unit_Controller.Instance.InvertMouseY(camPos.y);
        //    if (Unit_Controller.Instance.selectRect.Contains(camPos))
        //    {
        //        Unit_Controller.Instance.AddUnitToSelectedList(this.gameObject);
        //        selectedIndicator.SetActive(true);
        //    }
        //    else
        //    {
        //        selectedIndicator.SetActive(false);
        //    }
        //}
    }

    public void ToggleSelectedIndicator(bool value)
    {
        if(selectedIndicator != null)
        {
            selectedIndicator.SetActive(value);
            isSelected = value;
        }         
    }

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
