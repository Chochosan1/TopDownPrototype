﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Provides basic AI functionalities that other objects build upon.
/// </summary>

public class AI_Base : MonoBehaviour
{
    protected GameObject currentTarget;

    [Header("Base AI")]
    [SerializeField] protected string unitName;
    [Tooltip("The layer which can be detected by the AI unit. All other layers will be ignored.")]
    [SerializeField] protected LayerMask enemyLayer;
    [Tooltip("How far away can the AI unit detect its target without taking into consideration walls and such. Will be used before looking for target within the greater range. ")]
    [SerializeField] protected float firstPureEnemySenseRange = 5f;
    [Tooltip("How far away can the AI unit detect its target without taking into consideration walls and such. Will be used after first looking for targets within the smaller range. ")]
    [SerializeField] protected float secondPureEnemySenseRange = 30f;

    protected virtual void SetAgentDestination(NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }

    protected virtual Collider[] GetAllInRange(Vector3 offset)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + offset, firstPureEnemySenseRange, enemyLayer);

        return hitColliders;
    }

    //choose a random target out of all detected targets in a layer OR if the parameter is false choose the first target always
    protected virtual void ChooseNewTarget(bool chooseRandom)
    {
        if (currentTarget == null)
        {
            Collider[] firstHitColliders = Physics.OverlapSphere(transform.position, firstPureEnemySenseRange, enemyLayer);
            if (firstHitColliders.Length > 0)
            {
                if (chooseRandom)
                {
                    int randomIndex = Random.Range(0, firstHitColliders.Length);
                    currentTarget = firstHitColliders[randomIndex].gameObject;
                }
                else
                {
                    currentTarget = firstHitColliders[0].gameObject;
                }
                //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
            }
            else
            {
                Collider[] secondHitColliders = Physics.OverlapSphere(transform.position, secondPureEnemySenseRange, enemyLayer);
                if (secondHitColliders.Length > 0)
                {
                    if (chooseRandom)
                    {
                        int randomIndex = Random.Range(0, secondHitColliders.Length);
                        currentTarget = secondHitColliders[randomIndex].gameObject;
                    }
                    else
                    {
                        currentTarget = secondHitColliders[0].gameObject;
                    }
                    //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
                }
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
