using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UpgradeToUnlock { None, WoodHarvesting, GoldHarvesting, IronHarvesting, FoodHarvesting }
public enum Buildings { None, TownHall, Woodcamp, Ironmine, Goldmine, House, Turret }
public class Progress_Manager : MonoBehaviour
{
    public static Progress_Manager Instance;

    private BuildingController townHallController;

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
    }

    private void Update()
    {
        PlayerInventory.Instance.CurrentVillageCharisma += Time.deltaTime * 0.1f;
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

    public void EnableSpecificHarvesting(UpgradeToUnlock specificHarvesting)
    {
        switch (specificHarvesting)
        {
            case UpgradeToUnlock.WoodHarvesting:
                isWorkersCanHarvestWood = true;
                break;
            case UpgradeToUnlock.GoldHarvesting:
                isWorkersCanHarvestGold = true;
                break;
            case UpgradeToUnlock.IronHarvesting:
                isWorkersCanHarvestIron = true;
                break;
            case UpgradeToUnlock.FoodHarvesting:
                isWorkersCanHarvestFood = true;
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
