using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

   private int layerMask;
    


    private void Start()
    {
        layerMask = LayerMask.GetMask("Terrain");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            CancelObject();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentObjectIndex = 0;
            InstantiateObjectAtMousePos(previewObjects[currentObjectIndex]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentObjectIndex = 1;
            InstantiateObjectAtMousePos(previewObjects[currentObjectIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (currentObject != null)
            {
                SpawnObjectAtWorld();
            }
        }

        MoveCurrentObjectWithMouse();

        if(Input.GetKey(KeyCode.LeftControl))
        {
            RotateCurrentObject();
        }
    }

    public void SpawnObject(int index)
    {
        currentObjectIndex = index;
        InstantiateObjectAtMousePos(previewObjects[currentObjectIndex]);
    }

    private void InstantiateObjectAtMousePos(GameObject objectToSpawn)
    {
        if (currentObject == null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {
                currentObject = Instantiate(objectToSpawn, hit.point, transform.rotation);
                Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(true);
                colCheckers = currentObject.GetComponentsInChildren<CollisionChecker>();
                Debug.Log(colCheckers);
            }
        }
    }

    private void MoveCurrentObjectWithMouse()
    {
        if (currentObject != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, layerMask))
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

            Instantiate(realObjects[currentObjectIndex], currentObject.transform.position, currentObject.transform.rotation);
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }

    private void RotateCurrentObject()
    {
        if(currentObject != null)
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

    private void CancelObject()
    {
        if(currentObject != null)
        {
            Chochosan.UI_Manager.Instance.ToggleObjectManipulationInfo(false);
            Destroy(currentObject);
            currentObject = null;
            colCheckers = null;
        }
    }
}
