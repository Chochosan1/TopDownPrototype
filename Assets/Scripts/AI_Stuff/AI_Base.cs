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
    [Tooltip("The layer which can be detected by the AI unit. All other layers will be ignored.")]
    [SerializeField] protected LayerMask enemyLayer;
    [Tooltip("How far away can the AI unit detect its target without taking into consideration walls and such. ")]
    [SerializeField] protected float pureEnemySenseRange;

    protected virtual void SetAgentTarget(NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }


    //choose a random target out of all detected targets in a layer OR if the parameter is false choose the first target always
    protected virtual void ChooseNewTarget(bool chooseRandom)
    {
        if (currentTarget == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pureEnemySenseRange, enemyLayer);
            if (hitColliders.Length > 0)
            {
                if (chooseRandom)
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

    //Look towards the target
    protected virtual void LookAtTarget()
    {
        if (currentTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentTarget.transform.position - transform.position);
            targetRotation.x = transform.rotation.x;
            targetRotation.z = transform.rotation.z;
            transform.rotation = targetRotation;
        }
    }
}
