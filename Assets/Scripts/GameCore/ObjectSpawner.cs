using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls world spawning of buildings using the mouse. Also handles the saving/loading of all spawned buildings.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance;
    [SerializeField] private GameObject[] previewObjects;
    [SerializeField] private GameObject[] realObjects;
    [SerializeField] private float rotationSpeed;
    [Tooltip("If set to true, the object will match the surface rotation.")]
    [SerializeField] private bool is_ObjectSlopeRotationMatch;
    private bool canRotateCurrentObject;
    private GameObject currentObject;
    private int currentObjectIndex;
    private CollisionChecker[] colCheckers;
    private int buildableLayerMask;
    private Camera mainCamera;
    private List<GameObject> allBuildingsSpawned; //holds all spawned buildings here until it is required to loop through them
    private List<BuildingControllerSerializable> allBuildingsSpawnedSerializable; //stores the current state of all spawned buildings in a serializable list

    //event subscribed to for example in PlayerInventory in order to spend resources 
    public delegate void OnObjectSpawnedAtWorldDelegate(SO_CostRequirements requirements);
    public event OnObjectSpawnedAtWorldDelegate OnObjectBuildableSpawnedAtWorld;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }      
    }

    private void Start()
    {
        allBuildingsSpawned = new List<GameObject>();
        allBuildingsSpawnedSerializable = new List<BuildingControllerSerializable>();
        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            foreach (BuildingControllerSerializable bcs in Chochosan.SaveLoadManager.savedGameData.buildingList)
            {
                //load the position of the saved object
                Vector3 tempPos;
                tempPos.x = bcs.x;
                tempPos.y = bcs.y;
                tempPos.z = bcs.z;

                //load the rotation of the saved object
                Quaternion tempRot;
                tempRot.x = bcs.rotX;
                tempRot.y = bcs.rotY;
                tempRot.z = bcs.rotZ;
                tempRot.w = bcs.rotW;

                //spawn an object at the world based on the saved object
                GameObject tempObject = Instantiate(realObjects[bcs.buildingIndex], tempPos, tempRot);
                tempObject.GetComponentInChildren<Turret>()?.EnableTurret();
                
                //set controller specific stats
                BuildingController tempController = tempObject.GetComponent<BuildingController>();
                tempController.SetIsBuildingComplete(bcs.isBuildingComplete);
                tempController.SetBuildingProgress(bcs.buildingProgress);
                tempController.SetBuildingIndex(bcs.buildingIndex);
                tempController.SetBuildingLevel(bcs.currentBuildingLevel);
                tempController.SetBuildingHP(bcs.buildingCurrentHP);

                //finally add the loaded object into the list with all objects (very important in order to allow overriding saves)
                allBuildingsSpawned.Add(tempObject);

                ////spawn that many villagers based on the number of villagersAssigned in the save file
                //for (int i = 0; i < bcs.numberOfVillagersAssigned; i++)
                //{
                //    Vector3 tempVillagerPos;
                //    tempVillagerPos.x = bcs.villagerXpositions[i];
                //    tempVillagerPos.y = bcs.villagerYpositions[i];
                //    tempVillagerPos.z = bcs.villagerZpositions[i];

                //    //this adds the villagers to a local scope list in the controller as well
                //    tempController.SpawnSpecificVillager(tempVillagerPos, bcs.villagerTypeStrings[i]);
                //  //  Debug.Log("SETTING POSITIONS");
                //}
            }
        }      
        buildableLayerMask = LayerMask.GetMask("Terrain");
        mainCamera = Camera.main;
    }


    void Update()
    {
        MoveCurrentObjectWithMouse();

        if(Input.GetKeyDown(KeyCode.I))
        {
            Chochosan.SaveLoadManager.SaveGameState();
        }
    }

    //try to spawn a preview object
    public void SpawnObject(int index)
    {
        currentObjectIndex = index;

        if(PlayerInventory.Instance.IsHaveEnoughResources(previewObjects[currentObjectIndex].GetComponent<RequirementsToBuild>().GetRequirements()))
        {
            InstantiateObjectAtMousePos(previewObjects[currentObjectIndex]);
        } 
    }

    //spawn a preview object at mouse position
    private void InstantiateObjectAtMousePos(GameObject objectToSpawn)
    {
        if (currentObject == null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, buildableLayerMask))
            {              
                currentObject = Instantiate(objectToSpawn, hit.point, transform.rotation);
                Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(true);
                colCheckers = currentObject.GetComponentsInChildren<CollisionChecker>();
            }
        }
    }

    private void MoveCurrentObjectWithMouse()
    {
        if (currentObject != null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, buildableLayerMask))
            {
                currentObject.transform.position = hit.point;

                if (is_ObjectSlopeRotationMatch)
                {
                    // get the cross from the user's left, this returns the up/down direction.
                    Vector3 lookAt = Vector3.Cross(-hit.normal, Camera.main.transform.right);
                    // reverse it if it is down.
                    lookAt = lookAt.y < 0 ? -lookAt : lookAt;
                    // look at the hit's relative up, using the normal as the up vector
                    Quaternion newRot = Quaternion.LookRotation(hit.point + lookAt, hit.normal);
                    newRot.y = currentObject.transform.rotation.y;
                    currentObject.transform.rotation = newRot;
                }
            }
        }
    }

    //spawn the final object in the world
    public void SpawnObjectAtWorld()
    {
        if (currentObject != null)
        {
            foreach (CollisionChecker colCheck in colCheckers)
            {
                if (!colCheck.CheckIsSpawningAllowed())
                {
                    Chochosan.ChochosanHelper.ChochosanDebug("ONE OF THE CHECKERS IS NOT HAPPY", "red");
                    return;
                }
            }

            GameObject tempObject = Instantiate(realObjects[currentObjectIndex], currentObject.transform.position, currentObject.transform.rotation);
            BuildingController tempController = tempObject.GetComponent<BuildingController>();
            //cache the current build index so it can be used when loading data
            tempController.SetBuildingIndex(currentObjectIndex);
         //   tempController.SetInitialHP();

            //ISpawnedAtWorld tempInterface = tempObject.GetComponent<ISpawnedAtWorld>();
            //if (tempInterface != null)
            //    tempObject.GetComponent<ISpawnedAtWorld>().StartInitialSetup();
               
            //add the spawned object to the list with all spawned objects
            allBuildingsSpawned.Add(tempObject);

            //invoke the delegate
            OnObjectBuildableSpawnedAtWorld?.Invoke(currentObject.GetComponent<RequirementsToBuild>().GetRequirements());
                 
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }

    public void RotateCurrentObject(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (currentObject != null && canRotateCurrentObject)
        {
            float rotationDirection = obj.ReadValue<float>();
            if (rotationDirection > 0)
            {
                currentObject.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime, Space.Self);
            }
            else if (rotationDirection < 0)
            {
                currentObject.transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime, Space.Self);
            }
        }
    }

    //remove the preview object
    public void CancelObject()
    {
        if (currentObject != null)
        {
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }

    public void RemoveBuildingFromList(GameObject building)
    {
        if(building == null)
        {
            return;
        }
        allBuildingsSpawned.Remove(building);
    }

    public void ToggleObjectRotation()
    {
        if(currentObject != null)
        {
            canRotateCurrentObject = !canRotateCurrentObject;
        }       
    }

    public bool IsCurrentlySpawningBuilding()
    {
        return currentObject != null;
    }

    #region DataSaving
    //saves the current state of all spawned buildings from the non-serializable list to a serializable list
    public List<BuildingControllerSerializable> GetBuildingsInfo()
    {
        allBuildingsSpawnedSerializable = new List<BuildingControllerSerializable>(); //reset the list so that multiple saves do not stack and spawn many more objects
        foreach (GameObject building in allBuildingsSpawned)
        {           
            BuildingControllerSerializable bcs = building.GetComponent<BuildingController>().GetBuildingData();
            allBuildingsSpawnedSerializable.Add(bcs);
        }
        return allBuildingsSpawnedSerializable;
    }
    #endregion
}
