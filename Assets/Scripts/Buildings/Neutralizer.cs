using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neutralizer : MonoBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private LayerMask detectionLayer;
    void Awake()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, detectionLayer);

        foreach(Collider col in hitColliders)
        {
            col.GetComponent<AI_Spawner>().MarkSpawnerAsNeutralized();
        }
    }
}
