using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls unit selection and commanding. Controllable units must be in a specific layer and inherit the necessary ISelectable interface.
/// </summary>
public class Unit_Controller : MonoBehaviour
{
    public static Unit_Controller Instance;
    [SerializeField] private GameObject commandIconPrefab;
    [SerializeField] private Texture2D selectRectTexture2D = null;
    [SerializeField] private Texture2D cursorDefault, cursorUnit, cursorBuilding, cursorEnemy, cursorHarvestables;
    [SerializeField] private LayerMask selectableUnitLayer;
    [SerializeField] private LayerMask selectableBuildingLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask harvestablesLayer;
    [SerializeField] private LayerMask movableAreaLayer;
    [SerializeField] private LayerMask UILayer;
    private ObjectSpawner objectSpawner;
    private Camera mainCamera;
    [Tooltip("A villager to spawn after loading a save or by a building.")]
    [SerializeField] private GameObject genericVillager;
    private List<AI_Villager> spawnedVillagersList;

    //unit selection
    private List<GameObject> currentlySelectedUnits;
    private GameObject currentlySelectedBuilding;
    private ISelectable tempSelectableBuilding;
    private bool isInteractingWithUI;
    [HideInInspector]
    public Rect selectRect = new Rect();
    private Vector3 startClickRect = -Vector3.one;

    public delegate void OnUnitSelectedDelegate(ISelectable unitSelectable, BuildingController bc);
    public event OnUnitSelectedDelegate OnUnitSelected;

    public delegate void OnUnitDeselectedDelegate();
    public event OnUnitDeselectedDelegate OnUnitDeselected;

