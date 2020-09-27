using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to a harvestable object. The HarvestableType determines what kind of a resource the harvestable object will yield to the PlayerInventory.
/// </summary>
public enum HarvestableType { Wood, Gold, Iron, BuildingInProgress }
public class Harvestable_Controller : MonoBehaviour, IHarvestable
{
    private int currentHarvestableIndex; //used to spawn the right object when loading data
    [SerializeField] private SO_ResourceStats stats;
    [SerializeField] private HarvestableType harvestableType;
    private BuildingController buildingController; //building also work like Harvestables but the villagers add progress to them
    public float currentResourcesToHarvest;

    private void Start()
    {
        switch (harvestableType)
        {
            case HarvestableType.Wood:
                currentHarvestableIndex = 0;
                break;
            case HarvestableType.Gold:
                currentHarvestableIndex = 1;
                break;
            case HarvestableType.Iron:
                currentHarvestableIndex = 2;
                break;
            case HarvestableType.BuildingInProgress:
                buildingController = GetComponentInParent<BuildingController>();
                break;

            
        }
        if(!Chochosan.SaveLoadManager.IsSaveExists() && harvestableType != HarvestableType.BuildingInProgress)
        {
            HarvestableLoader.AddHarvestableToList(this);
            currentResourcesToHarvest = stats.maxResourcesToHarvest;
        }       
    }

    public void Harvest()
    {
        switch (harvestableType)
        {
            case HarvestableType.Wood:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentWood += stats.resourcePerSingleHarvest;
                CheckHarvestableState();
                break;
            case HarvestableType.Gold:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentGold += stats.resourcePerSingleHarvest;
                CheckHarvestableState();
                break;
            case HarvestableType.Iron:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentIron += stats.resourcePerSingleHarvest;
                CheckHarvestableState();
                break;
            case HarvestableType.BuildingInProgress:
                buildingController.AddBuildProgress();
                break;
        }
        
    }

    //removes the harvestable if depleted
    private void CheckHarvestableState()
    {
        if (currentResourcesToHarvest <= 0)
        {
            HarvestableLoader.RemoveHarvestableFromList(this);
            Destroy(gameObject);
        }
    }

    #region DataSaving
    public HarvestableControllerSerializable GetHarvestableData()
    {
        HarvestableControllerSerializable hcs = new HarvestableControllerSerializable();
        hcs.harvestableIndex = currentHarvestableIndex;
        hcs.currentResourcesToHarvest = currentResourcesToHarvest;
        hcs.x = transform.position.x;
        hcs.y = transform.position.y;
        hcs.z = transform.position.z;
        return hcs;
    }
    #endregion
}
