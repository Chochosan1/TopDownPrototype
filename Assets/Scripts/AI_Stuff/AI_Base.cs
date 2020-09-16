using System.Collections;
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
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float pureEnemySenseRange;

    protected void SetAgentTarget(NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }

    protected void ChooseNewTarget()
    {
        if (currentTarget == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, pureEnemySenseRange, enemyLayer);
            if (hitColliders.Length > 0)
            {
                currentTarget = hitColliders[0].gameObject;
                Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
            }
        }
    }
}
