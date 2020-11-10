using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Takes care of all stats for the village.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    //event triggered whenever some currency in the inventory changes; subscribed to for example in the UI_Manager to update certain texts
    public delegate void OnInventoryValueChangedDelegate(string valueName, float value);
    public event OnInventoryValueChangedDelegate OnInventoryValueChanged;

    private ObjectSpawner objectSpawner;
    [Header("Production")]
    [Tooltip("That much food is required in order to activate the auto food generation that comes from buildings like the Mill.")]
    [SerializeField] private float minFoodRequiredToAutoGenerateFood = 15f;
    [SerializeField] private float warehouseBonusWoodCapacity;
    [SerializeField] private float warehouseBonusIronCapacity;
    [SerializeField] private float warehouseBonusGoldCapacity;
    [SerializeField] private float warehouseBonusFoodCapacity;

    private void Start()
    {
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            MaxWood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxWood;
            MaxGold = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxGold;
            MaxIron = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxIron;
            MaxFood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxFood;

            CurrentWood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentWood;
            CurrentGold = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentGold;
            CurrentIron = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentIron;
            MaxPopulation = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxPopulation;
            CurrentVillageCharisma = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentCharisma;
            CurrentFood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentFood;
            CurrentAutoFoodGeneration = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentAutoFoodGeneration;
            CurrentDay = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentDay;
        }
        else
        {
            MaxWood = 160;
            MaxGold = 15;
            MaxIron = 25;
            MaxFood = 50;

            CurrentWood = 160;
            CurrentGold = 0;
            CurrentIron = 25;
            CurrentFood = 40;
            CurrentVillageCharisma = 0;
            CurrentAutoFoodGeneration = 0;
            CurrentDay = 1;
        }

        //Event subscription
        objectSpawner = GetComponent<ObjectSpawner>();
        objectSpawner.OnObjectBuildableSpawnedAtWorld += SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded += SpendResources;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally += CalculateUpkeep;
        //    Chochosan.EventManager.Instance.OnBuildingBuiltFinally += AddBuildingBonus;
    }

    private void OnDisable()
    {
        objectSpawner.OnObjectBuildableSpawnedAtWorld -= SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded -= SpendResources;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally -= CalculateUpkeep;
        //     Chochosan.EventManager.Instance.OnBuildingBuiltFinally -= AddBuildingBonus;
    }

    public float CurrentWoodUpkeep
    {
        get
        {
            return currentWoodUpkeep;
        }
        set
        {
            currentWoodUpkeep = value;
            OnInventoryValueChanged?.Invoke("woodupkeep", currentWoodUpkeep);
        }
    }
    private float currentWoodUpkeep;

    public float CurrentWood
    {
        get
        {
            return currentWood;
        }
        set
        {
            if (value <= maxWood)
            {
                currentWood = value;

                if (currentWood < 0)
                    currentWood = 0;
            }
            else
            {
                currentWood = maxWood;
                Debug.Log("MAX WOOD GRANTED -> " + currentWood + " / " + value);
            }
            OnInventoryValueChanged?.Invoke("wood", currentWood);
        }
    }
    private float currentWood;

    public float MaxWood
    {
        get
        {
            return maxWood;
        }
        set
        {
            maxWood = value;
            OnInventoryValueChanged?.Invoke("maxWood", maxWood);
        }
    }
    private float maxWood;

    public float CurrentGold
    {
        get
        {
            return currentGold;
        }
        set
        {
            if (value <= maxGold)
            {
                currentGold = value;

                if (currentGold < 0)
                    currentGold = 0;
            }
            else
            {
                currentGold = maxGold;
            }
            OnInventoryValueChanged?.Invoke("gold", currentGold);
        }
    }
    private float currentGold;

    public float MaxGold
    {
        get
        {
            return maxGold;
        }
        set
        {
            maxGold = value;
            OnInventoryValueChanged?.Invoke("maxGold", maxGold);
        }
    }
    private float maxGold;

    public float CurrentIron
    {
        get
        {
            return currentIron;
        }
        set
        {
            if (value <= maxIron)
            {
                currentIron = value;

                if (currentIron < 0)
                    currentIron = 0;
            }
            else
            {
                currentIron = maxIron;
            }
            OnInventoryValueChanged?.Invoke("iron", currentIron);
        }
    }
    private float currentIron;

    public float MaxIron
    {
        get
        {
            return maxIron;
        }
        set
        {
            maxIron = value;
            OnInventoryValueChanged?.Invoke("maxIron", maxIron);
        }
    }
    private float maxIron;

    public int CurrentPopulation
    {
        get
        {
            return currentPopulation;
        }
        set
        {
            currentPopulation = value;
            OnInventoryValueChanged?.Invoke("currentPopulation", currentPopulation);
        }
    }
    private int currentPopulation;

    public int MaxPopulation
    {
        get
        {
            return maxPopulation;
        }
        set
        {
            maxPopulation = value;
            OnInventoryValueChanged?.Invoke("maxPopulation", maxPopulation);
        }
    }
    private int maxPopulation;

    public float CurrentVillageCharisma
    {
        get
        {
            return currentVillageCharisma;
        }
        set
        {
            currentVillageCharisma = value;
            OnInventoryValueChanged?.Invoke("charisma", currentVillageCharisma);
        }
    }
    private float currentVillageCharisma;

    public float CurrentFood
    {
        get
        {
            return currentFood;
        }
        set
        {
            if (value <= maxFood)
            {
                currentFood = value;

                if (currentFood < 0)
                    currentFood = 0;
            }
            else
            {
                currentFood = maxFood;
            }
            OnInventoryValueChanged?.Invoke("food", currentFood);
        }
    }
    private float currentFood;

    public float MaxFood
    {
        get
        {
            return maxFood;
        }
        set
        {
            maxFood = value;
            OnInventoryValueChanged?.Invoke("maxFood", maxFood);
        }
    }
    private float maxFood;

    public float CurrentAutoFoodGeneration
    {
        get
        {
            if (currentFood >= minFoodRequiredToAutoGenerateFood)
            {
                OnInventoryValueChanged?.Invoke("autoFood", currentAutoFoodGeneration);
                return currentAutoFoodGeneration;
            }
            else
            {
                OnInventoryValueChanged?.Invoke("autoFood", 0);
                return 0;
            }
        }
        set
        {
            currentAutoFoodGeneration = value;
            OnInventoryValueChanged?.Invoke("autoFood", currentAutoFoodGeneration);
        }
    }
    private float currentAutoFoodGeneration;

    public float CurrentFoodConsumption
    {
        get
        {
            return currentFoodConsumption;
        }
        set
        {
            currentFoodConsumption = value;
            OnInventoryValueChanged?.Invoke("foodConsumption", currentFoodConsumption);
        }
    }
    private float currentFoodConsumption;

    public int CurrentDay
    {
        get
        {
            return currentDay;
        }
        set
        {
            currentDay = value;
            OnInventoryValueChanged?.Invoke("currentDay", currentDay);
            Chochosan.EventManager.Instance.OnNewDay?.Invoke();
        }
    }
    private int currentDay;

    public bool IsHaveEnoughHousingSpace()
    {
        return CurrentPopulation < MaxPopulation;
    }

    //Called when building/upgrading. All cost requirements are stored in a ScriptableObject that is passed to here.
    public bool IsHaveEnoughResources(SO_CostRequirements requirements)
    {
        if (currentWood >= requirements.woodRequired &&
           currentGold >= requirements.goldRequired &&
           currentIron >= requirements.ironRequired &&
           currentFood >= requirements.foodRequired
           )
        {
            return true;
        }
        Chochosan.UI_Manager.Instance.DisplayWarningMessage("NOT ENOUGH RESOURCES");
        return false;
    }

    public void SpendResources(SO_CostRequirements requirements)
    {
        CurrentWood -= requirements.woodRequired;
        CurrentGold -= requirements.goldRequired;
        CurrentIron -= requirements.ironRequired;
        CurrentFood -= requirements.foodRequired;
    }

    private void CalculateUpkeep(BuildingController bc, Buildings buildingType)
    {
        CurrentWoodUpkeep += bc.WoodUpkeep;
    }

    public void AddBuildingsProgress(BuildingController bc, Buildings buildingType)
    {
        switch (buildingType)
        {
            case Buildings.Mill:
                CurrentAutoFoodGeneration += bc.FoodGeneration;
                break;
            case Buildings.Warehouse:
                MaxWood += warehouseBonusWoodCapacity;
                MaxIron += warehouseBonusIronCapacity;
                MaxGold += warehouseBonusGoldCapacity;
                MaxFood += warehouseBonusFoodCapacity;
                break;
            case Buildings.TownHall:
                Progress_Manager.Instance.SetTownHall(bc);
                break;
        }
    }

    public InventorySaveData GetInventory()
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.currentWood = currentWood;
        saveData.currentGold = currentGold;
        saveData.currentIron = currentIron;
        saveData.maxPopulation = maxPopulation;
        saveData.currentCharisma = currentVillageCharisma;
        saveData.currentFood = currentFood;
        saveData.currentAutoFoodGeneration = currentAutoFoodGeneration;
        saveData.currentDay = currentDay;
        saveData.maxWood = maxWood;
        saveData.maxGold = maxGold;
        saveData.maxIron = maxIron;
        saveData.maxFood = maxFood;
        saveData.currentDayTimestamp = Progress_Manager.Instance.CurrentDayTimestamp;
        return saveData;
    }
}
