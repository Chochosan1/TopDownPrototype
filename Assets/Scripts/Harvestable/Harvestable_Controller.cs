using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to a harvestable object. The HarvestableType determines what kind of a resource the harvestable object will yield to the PlayerInventory.
/// </summary>
public enum HarvestableType { Wood, Gold, Iron }
public class Harvestable_Controller : MonoBehaviour, IHarvestable
{
    [SerializeField] private SO_ResourceStats stats;
    [SerializeField] private HarvestableType harvestableType;
    private float currentResourcesToHarvest;

    void Start()
    {
        currentResourcesToHarvest = stats.maxResourcesToHarvest;
    }

    public void Harvest()
    {
        switch (harvestableType)
        {
            case HarvestableType.Wood:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentWood += stats.resourcePerSingleHarvest;           
                break;
            case HarvestableType.Gold:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentGold += stats.resourcePerSingleHarvest;
                break;
            case HarvestableType.Iron:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentIron += stats.resourcePerSingleHarvest;
                break;
        }
        CheckHarvestableState();
    }

    //removes the harvestable if depleted
    private void CheckHarvestableState()
    {
        if (currentResourcesToHarvest <= 0)
        {
            Destroy(gameObject);
        }
    }
}
