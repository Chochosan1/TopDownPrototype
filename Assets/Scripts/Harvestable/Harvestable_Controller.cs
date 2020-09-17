using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvestable_Controller : MonoBehaviour, IHarvestable
{
    [SerializeField] private SO_ResourceStats stats;
    private float currentResourcesToHarvest;

    void Start()
    {
        currentResourcesToHarvest = stats.maxResourcesToHarvest;
    }

    public void Harvest()
    {
        currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
        PlayerInventory.Instance.CurrentWood += stats.resourcePerSingleHarvest;
        CheckHarvestableState();
    }

    private void CheckHarvestableState()
    {
        if(currentResourcesToHarvest <= 0)
        {
            Destroy(gameObject);
        }
    }
}