    public delegate void OnTryToSelectUnitsDelegate();
    public event OnTryToSelectUnitsDelegate OnTryToSelectUnits;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        spawnedVillagersList = new List<AI_Villager>();
    }

    void Start()
    {
        Cursor.SetCursor(cursorDefault, Vector2.zero, CursorMode.Auto);
        currentlySelectedUnits = new List<GameObject>();
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
                GameObject tempVillager = SpawnSpecificUnit(tempVillagerPos, Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerTypeStrings[i]);
                tempVillager.GetComponent<AI_Villager>().currentHealth = Chochosan.SaveLoadManager.savedGameData.unitsSaveData.villagerHealthValues[i];
            }
        }
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, selectableUnitLayer))
        {
            Cursor.SetCursor(cursorUnit, Vector2.zero, CursorMode.Auto);
        }
        else if (Physics.Raycast(ray, out hit, 100, selectableBuildingLayer))
        {
            Cursor.SetCursor(cursorBuilding, Vector2.zero, CursorMode.Auto);
        }
        else if (Physics.Raycast(ray, out hit, 100, enemyLayer))
        {
            Cursor.SetCursor(cursorEnemy, Vector2.zero, CursorMode.Auto);
        }
        else if (Physics.Raycast(ray, out hit, 100, harvestablesLayer))
        {
            Cursor.SetCursor(cursorHarvestables, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(cursorDefault, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!objectSpawner.IsCurrentlySpawningBuilding())
            {
                startClickRect = Input.mousePosition;
                //SelectUnit();
            }
        }

        if (Input.GetMouseButton(0))
        {
            //resize the rect
            selectRect = new Rect(startClickRect.x, InvertMouseY(startClickRect.y), Input.mousePosition.x - startClickRect.x,
                InvertMouseY(Input.mousePosition.y) - InvertMouseY(startClickRect.y));
        }

        if (Input.GetMouseButtonUp(0))
        {
            isInteractingWithUI = Chochosan.UI_Manager.IsPointerOverUIElement();

            //clear all currently selected units
            if (!isInteractingWithUI)
                ClearCurrentlySelectedUnits();

            //hide the rect 
            startClickRect = -Vector3.one;

            //so that selectRect works no matter from which to which direction it goes
            if (selectRect.width < 0)
            {
                selectRect.x += selectRect.width;
                selectRect.width = -selectRect.width;
            }
            if (selectRect.height < 0)
            {
                selectRect.y += selectRect.height;
                selectRect.height = -selectRect.height;
            }

            if (!isInteractingWithUI)
            {
                OnTryToSelectUnits?.Invoke();
                if(currentlySelectedUnits.Count == 0)
                {
                    SelectUnit();
                }       
                else if(currentlySelectedUnits.Count == 1)
                {
                    OnUnitSelected?.Invoke(currentlySelectedUnits[0].GetComponent<ISelectable>(), null);
                }
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

    public float InvertMouseY(float y)
    {
        return Screen.height - y;
    }

    //draw the select rect here
    private void OnGUI()
    {
        if (startClickRect != -Vector3.one)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.DrawTexture(selectRect, selectRectTexture2D);
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
                AddUnitToSelectedList(hit.collider.gameObject);
                ISelectable tempSelectable = hit.collider.gameObject.GetComponent<ISelectable>();
                if (tempSelectable != null)
                {
                    tempSelectable.ToggleSelectedIndicator(true);
                    OnUnitSelected?.Invoke(tempSelectable, null);
                }
                Debug.Log(hit.collider.gameObject);
            }
            else if (Physics.Raycast(ray, out hit, 100, selectableBuildingLayer))
            {
                currentlySelectedBuilding = hit.collider.gameObject;
                tempSelectableBuilding = currentlySelectedBuilding.GetComponent<ISelectable>();
                if (tempSelectableBuilding != null)
                {
                    tempSelectableBuilding.ToggleSelectedIndicator(true);
                    OnUnitSelected?.Invoke(tempSelectableBuilding, currentlySelectedBuilding.GetComponent<BuildingController>());
                }
                Debug.Log(hit.collider.gameObject);
            }
        }
    }

    private void CommandSelectedUnit()
    {
        if (currentlySelectedUnits != null && currentlySelectedUnits.Count > 0)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (movableAreaLayer == (movableAreaLayer | (1 << hit.collider.gameObject.layer))) //check if the object is in the specific layer
                {
                    ShowCommandIcon(hit.point);
                    foreach (GameObject selectedUnit in currentlySelectedUnits)
                    {
                        //offset each individual unit's position so that they don't fight for exactly the same spot
                        int offset = currentlySelectedUnits.IndexOf(selectedUnit);
                        Vector3 position = hit.point + new Vector3(offset, 0, offset / 2);
                        if(selectedUnit != null)
                            selectedUnit.GetComponent<ISelectable>()?.ForceSetAgentArea(position);
                    }
                }
                else
                {
                    foreach (GameObject selectedUnit in currentlySelectedUnits)
                    {
                        if (hit.collider.gameObject != null && selectedUnit != null)
                            selectedUnit.GetComponent<ISelectable>()?.ForceSetSpecificTarget(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    private void ShowCommandIcon(Vector3 position)
    {
        commandIconPrefab.transform.position = position;
     
        //disable and then reenable to reset the state of the animation
        commandIconPrefab.SetActive(false);
        commandIconPrefab.SetActive(true);
    }


    public GameObject GetCurrentlySelectedBuilding()
    {
        return currentlySelectedBuilding;
    }

    private Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

    private void ClearCurrentlySelectedUnits()
    {
        currentlySelectedUnits.Clear();
        currentlySelectedBuilding = null;
        tempSelectableBuilding?.ToggleSelectedIndicator(false);
        tempSelectableBuilding = null;
        OnUnitDeselected?.Invoke();
    }

    public void AddUnitToSelectedList(GameObject selectedUnit)
    {
        currentlySelectedUnits.Add(selectedUnit);
    }
    #endregion

    public void AddVillagerToList(AI_Villager villager)
    {
        Debug.Log("Villager added: " + villager.name);
        spawnedVillagersList.Add(villager);
    }

    public void RemoveVillagerToList(AI_Villager villager)
    {
        spawnedVillagersList.Remove(villager);
    }

    public UnitsSerializable GetSpawnedUnitsData()
    {
        UnitsSerializable us = new UnitsSerializable();

        //this many villagers will spawn later on when loading data
        us.numberOfVillagersAssigned = spawnedVillagersList.Count;
        Debug.Log(spawnedVillagersList.Count);
        //initialize the arrays from the serializable copy
        us.villagerXpositions = new float[spawnedVillagersList.Count];
        us.villagerYpositions = new float[spawnedVillagersList.Count];
        us.villagerZpositions = new float[spawnedVillagersList.Count];
        us.villagerTypeStrings = new string[spawnedVillagersList.Count];
        us.villagerHealthValues = new float[spawnedVillagersList.Count];

        //store each villager's X, Y, Z positions in arrays
        for (int i = 0; i < spawnedVillagersList.Count; i++)
        {
            us.villagerXpositions[i] = spawnedVillagersList[i].transform.position.x;
            us.villagerYpositions[i] = spawnedVillagersList[i].transform.position.y;
            us.villagerZpositions[i] = spawnedVillagersList[i].transform.position.z;
            us.villagerHealthValues[i] = spawnedVillagersList[i].currentHealth;
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

    private GameObject SpawnSpecificUnit(Vector3 position, string villagerType)
    {
        if (genericVillager != null)
        {
            GameObject tempVillagerGameobject = Instantiate(genericVillager, position, genericVillager.transform.rotation);
            AI_Villager tempVillagerController = tempVillagerGameobject.GetComponent<AI_Villager>();
            tempVillagerController.SwitchVillagerType(villagerType);
            tempVillagerController.SetInitialHP();
            //very important to assign the villager to the building after spawning (useful for loading/saving data later on because the list must not be empty)
            AddVillagerToList(tempVillagerController);
            Debug.Log("LOADED AND ADDED TO LIST ONE VILLAGER");
            return tempVillagerGameobject;
        }
        return null;
    }
}
