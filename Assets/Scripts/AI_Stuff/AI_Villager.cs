using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Villager type AI unit that can harvest materials.
/// </summary>
public enum AIState_Villager { Idle, MovingToSpecificTarget, Harvesting, MovingToArea}
public enum Villager_Type { WoodWorker, GoldWorker, IronWorker, FoodWorker, Builder }
public class AI_Villager : AI_Base, ISelectable
{
    public bool debugState = false;
    private NavMeshAgent agent;
    private Animator anim;
    private AIState_Villager aiState;
    [SerializeField] private Villager_Type villagerType;
    [Tooltip("How often should the villager loot resource from the harvestable object. Best way is to match it with the animation.")]
    [SerializeField] private float harvestInterval = 1.5f;
    private float harvestAnimTimestamp;
    private Harvestable_Controller currentHarvestable;
    private BuildingController buildingController;
    private Transform thisTransform;

    [Tooltip("Use for defaulted by the level objects. For example, if the level must started with 1 villager, mark the villager to force save so that next time the game loads he'll still be there.")]
    [SerializeField] private bool defaultedWorldObject = false;

    void Start()
    {
        //if it is a defaulted world object
        if(defaultedWorldObject)
        {
            Unit_Controller.Instance.AddVillagerToList(gameObject.GetComponent<AI_Villager>()); //add to the list with spawned objects

            //all defaulted objects are parented to an object that gets deleted if there is an existing save
            //however navmesh agents break if they are parented so if they exist (parent object not deleted) it's necessary to remove their parent 
            gameObject.transform.SetParent(null); 
        }
        thisTransform = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = AIState_Villager.Idle;
        SwitchVillagerType(villagerType);
        PlayerInventory.Instance.CurrentPopulation++;   
    }

    void Update()
    {
        if (aiState == AIState_Villager.Idle)
        {
            currentTarget = null;
            currentHarvestable = null;
            anim.SetBool("Idle", true);
            anim.SetBool("Walk", false);
            anim.SetBool("HarvestWood", false);
            anim.SetBool("Build", false);
            ChooseNewTarget(true);
        }
        else if (aiState == AIState_Villager.MovingToSpecificTarget) //if a specific target has been chosen by the AI
        {
            if (currentTarget == null || currentHarvestable == null)
            {
                aiState = AIState_Villager.Idle;
                SetAgentDestination(agent, agent.transform.position);
                return;
            }
            SetAgentDestination(agent, currentTarget.transform.position);

            anim.SetBool("Walk", true);
            anim.SetBool("HarvestWood", false);
            anim.SetBool("Build", false);
        }
        else if (aiState == AIState_Villager.MovingToArea) //if set to move to a specific area without having a specific target
        {
            if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
            {
                GoIntoIdleState();
            }
            anim.SetBool("Walk", true);
            anim.SetBool("HarvestWood", false);
            anim.SetBool("Build", false);
        }
        else if (aiState == AIState_Villager.Harvesting)
        {
            if (currentTarget == null || currentHarvestable == null)
            {
                aiState = AIState_Villager.Idle;
                return;
            }
            if (Time.time >= harvestAnimTimestamp)
            {
                if (currentHarvestable != null)
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
            distance = Vector3.Distance(currentTarget.transform.position, thisTransform.position);

            if (distance <= secondPureEnemySenseRange && distance > agent.stoppingDistance && aiState != AIState_Villager.Harvesting) //if player is far but scented then go to him
            {
                if(aiState != AIState_Villager.MovingToSpecificTarget)
                {
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentHarvestable = currentTarget.GetComponentInChildren<Harvestable_Controller>();
                    if(currentHarvestable != null)
                    {
                        agent.stoppingDistance = currentHarvestable.customStoppingDistance;
                    }
                }             
            }
            else if (distance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                if (aiState != AIState_Villager.Harvesting)
                {
                    aiState = AIState_Villager.Harvesting;
                    harvestAnimTimestamp = Time.time + harvestInterval;
                    currentHarvestable = currentTarget.GetComponentInChildren<Harvestable_Controller>();
                    switch (villagerType)
                    {
                        case Villager_Type.WoodWorker:
                            anim.SetBool("Walk", false);
                            anim.SetBool("Build", false);
                            anim.SetBool("HarvestWood", true);
                            break;
                        case Villager_Type.GoldWorker:
                            anim.SetBool("Walk", false);
                            anim.SetBool("Build", false);
                            anim.SetBool("HarvestWood", true);
                            break;
                        case Villager_Type.IronWorker:
                            anim.SetBool("Walk", false);
                            anim.SetBool("Build", false);
                            anim.SetBool("HarvestWood", true);
                            break;
                        case Villager_Type.FoodWorker:
                            anim.SetBool("Walk", false);
                            anim.SetBool("Build", false);
                            anim.SetBool("HarvestWood", true);
                            break;
                        case Villager_Type.Builder:
                            anim.SetBool("Walk", false);
                            anim.SetBool("HarvestWood", false);
                            anim.SetBool("Build", true);
                            break;
                    }
                    LookAtTarget();
                }
            }
            else
            {
                if(currentTarget == null)
                {
                    GoIntoIdleState();
                }             
            }
        }
        else
        {
            if (aiState != AIState_Villager.MovingToArea)
                GoIntoIdleState();
        }

        if (debugState)
        {
          //  Chochosan.ChochosanHelper.ChochosanDebug("Target", currentTarget, "red");
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
            SetAgentDestination(agent, agent.transform.position);
        }
    }

    //sets the current type of the villager; also swaps the used layer for harvestable detection
    public void SwitchVillagerType(Villager_Type newVillagerType)
    {
        villagerType = newVillagerType;

        switch (villagerType)
        {
            case Villager_Type.WoodWorker:
                enemyLayer = LayerMask.GetMask("Wood");
                unitName = "Wood cutter";
                break;
            case Villager_Type.GoldWorker:
                enemyLayer = LayerMask.GetMask("Gold");
                unitName = "Gold miner";
                break;
            case Villager_Type.IronWorker:
                enemyLayer = LayerMask.GetMask("Iron");
                unitName = "Iron miner";
                break;
            case Villager_Type.FoodWorker:
                enemyLayer = LayerMask.GetMask("Food");
                unitName = "Food gatherer";
                break;
            case Villager_Type.Builder:              
                enemyLayer = LayerMask.GetMask("BuildingInProgress");
                unitName = "Builder";
                break;
        }
        //send a message that a displayable UI value has been changed (in this case the name of the villager)
        Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);
    }


