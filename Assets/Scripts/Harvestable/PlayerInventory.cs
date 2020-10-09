using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            CurrentWood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentWood;
            CurrentGold = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentGold;
            CurrentIron = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentIron;
            MaxPopulation = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxPopulation;
            CurrentVillageCharisma = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentCharisma;
            CurrentFood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentFood;
            CurrentDay = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentDay;
        }
        else
        {
            CurrentWood = 120;
            CurrentGold = 2;
            CurrentIron = 2;
            CurrentVillageCharisma = 0;
            CurrentDay = 1;
        }


        //Event subscription
        objectSpawner = GetComponent<ObjectSpawner>();
        objectSpawner.OnObjectBuildableSpawnedAtWorld += SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded += SpendResources;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally += CalculateUpkeep;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally += AddAutoFoodGeneration;
    }

    private void OnDisable()
    {
        objectSpawner.OnObjectBuildableSpawnedAtWorld -= SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded -= SpendResources;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally -= CalculateUpkeep;
        Chochosan.EventManager.Instance.OnBuildingBuiltFinally -= AddAutoFoodGeneration;
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
            currentWood = value;
            OnInventoryValueChanged?.Invoke("wood", currentWood);
        }
    }
    private float currentWood;

    public float CurrentGold
    {
        get
        {
            return currentGold;
        }
        set
        {
            currentGold = value;
            OnInventoryValueChanged?.Invoke("gold", currentGold);
        }
    }
    private float currentGold;

    public float CurrentIron
    {
        get
        {
            return currentIron;
        }
        set
        {
            currentIron = value;
            OnInventoryValueChanged?.Invoke("iron", currentIron);
        }
    }
    private float currentIron;

    public int CurrentPopulation
    {
        get
        {
            return currentPopulation;
        }
        set
        {
            Debug.Log("ADDED POP");
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
            currentFood = value;
            OnInventoryValueChanged?.Invoke("food", currentFood);
        }
    }
    private float currentFood;

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
        Chochosan.UI_Manager.Instance.DisplayWarningMessage();
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

    private void AddAutoFoodGeneration(BuildingController bc, Buildings buildingType)
    {
        switch (buildingType)
        {
            case Buildings.Mill:
                CurrentAutoFoodGeneration += bc.FoodGeneration;
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
        saveData.currentDay = currentDay;
        saveData.currentDayTimestamp = Progress_Manager.Instance.CurrentDayTimestamp;
        return saveData;
    }
}
