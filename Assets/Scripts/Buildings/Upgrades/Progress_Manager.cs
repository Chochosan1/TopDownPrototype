using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Progress_Manager : MonoBehaviour
{
    public static Progress_Manager Instance;

    private bool isWorkersCanHarvestWood;
    private bool isWorkersCanHarvestGold;
    private bool isWorkersCanHarvestIron;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
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
}
