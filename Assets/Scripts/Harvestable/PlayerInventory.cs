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

    private void Start()
    {
        if(Chochosan.SaveLoadManager.IsSaveExists())
        {
            CurrentWood = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentWood;
            CurrentGold = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentGold;
            CurrentIron = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentIron;
            MaxPopulation = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.maxPopulation;
            CurrentVillageCharisma = Chochosan.SaveLoadManager.savedGameData.inventorySaveData.currentCharisma;
        }
        else
        {
            CurrentWood = 120;
            CurrentGold = 2;
            CurrentIron = 2;
            CurrentVillageCharisma = 0;
        }
       

        //Event subscription
        objectSpawner = GetComponent<ObjectSpawner>();
        objectSpawner.OnObjectBuildableSpawnedAtWorld += SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded += SpendResources;
    }

    private void OnDisable()
    {
        objectSpawner.OnObjectBuildableSpawnedAtWorld -= SpendResources;
        Chochosan.EventManager.Instance.OnBuildingUpgraded -= SpendResources;
    }

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

    public bool IsHaveEnoughHousingSpace()
    {
        return CurrentPopulation < MaxPopulation;
    }

    //Called when building/upgrading. All cost requirements are stored in a ScriptableObject that is passed to here.
    public bool IsHaveEnoughResources(SO_CostRequirements requirements)
    {
        if(currentWood >= requirements.woodRequired &&
           currentGold >= requirements.goldRequired &&
           currentIron >= requirements.ironRequired
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
    }

    public InventorySaveData GetInventory()
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.currentWood = currentWood;
        saveData.currentGold = currentGold;
        saveData.currentIron = currentIron;
        saveData.maxPopulation = maxPopulation;
        saveData.currentCharisma = currentVillageCharisma;
        return saveData;
    }
}
