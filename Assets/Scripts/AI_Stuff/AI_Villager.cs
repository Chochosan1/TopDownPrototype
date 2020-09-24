using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Villager type AI unit that can harvest materials.
/// </summary>
public enum AIState_Villager { Idle, MovingToSpecificTarget, Harvesting, MovingToArea}
public class AI_Villager : AI_Base, ISelectable
{
    public bool debugState = false;
    private NavMeshAgent agent;
    private Animator anim;
    private AIState_Villager aiState;
    [Tooltip("How often should the villager loot resource from the harvestable object. Best way is to match it with the animation.")]
    [SerializeField] private float harvestInterval = 1.5f;
    private float harvestAnimTimestamp;
    private Harvestable_Controller currentHarvestable;
    private BuildingController buildingController;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = AIState_Villager.Idle;
    }

    void Update()
    {
        if (aiState == AIState_Villager.Idle)
        {
            anim.SetBool("Idle", true);
            anim.SetBool("Walk", false);
            anim.SetBool("HarvestWood", false);
            ChooseNewTarget(true);
        }
        else if(aiState == AIState_Villager.MovingToSpecificTarget) //if a specific target has been chosen by the AI
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
        else if(aiState == AIState_Villager.MovingToArea) //if set to move to a specific area without having a specific target
        {
            if(Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                GoIntoIdleState();
            }
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
                if(currentHarvestable != null)
                {
                    currentHarvestable.Harvest();
                    harvestAnimTimestamp = Time.time + harvestInterval;
                }
                else
                {
                    aiState = AIState_Villager.Idle;
                    Chochosan.ChochosanHelper.ChochosanDebug("NULL CAUGHT", "red");
                }             
            }
        }

     
        float distance = 10000;
        if (currentTarget != null)
        {
            distance = Vector3.Distance(currentTarget.transform.position, transform.position);

            if (distance <= secondPureEnemySenseRange && distance > agent.stoppingDistance && aiState != AIState_Villager.Harvesting) //if player is far but scented then go to him
            {
                aiState = AIState_Villager.MovingToSpecificTarget;
            }
            else if (distance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                if (aiState != AIState_Villager.Harvesting)
                {
                    aiState = AIState_Villager.Harvesting;
                    harvestAnimTimestamp = Time.time + harvestInterval;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                    LookAtTarget();
                }
            }
            else
            {
                GoIntoIdleState();         
            }
        }
        else
        {
            if(aiState != AIState_Villager.MovingToArea)
                GoIntoIdleState();     
        }

        if(debugState)
        {
            Chochosan.ChochosanHelper.ChochosanDebug("Target", currentTarget, "red");
            Chochosan.ChochosanHelper.ChochosanDebug("State", aiState, "green");
        }     
    }

    private void GoIntoIdleState()
    {
        if (aiState != AIState_Villager.Idle)
        {
            aiState = AIState_Villager.Idle;
            currentTarget = null;
            currentHarvestable = null;
            SetAgentTarget(agent, agent.transform.position);
        }
    }

    //send agent to a certain location; used mainly for select -> click to send mechanic
    public void ForceSetAgentArea(Vector3 destination)
    {
        aiState = AIState_Villager.MovingToArea;
        currentTarget = null;
        currentHarvestable = null;
        agent.destination = destination;
    }

    public string GetSelectedUnitInfo()
    {
        return unitName;
    }

    public void SetHomeBuilding(BuildingController buildingController)
    {
        this.buildingController = buildingController;
    }

    public bool IsOpenUpgradePanel()
    {
        return false;
    }

    public void UpgradeUnit()
    {
        Debug.Log("This unit cannot be upgraded");
    }
}
