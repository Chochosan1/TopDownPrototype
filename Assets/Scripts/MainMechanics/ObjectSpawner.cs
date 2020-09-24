using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls world spawning of buildings using the mouse.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] previewObjects;
    [SerializeField] private GameObject[] realObjects;
    [SerializeField] private float rotationSpeed;
    [Tooltip("If set to true, the object will match the surface rotation.")]
    [SerializeField] private bool is_ObjectSlopeRotationMatch;
    private GameObject currentObject;
    private int currentObjectIndex;
    private CollisionChecker[] colCheckers;
    private int buildableLayerMask;
    private Camera mainCamera;

    //event subscribed to for example in PlayerInventory in order to spend resources 
    public delegate void OnObjectSpawnedAtWorldDelegate(SO_CostRequirements requirements);
    public event OnObjectSpawnedAtWorldDelegate OnObjectSpawnedAtWorld;

    private void Start()
    {
        buildableLayerMask = LayerMask.GetMask("Terrain");
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            CancelObject();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (currentObject != null)
            {
                SpawnObjectAtWorld();
            }
        }

        MoveCurrentObjectWithMouse();

        if (Input.GetKey(KeyCode.LeftControl))
        {
            RotateCurrentObject();
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
    private void SpawnObjectAtWorld()
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
            ISpawnedAtWorld tempInterface = tempObject.GetComponent<ISpawnedAtWorld>();
            if (tempInterface != null)
                tempObject.GetComponent<ISpawnedAtWorld>().StartInitialSetup();
            OnObjectSpawnedAtWorld?.Invoke(currentObject.GetComponent<RequirementsToBuild>().GetRequirements());
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }

    private void RotateCurrentObject()
    {
        if (currentObject != null)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                currentObject.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime, Space.Self);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                currentObject.transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime, Space.Self);
            }
        }
    }

    //remove the preview object
    private void CancelObject()
    {
        if (currentObject != null)
        {
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }

    public bool IsCurrentlySpawningBuilding()
    {
        return currentObject != null;
    }
}
