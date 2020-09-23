using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls unit selection and commanding. Controllable units must be in a specific layer and inherit the necessary IMovable interface.
/// </summary>
public class Unit_Controller : MonoBehaviour
{
    [SerializeField] private LayerMask selectableUnitLayer;
    [SerializeField] private LayerMask movableAreaLayer;
    private ObjectSpawner objectSpawner;
    private Camera mainCamera;
    private GameObject currentlySelectedUnit;

    // Start is called before the first frame update
    void Start()
    {
        objectSpawner = GetComponent<ObjectSpawner>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ClearCurrentlySelectedUnit();
        }


        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(!objectSpawner.IsCurrentlySpawningBuilding())
            {
                if (currentlySelectedUnit == null)
                {
                    SelectUnit();
                }
                else
                {
                    CommandSelectedUnit();
                }
            }
        }
    }

    #region UnitControl
    private void SelectUnit()
    {
        if (!objectSpawner.IsCurrentlySpawningBuilding())
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, selectableUnitLayer))
            {
                currentlySelectedUnit = hit.collider.gameObject;
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
                if(movableAreaLayer == (movableAreaLayer | (1 << hit.collider.gameObject.layer))) //check if the object is in the specific layer
                {
                    ISelectable tempSelectable = currentlySelectedUnit.GetComponent<ISelectable>();
                    tempSelectable?.ForceSetAgentArea(hit.point);
                    Debug.Log("COMMAND");
                }      
            }
        }
    }

    private void ClearCurrentlySelectedUnit()
    {
        currentlySelectedUnit = null;
        Debug.Log("DESELECT");
    }
    #endregion
}
