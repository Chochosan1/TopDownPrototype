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
        CurrentWood = 12;
        CurrentGold = 0;
        CurrentIron = 0;
    }

    private void OnEnable()
    {
        objectSpawner = GetComponent<ObjectSpawner>();
        objectSpawner.OnObjectSpawnedAtWorld += SpendResources;
    }

    private void OnDisable()
    {
        objectSpawner.OnObjectSpawnedAtWorld -= SpendResources;
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

    public bool IsHaveEnoughResources(RequirementsToBuild requirements)
    {
        if(currentWood >= requirements.woodRequired &&
           currentGold >= requirements.goldRequired &&
           currentIron >= requirements.ironRequired
           )
        {
            return true;
        }         
        return false;
    }

    public void SpendResources(RequirementsToBuild requirements)
    {
        CurrentWood -= requirements.woodRequired;
        CurrentGold -= requirements.goldRequired;
        CurrentIron -= requirements.ironRequired;        
    }
}
