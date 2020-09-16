using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    [SerializeField] private LayerMask layersToExclude;
    private bool isSpawningAllowed;
    private MeshRenderer meshRend;
    private Material[] materials;

    private void Start()
    {
        meshRend = GetComponent<MeshRenderer>();
        materials = meshRend.materials;
        isSpawningAllowed = true;
        meshRend.material.SetColor("_BaseColor", Color.green);
    }

    private void OnTriggerStay(Collider other)
    {
        //Check if there is an object blocking the spawning of the desired object, Terrain and PreviewObjects layers should be ignored
        if (/*other.gameObject.layer != LayerMask.NameToLayer("Terrain") &&*/ other.gameObject.layer != LayerMask.NameToLayer("AllowedPreviewCollisions"))
        {
            isSpawningAllowed = false;
            meshRend.material.SetColor("_BaseColor", Color.red);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Check if the desired object left collision with a blocking object, Terrain and PreviewObjects layers should be ignored
        if (/*other.gameObject.layer != LayerMask.NameToLayer("Terrain") &&*/ other.gameObject.layer != LayerMask.NameToLayer("AllowedPreviewCollisions"))
        {
            isSpawningAllowed = true;
            meshRend.material.SetColor("_BaseColor", Color.green);
        }
    }

    public bool CheckIsSpawningAllowed()
    {
        return isSpawningAllowed;
    }
}
