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

    public delegate void OnUnitSelectedDelegate(ISelectable unitSelectable);
    public event OnUnitSelectedDelegate OnUnitSelected;

    public delegate void OnUnitDeselectedDelegate();
    public event OnUnitDeselectedDelegate OnUnitDeselected;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

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
                 SelectUnit();
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
                OnUnitSelected?.Invoke(tempSelectable);
                Debug.Log(hit.collider.gameObject);
            }
            else if(Physics.Raycast(ray, out hit, 100, selectableBuildingLayer))
            {
                currentlySelectedUnit = hit.collider.gameObject;
                tempSelectable = currentlySelectedUnit.GetComponent<ISelectable>();
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
                if(movableAreaLayer == (movableAreaLayer | (1 << hit.collider.gameObject.layer))) //check if the object is in the specific layer
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
    #endregion
}
