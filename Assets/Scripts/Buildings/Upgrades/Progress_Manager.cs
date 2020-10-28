using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Calculates different stats all the time and unlocks progress.
/// </summary>
public enum UpgradeToUnlock { None, WoodHarvesting, GoldHarvesting, IronHarvesting, FoodHarvesting }
public enum Buildings { None, TownHall, Woodcamp, Ironmine, Goldmine, House, Turret, Mill, Warehouse, Barracks, Wizardry, Neutralizer }
public class Progress_Manager : MonoBehaviour
{
    public static Progress_Manager Instance;

    [Header("General Game Settings")]
    [Tooltip("How often should values in the game update? Measured in seconds.")]
    [SerializeField] private float gameTickCooldown = 1f;
    private float gameTickTimestamp;
    [Tooltip("How often should a day last? Measured in seconds.")]
    [SerializeField] private float dayDurationInSeconds = 60f;
    private float dayTimestamp;

    //values in the game are set as per daily basis (e.g costs 20 wood upkeep per day) but the values themself are updated 
    //much more frequently than that. This requires a displayValueReducer to turn the perDayValues into values that can be updated more often
    private float updateValueReducer;

    private BuildingController townHallController;
    private BuildingController currentBuildingToDamageIfNoWood;

    private bool isWorkersCanHarvestWood;
    private bool isWorkersCanHarvestGold;
    private bool isWorkersCanHarvestIron;
    private bool isWorkersCanHarvestFood;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        updateValueReducer = dayDurationInSeconds / gameTickCooldown;
    }

    private void Start()
    {
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            CurrentDayTimestamp = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentDayTimestamp;
        }
        else
        {
            CurrentDayTimestamp = dayDurationInSeconds;
        }
        gameTickTimestamp = gameTickCooldown;
        Debug.Log(CurrentDayTimestamp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayerInventory.Instance.CurrentWood += 50f;
            PlayerInventory.Instance.CurrentGold += 50f;
            PlayerInventory.Instance.CurrentIron += 50f;
            PlayerInventory.Instance.CurrentFood += 50f;
        }
        if (Time.time >= gameTickTimestamp)
        {
            if (townHallController != null)
                PlayerInventory.Instance.CurrentVillageCharisma += 33f / updateValueReducer;
            PlayerInventory.Instance.CurrentWood -= PlayerInventory.Instance.CurrentWoodUpkeep / updateValueReducer;
            PlayerInventory.Instance.CurrentFood += (PlayerInventory.Instance.CurrentAutoFoodGeneration - PlayerInventory.Instance.CurrentFoodConsumption) / updateValueReducer;

            DamageARandomBuildingIfNoWoodAvailable();

            dayTimestamp -= gameTickCooldown;

            if (dayTimestamp <= 0)
            {
                PlayerInventory.Instance.CurrentDay++;
                dayTimestamp = dayDurationInSeconds;
            }
            gameTickTimestamp = Time.time + gameTickCooldown;

            if (PlayerInventory.Instance.CurrentVillageCharisma >= 100 && PlayerInventory.Instance.IsHaveEnoughHousingSpace())
            {
                if (townHallController == null)
                {
                    townHallController = GameObject.Find("TownHallNew(Clone)").GetComponent<BuildingController>();
                    return;
                }
                StartCoroutine(townHallController.SpawnVillagerAfterTime());
                PlayerInventory.Instance.CurrentVillageCharisma = 0;
            }
        }
    }

    public float CurrentDayTimestamp
    {
        get { return dayTimestamp; }
        set { dayTimestamp = value <= dayDurationInSeconds ? value : dayDurationInSeconds; }
    }

    public int CurrentSpawnersToNeutralize
    {
        get
        {
            return currentSpawnersToNeutralize;
        }
        set
        {
            currentSpawnersToNeutralize = value;
         //   OnInventoryValueChanged?.Invoke("currentDay", currentDay);
        }
    }
    private int currentSpawnersToNeutralize;

    public BuildingController GetTownHall()
    {
        return townHallController;
    }

    public void SetTownHall(BuildingController bc)
    {
        townHallController = bc;
    }

    private void DamageARandomBuildingIfNoWoodAvailable()
    {
        if (PlayerInventory.Instance.CurrentWood <= 0)
        {
            if (currentBuildingToDamageIfNoWood == null)
            {
                List<GameObject> currentBuildings = ObjectSpawner.Instance.GetAllSpawnedBuildings();
                currentBuildingToDamageIfNoWood = currentBuildings[Random.Range(0, currentBuildings.Count)].GetComponent<BuildingController>();
                if (!currentBuildingToDamageIfNoWood.GetIsBuildingComplete())
                {
                    currentBuildingToDamageIfNoWood = null;
                    return;
                }
            }
            Debug.Log(currentBuildingToDamageIfNoWood.name);
            currentBuildingToDamageIfNoWood.TakeDamage(currentBuildingToDamageIfNoWood.GetMaxHP() * 0.05f, null);
        }
    }

    public void EnableSpecificHarvesting(UpgradeToUnlock specificHarvesting, bool isUnlock)
    {
        switch (specificHarvesting)
        {
            case UpgradeToUnlock.WoodHarvesting:
                isWorkersCanHarvestWood = isUnlock;
                break;
            case UpgradeToUnlock.GoldHarvesting:
                isWorkersCanHarvestGold = isUnlock;
                break;
            case UpgradeToUnlock.IronHarvesting:
                isWorkersCanHarvestIron = isUnlock;
                break;
            case UpgradeToUnlock.FoodHarvesting:
                isWorkersCanHarvestFood = isUnlock;
                break;
        }
    }

    public bool IsWoodHarvestingUnlocked()
    {
        return isWorkersCanHarvestWood;
    }

    public bool IsGoldHarvestingUnlocked()
    {
        return isWorkersCanHarvestGold;
    }

    public bool IsIronHarvestingUnlocked()
    {
        return isWorkersCanHarvestIron;
    }

    public bool IsFoodHarvestingUnlocked()
    {
        return isWorkersCanHarvestFood;
    }
}
