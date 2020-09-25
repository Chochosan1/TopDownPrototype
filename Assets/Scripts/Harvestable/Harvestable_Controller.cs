using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to a harvestable object. The HarvestableType determines what kind of a resource the harvestable object will yield to the PlayerInventory.
/// </summary>
public enum HarvestableType { Wood, Gold, Iron }
public class Harvestable_Controller : MonoBehaviour, IHarvestable
{
    private int currentHarvestableIndex; //used to spawn the right object when loading data
    [SerializeField] private SO_ResourceStats stats;
    [SerializeField] private HarvestableType harvestableType;
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
        }
        if(!Chochosan.SaveLoadManager.IsSaveExists())
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
