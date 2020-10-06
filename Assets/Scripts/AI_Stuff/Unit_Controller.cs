using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls unit selection and commanding. Controllable units must be in a specific layer and inherit the necessary ISelectable interface.
/// </summary>
public class Unit_Controller : MonoBehaviour
{
    public static Unit_Controller Instance;
    [SerializeField] private LayerMask selectableUnitLayer;
    [SerializeField] private LayerMask selectableBuildingLayer;
    [SerializeField] private LayerMask movableAreaLayer;
    private ObjectSpawner objectSpawner;
    private Camera mainCamera;
    private GameObject currentlySelectedUnit;
    private ISelectable tempSelectable;
    [SerializeField] private GameObject genericVillager;
    [Tooltip("All villagers that have been spawned by this building.")]
    [SerializeField] private List<AI_Villager> spawnedVillagersList;

    public delegate void OnUnitSelectedDelegate(ISelectable unitSelectable);
    public event OnUnitSelectedDelegate OnUnitSelected;

    public delegate void OnUnitDeselectedDelegate();
    public event OnUnitDeselectedDelegate OnUnitDeselected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnedVillagersList = new List<AI_Villager>();
        objectSpawner = GetComponent<ObjectSpawner>();
        mainCamera = Camera.main;

        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            //spawn that many villagers based on the number of villagersAssigned in the save file
            for (int i = 0; i < Chochosan.SaveLoadManager.savedGameData.unitsSaveData.numberOfVillagersAssigned; i++)
            {
                Vector3 tempVillagerPos;
                tempVillagerPos.x = Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerXpositions[i];
                tempVillagerPos.y = Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerYpositions[i];
                tempVillagerPos.z = Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerZpositions[i];

                //this adds the villagers to a local scope list in the controller as well
                SpawnSpecificUnit(tempVillagerPos, Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerTypeStrings[i]);
                //  Debug.Log("SETTING POSITIONS");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ClearCurrentlySelectedUnit();
        }


        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!objectSpawner.IsCurrentlySpawningBuilding())
            {
                SelectUnit();
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (!objectSpawner.IsCurrentlySpawningBuilding())
            {
                CommandSelectedUnit();
            }
        }
    }

    #region ObjectSelection
    private void SelectUnit()
    {
        if (!objectSpawner.IsCurrentlySpawningBuilding())
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100, selectableUnitLayer))
            {
                currentlySelectedUnit = hit.collider.gameObject;
                tempSelectable = currentlySelectedUnit.GetComponent<ISelectable>();
                if (tempSelectable != null)
                    OnUnitSelected?.Invoke(tempSelectable);
                Debug.Log(hit.collider.gameObject);
            }
            else if (Physics.Raycast(ray, out hit, 100, selectableBuildingLayer))
            {
                currentlySelectedUnit = hit.collider.gameObject;
                tempSelectable = currentlySelectedUnit.GetComponent<ISelectable>();
                if (tempSelectable != null)
                    OnUnitSelected?.Invoke(tempSelectable);
                Debug.Log(hit.collider.gameObject);
            }
        }
    }

    private void CommandSelectedUnit()
    {
        if (currentlySelectedUnit != null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (movableAreaLayer == (movableAreaLayer | (1 << hit.collider.gameObject.layer))) //check if the object is in the specific layer
                {
                    tempSelectable?.ForceSetAgentArea(hit.point);
                }
                else
                {
                    tempSelectable?.ForceSetSpecificTarget(hit.collider.gameObject);
                }
            }
        }
    }

    public void ClearCurrentlySelectedUnit()
    {
        currentlySelectedUnit = null;
        tempSelectable = null;
        OnUnitDeselected?.Invoke();
    }

    public void AddVillagerToList(AI_Villager villager)
    {
        spawnedVillagersList.Add(villager);
    }

    public UnitsSerializable GetSpawnedUnitsData()
    {
        UnitsSerializable us = new UnitsSerializable();

        //this many villagers will spawn later on when loading data
        us.numberOfVillagersAssigned = spawnedVillagersList.Count;

        //initialize the arrays from the serializable copy
        us.villagerXpositions = new float[spawnedVillagersList.Count];
        us.villagerYpositions = new float[spawnedVillagersList.Count];
        us.villagerZpositions = new float[spawnedVillagersList.Count];
        us.villagerTypeStrings = new string[spawnedVillagersList.Count];

        //store each villager's X, Y, Z positions in arrays
        for (int i = 0; i < spawnedVillagersList.Count; i++)
        {
            us.villagerXpositions[i] = spawnedVillagersList[i].transform.position.x;
            us.villagerYpositions[i] = spawnedVillagersList[i].transform.position.y;
            us.villagerZpositions[i] = spawnedVillagersList[i].transform.position.z;
            switch (spawnedVillagersList[i].GetCurrentVillagerType())
            {
                case Villager_Type.WoodWorker:
                    us.villagerTypeStrings[i] = "Wood";
                    break;
                case Villager_Type.GoldWorker:
                    us.villagerTypeStrings[i] = "Gold";
                    break;
                case Villager_Type.IronWorker:
                    us.villagerTypeStrings[i] = "Iron";
                    break;
                case Villager_Type.FoodWorker:
                    us.villagerTypeStrings[i] = "Food";
                    break;
                case Villager_Type.Builder:
                    us.villagerTypeStrings[i] = "Builder";
                    break;
            }
        }

        return us;
    }

    public void SpawnSpecificUnit(Vector3 position, string villagerType)
    {
        if (genericVillager != null)
        {
            GameObject tempVillagerGameobject = Instantiate(genericVillager, position, genericVillager.transform.rotation);
            AI_Villager tempVillagerController = tempVillagerGameobject.GetComponent<AI_Villager>();
            tempVillagerController.SwitchVillagerType(villagerType);
            //very important to assign the villager to the building after spawning (useful for loading/saving data later on because the list must not be empty)
            AddVillagerToList(tempVillagerController);
            Debug.Log("LOADED AND ADDED TO LIST ONE VILLAGER");
        }
    }
    #endregion
}
