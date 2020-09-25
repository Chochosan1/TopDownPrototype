using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls absolutely all harvestables in the world and keeps them in lists.
/// </summary>
public class HarvestableLoader : MonoBehaviour
{
    //used for spawning
    [SerializeField] private GameObject[] harvestablePrefabs;

    //all default scene harvestables; they must be destroyed if a save file exists in order to avoid duplicates
    [SerializeField] private GameObject defaultSceneResourcesPrefab; 

    //all harvestables; non-serializable list
    public static List<Harvestable_Controller> allSpawnedHarvestables;

    //copy of all important harvestable stats; serializable list
    public static List<HarvestableControllerSerializable> allSpawnedHarvestablesSerializable; 

    private void Awake()
    {
        allSpawnedHarvestables = new List<Harvestable_Controller>();
        allSpawnedHarvestablesSerializable = new List<HarvestableControllerSerializable>();
    }

    private void Start()
    {
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            Destroy(defaultSceneResourcesPrefab);
            foreach (HarvestableControllerSerializable hcs in Chochosan.SaveLoadManager.savedGameData.harvestableList)
            {
                //load the position
                Vector3 tempPos;
                tempPos.x = hcs.x;
                tempPos.y = hcs.y;
                tempPos.z = hcs.z;

                //spawn the correct object based on the saved index
                GameObject tempHarvestable = Instantiate(harvestablePrefabs[hcs.harvestableIndex], tempPos, Quaternion.identity);

                //set specific controller stats
                Harvestable_Controller tempController = tempHarvestable.GetComponent<Harvestable_Controller>();              
                tempController.currentResourcesToHarvest = hcs.currentResourcesToHarvest;

                //add the object to the list to allow further saving overriding
                AddHarvestableToList(tempController);
            }
        }
    }

    //an object has been spawned (no matter where and how)? -> add it to the list to allow it to be saved later on
    public static void AddHarvestableToList(Harvestable_Controller harvestable)
    {
        allSpawnedHarvestables.Add(harvestable);
    }

    //an object has been destroyed (no matter where and how)? -> remove it from the list to avoid sleepless nights
    public static void RemoveHarvestableFromList(Harvestable_Controller harvestable)
    {
        allSpawnedHarvestables.Remove(harvestable);
    }

    public static void AddSerializableHarvestableToList(HarvestableControllerSerializable harvestable)
    {
        allSpawnedHarvestablesSerializable.Add(harvestable);
    }

    //All harvestables in the world are in a list that is not serializable. Looping through that list a serializable copy of the 
    //harvestable controller is created and put in a serializable list. That list is then sent to the SaveLoadManager and extracted
    //later in order to load the harvestables save.
    public static List<HarvestableControllerSerializable> GetHarvestables()
    {
        foreach (Harvestable_Controller harvestableController in HarvestableLoader.allSpawnedHarvestables)
        {
            HarvestableControllerSerializable hcs = harvestableController.GetHarvestableData();
            HarvestableLoader.AddSerializableHarvestableToList(hcs);
        }
        return HarvestableLoader.allSpawnedHarvestablesSerializable;
    }
}
