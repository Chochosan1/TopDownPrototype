using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum AIState_Villager { Idle, Moving, Harvesting}
public class AI_Villager : AI_Base
{
    public bool debugState = false;
    private NavMeshAgent agent;
    private Animator anim;
    private AIState_Villager aiState;
    [SerializeField] private float harvestInterval = 1.5f;
    private float harvestAnimTimestamp;
    private Harvestable_Controller currentHarvestable;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = AIState_Villager.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        ChooseNewTarget(true);
        if (aiState == AIState_Villager.Idle)
        {
            anim.SetBool("Idle", true);
            anim.SetBool("Walk", false);
            anim.SetBool("HarvestWood", false);
        }
        else if(aiState == AIState_Villager.Moving)
        {
            if(currentTarget == null)
            {
                aiState = AIState_Villager.Idle;
                SetAgentTarget(agent, agent.transform.position);
                return;
            }
            SetAgentTarget(agent, currentTarget.transform.position);
     
            anim.SetBool("Walk", true);
            anim.SetBool("HarvestWood", false);
        }
        else if(aiState == AIState_Villager.Harvesting)
        {
            if (currentTarget == null)
            {
                aiState = AIState_Villager.Idle;
                return;
            }
            anim.SetBool("Walk", false);
            anim.SetBool("HarvestWood", true);
            if(Time.time >= harvestAnimTimestamp)
            {
                currentHarvestable.Harvest();
                harvestAnimTimestamp = Time.time + harvestInterval;
            }
        }

     
        float distance = 10000;
        if (currentTarget != null)
        {
            distance = Vector3.Distance(currentTarget.transform.position, transform.position);

            if (distance <= pureEnemySenseRange && distance > agent.stoppingDistance && aiState != AIState_Villager.Harvesting) //if player is far but scented then go to him
            {
                aiState = AIState_Villager.Moving;
            }
            else if (distance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                if (aiState != AIState_Villager.Harvesting)
                {
                    aiState = AIState_Villager.Harvesting;
                    harvestAnimTimestamp = Time.time + harvestInterval;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                }
            }
            else
            {
                aiState = AIState_Villager.Idle;
            }
        }
        else
        {
            aiState = AIState_Villager.Idle;
        }

        if(debugState)
        {
            Chochosan.ChochosanHelper.ChochosanDebug("Target", currentTarget, "red");
            Chochosan.ChochosanHelper.ChochosanDebug("State", aiState, "green");
        }     
    }
}
