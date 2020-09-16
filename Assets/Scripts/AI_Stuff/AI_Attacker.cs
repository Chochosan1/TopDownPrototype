using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState { Patrol, MovingToTarget, Attack}

[RequireComponent(typeof(NavMeshAgent))]
public class AI_Attacker : AI_Base, IDamageable
{
    [Header("Additional AI options")]
    [SerializeField] private GameObject target;
    [SerializeField] private AI_Stats stats;
    private AIState aiState;
    private NavMeshAgent agent;
    private float currentHealth;

  

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    //    SetAgentTarget(agent, target.transform.position);
        aiState = AIState.Patrol;
        currentHealth = stats.maxHealth;
    }

    private void Update()
    {
        if(aiState == AIState.MovingToTarget)
        {
            SetAgentTarget(agent, currentTarget.transform.position);
          //  Chochosan.ChochosanHelper.ChochosanDebug("MOVING", "green");
        }
        else if(aiState == AIState.Attack)
        {
        //    Chochosan.ChochosanHelper.ChochosanDebug("Attack", "red");
        }
        ChooseNewTarget();

        float distance = 10000;
        if (currentTarget != null)
        {
            distance = Vector3.Distance(currentTarget.transform.position, transform.position);

            if (distance <= pureEnemySenseRange && distance > agent.stoppingDistance && aiState != AIState.Attack) //if player is far but scented then go to him
            {
                aiState = AIState.MovingToTarget;              

            }
            else if (distance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                aiState = AIState.Attack;          
            }
        }    
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
