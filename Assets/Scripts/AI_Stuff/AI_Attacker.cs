﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState { GoingToDefaultTarget, MovingToTarget, Attack}

public class AI_Attacker : AI_Base, IDamageable
{
    [Header("Additional AI options")]
    private IDamageable currentDamageable;
    [SerializeField] private int attackerIndex;
    [SerializeField] private GameObject defaultTargetIfNoOtherAvailable;
    [SerializeField] private AI_Stats stats;
    [SerializeField] private float attackInterval = 1.5f;
    [Tooltip("This stopping distance will be used by the agent that is attacking THIS target. This is useful when THIS target requires the agent to stop further from it (e.g big buildings)")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float defaultAgentStoppingDistance = 1;
    private float attackAnimTimestamp;
    private AIState aiState;
    private NavMeshAgent agent;
    private float currentHealth;
    

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    //    SetAgentTarget(agent, target.transform.position);
        aiState = AIState.GoingToDefaultTarget;
        defaultTargetIfNoOtherAvailable = GameObject.FindGameObjectWithTag("DefaultTargetToProtect");
        if (!Chochosan.SaveLoadManager.IsSaveExists())
        {
            SetInitialHP();
            AI_Attacker_Loader.AddAttackerToList(this);
        }
    }

    private void Update()
    {
        if (aiState == AIState.MovingToTarget)
        {
            if (currentTarget == null)
            {
                aiState = AIState.GoingToDefaultTarget;
                SetAgentDestination(agent, agent.transform.position);
                return;
            }
            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if(aiState == AIState.Attack)
        {       
            if (Time.time >= attackAnimTimestamp)
            {
                if (currentTarget != null && currentDamageable != null)
                {
                    currentDamageable.TakeDamage(stats.damage);
                    attackAnimTimestamp = Time.time + attackInterval;
                    Chochosan.ChochosanHelper.ChochosanDebug("Attack" + gameObject.name, "green");
                }
                else
                {
                    aiState = AIState.GoingToDefaultTarget;
                    Chochosan.ChochosanHelper.ChochosanDebug("NULL CAUGHT || ATTACKER", "red");
                }
            }
        }
        else if(aiState == AIState.GoingToDefaultTarget)
        {
            SetAgentDestination(agent, defaultTargetIfNoOtherAvailable.transform.position);
            agent.stoppingDistance = defaultAgentStoppingDistance;
            ChooseNewTarget(true);
        }
        

        float distance = 10000;
        if (currentTarget != null)
        {
            distance = Vector3.Distance(currentTarget.transform.position, transform.position);

            if (distance <= secondPureEnemySenseRange && distance > agent.stoppingDistance) //if player is far but scented then go to him
            {
                GoIntoMovingState();
            }
            else if (distance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                if (aiState != AIState.Attack)
                {
                    aiState = AIState.Attack;
                    attackAnimTimestamp = Time.time + attackInterval;              
                    LookAtTarget();
                }
            }
            else
            {
                GoIntoPatrolState();
            }
        }
        else
        {
            GoIntoPatrolState();
        }
    }

    private void GoIntoPatrolState()
    {
        if (aiState != AIState.GoingToDefaultTarget)
        {
            aiState = AIState.GoingToDefaultTarget;
            currentTarget = null;
            currentDamageable = null;
            SetAgentDestination(agent, agent.transform.position);
        }
    }
    
    private void GoIntoMovingState()
    {
        if(aiState != AIState.MovingToTarget)
        {
            aiState = AIState.MovingToTarget;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            if (currentDamageable != null)
                agent.stoppingDistance = currentDamageable.GetCustomAgentStoppingDistance();
            else
                agent.stoppingDistance = defaultAgentStoppingDistance;
        }
    }
    
    private void SetInitialHP()
    {
        currentHealth = stats.maxHealth;
    }

    public void SetAttackerHP(float hp)
    {
        currentHealth = hp > stats.maxHealth ? stats.maxHealth : hp;
    }

    public void SetAttackerIndex(int index)
    {
        attackerIndex = index;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            AI_Attacker_Loader.RemoveAttackerFromList(this);
            Destroy(this.gameObject);
        }
    }

    public float GetCustomAgentStoppingDistance()
    {
        return customAgentStoppingDistance;
    }

    public AI_Attacker_Serializable GetAttackerData()
    {
        AI_Attacker_Serializable acs = new AI_Attacker_Serializable();
        acs.x = transform.position.x;
        acs.y = transform.position.y;
        acs.z = transform.position.z;
        acs.currentHP = currentHealth;
        acs.attackerIndex = attackerIndex;
        return acs;
    }
}
