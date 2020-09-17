using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Provides basic AI functionalities that other objects build upon.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AI_Base : MonoBehaviour
{
    protected GameObject currentTarget;
    [Header("Base AI")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float pureEnemySenseRange;

    protected void SetAgentTarget(NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }


    //choose a random target out of all detected targets in a layer OR if the parameter is false choose the first target always
    protected void ChooseNewTarget(bool chooseRandom)
    {
        if (currentTarget == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pureEnemySenseRange, enemyLayer);
            if (hitColliders.Length > 0)
            {
                if(chooseRandom)
                {
                    int randomIndex = Random.Range(0, hitColliders.Length);
                    currentTarget = hitColliders[randomIndex].gameObject;
                }
                else
                {
                    currentTarget = hitColliders[0].gameObject;
                }          
              //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
            }
        }
    }
}