    //sets the current type of the villager; also swaps the used layer for harvestable detection
    public void SwitchVillagerType(string newVillagerType)
    {
        switch (newVillagerType)
        {
            case "Wood":
                enemyLayer = LayerMask.GetMask("Wood");
                villagerType = Villager_Type.WoodWorker;
                unitName = "Wood cutter";
                break;
            case "Gold":
                enemyLayer = LayerMask.GetMask("Gold");
                villagerType = Villager_Type.GoldWorker;
                unitName = "Gold miner";
                break;
            case "Iron":
                enemyLayer = LayerMask.GetMask("Iron");
                villagerType = Villager_Type.IronWorker;
                unitName = "Iron miner";
                break;
            case "Food":
                enemyLayer = LayerMask.GetMask("Food");
                villagerType = Villager_Type.FoodWorker;
                unitName = "Food gatherer";
                break;
            case "Builder":
                enemyLayer = LayerMask.GetMask("BuildingInProgress");
                villagerType = Villager_Type.Builder;
                unitName = "Builder";                          
                break;
        }
    }

    public Villager_Type GetCurrentVillagerType()
    {
        return villagerType;
    }

    //send agent to a certain location; used mainly for select -> click to send mechanic
    public void ForceSetAgentArea(Vector3 destination)
    {
        aiState = AIState_Villager.MovingToArea;
        agent.stoppingDistance = 0;
        currentTarget = null;
        currentHarvestable = null;
        agent.destination = destination;
    }

    //if the clicked object fits certain tags then the object becomes the currentTarget
    public void ForceSetSpecificTarget(GameObject target)
    {
        switch(target.tag)
        {
            case "Wood":
                if (Progress_Manager.Instance.IsWoodHarvestingUnlocked())
                {
                    SwitchVillagerType(Villager_Type.WoodWorker);
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentTarget = target;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                    agent.stoppingDistance = target.GetComponent<Harvestable_Controller>().customStoppingDistance;
                }
                else
                    Chochosan.ChochosanHelper.ChochosanDebug("Woodharvesting locked!", "red");
                break;
            case "Gold":
                if (Progress_Manager.Instance.IsGoldHarvestingUnlocked())
                {
                    SwitchVillagerType(Villager_Type.GoldWorker);
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentTarget = target;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                    agent.stoppingDistance = target.GetComponent<Harvestable_Controller>().customStoppingDistance;
                }
                else
                    Chochosan.ChochosanHelper.ChochosanDebug("Goldharvesting locked!", "red");
                break;
            case "Iron":
                if (Progress_Manager.Instance.IsIronHarvestingUnlocked())
                {
                    SwitchVillagerType(Villager_Type.IronWorker);
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentTarget = target;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                    agent.stoppingDistance = target.GetComponent<Harvestable_Controller>().customStoppingDistance;
                }
                else
                    Chochosan.ChochosanHelper.ChochosanDebug("Ironharvesting locked!", "red");
                break;
            case "Food":
                if (Progress_Manager.Instance.IsFoodHarvestingUnlocked())
                {
                    SwitchVillagerType(Villager_Type.FoodWorker);
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentTarget = target;
                    currentHarvestable = currentTarget.GetComponent<Harvestable_Controller>();
                    agent.stoppingDistance = target.GetComponent<Harvestable_Controller>().customStoppingDistance;
                }
                else
                    Chochosan.ChochosanHelper.ChochosanDebug("Foodharvesting locked!", "red");
                break;
            //case "BuildingInProgress":
            //    SwitchVillagerType(Villager_Type.Builder);
            //    aiState = AIState_Villager.MovingToSpecificTarget;
            //    currentTarget = target;
            //    agent.stoppingDistance = target.GetComponent<Harvestable_Controller>().customStoppingDistance;
            //    break;
            case "SelectableBuilding":
                if(!target.GetComponent<BuildingController>().GetIsBuildingComplete())
                {
                    SwitchVillagerType(Villager_Type.Builder);
                    aiState = AIState_Villager.MovingToSpecificTarget;
                    currentTarget = target;
                    currentHarvestable = currentTarget.GetComponentInChildren<Harvestable_Controller>();
                    agent.stoppingDistance = currentHarvestable.customStoppingDistance;
                }          
                break;
        }
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
