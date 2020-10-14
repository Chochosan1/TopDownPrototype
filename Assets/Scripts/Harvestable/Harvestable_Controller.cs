using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attach to a harvestable object. The HarvestableType determines what kind of a resource the harvestable object will yield to the PlayerInventory.
/// </summary>
public enum HarvestableType { Wood, Gold, Iron, Food, BuildingInProgress }

[RequireComponent(typeof(BoxCollider))] //the collider must be present in order for the object to be detectable by villagers
public class Harvestable_Controller : MonoBehaviour, IHarvestable
{
    [SerializeField] private int currentHarvestableIndex; //used to spawn the right object when loading data; should match the prefab set in the list in HarvestableLoader
    [SerializeField] private SO_ResourceStats stats;
    [SerializeField] private HarvestableType harvestableType;
    [SerializeField] private GameObject depletedParticle;
    public float customStoppingDistance;
    private BuildingController buildingController; //building also work like Harvestables but the villagers add progress to them
    public float currentResourcesToHarvest;

    private void Start()
    {
        switch (harvestableType)
        {
            //case HarvestableType.Wood:
            //    currentHarvestableIndex = 0;
            //    break;
            //case HarvestableType.Gold:
            //    currentHarvestableIndex = 1;
            //    break;
            //case HarvestableType.Iron:
            //    currentHarvestableIndex = 2;
            //    break;
            //case HarvestableType.Food:
            //    currentHarvestableIndex = 3;
            //    break;
            case HarvestableType.BuildingInProgress:
                buildingController = GetComponentInParent<BuildingController>();
                break;


        }
        if (!Chochosan.SaveLoadManager.IsSaveExists() && harvestableType != HarvestableType.BuildingInProgress) //dont add buildingInProgress to the harvestable list; this is instead handled by the general building saving
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
            case HarvestableType.Food:
                currentResourcesToHarvest -= stats.resourcePerSingleHarvest;
                PlayerInventory.Instance.CurrentFood += stats.resourcePerSingleHarvest;
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
            gameObject.GetComponent<Animator>().SetTrigger("isDepleted");
            depletedParticle.SetActive(true);
            Destroy(this);
            Destroy(gameObject, 2f);
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
