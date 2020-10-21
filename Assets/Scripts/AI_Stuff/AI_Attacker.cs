﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState { Idle, GoingToDefaultTarget, MovingToTarget, Attack, MovingToArea }
public enum AttackerType { Warrior, Wizard }
public class AI_Attacker : AI_Base, IDamageable, ISelectable
{
    public bool debugState, isDummy;
    [Header("Additional AI options")]
    private IDamageable currentDamageable;
    [SerializeField] private AttackerType attackerType;
    [SerializeField] private bool isSelectable = false;
    [SerializeField] private int attackerIndex;
    [SerializeField] private GameObject defaultTargetIfNoOtherAvailable;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject deathParticle;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Transform individualUnitCanvas;
    [SerializeField] private UnityEngine.UI.Slider hpBar;
    [SerializeField] private UnityEngine.UI.Image hpBarImage;
    [SerializeField] private AI_Stats stats;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 4.5f;
    [Tooltip("This stopping distance will be used by the agent that is attacking THIS target. This is useful when THIS target requires the agent to stop further from it (e.g big buildings)")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float defaultAgentStoppingDistance = 1;
    [SerializeField] private bool isCountTowardsPopulation = false;
    private bool isDead = false;
    private bool isTargetingBuilding = false;

    [Header("Wizard")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float forwardOffset;
    [SerializeField] private int projectilePoolSize;
    private List<GameObject> projectilePool;

    private float attackAnimTimestamp;
    private AIState aiState;
    private NavMeshAgent agent;
    private Vector3 currentDestination;
    private Animator anim;
    private new SkinnedMeshRenderer renderer;
    private Camera mainCamera;
    private Transform thisTransform;
    private Transform currentTargetTransform;
    private float currentHealth;

    private void OnDisable()
    {
        if (isSelectable)
        {
            Unit_Controller.Instance.OnTryToSelectUnits -= CheckIfSelectedBySelector;
        }
    }

    void Start()
    {
        if (isSelectable)
        {
            Unit_Controller.Instance.OnTryToSelectUnits += CheckIfSelectedBySelector;
        }
        if (isCountTowardsPopulation)
        {
            PlayerInventory.Instance.CurrentFoodConsumption += stats.foodPerDayUpkeep;
            PlayerInventory.Instance.CurrentPopulation++;
        }


        projectilePool = new List<GameObject>();
        thisTransform = transform;
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = AIState.GoingToDefaultTarget;
        //  defaultTargetIfNoOtherAvailable = GameObject.FindGameObjectWithTag("DefaultTargetToProtect");
        if (!Chochosan.SaveLoadManager.IsSaveExists())
        {
            SetInitialStateNotLoadedFromSave();
        }

        hpBar.maxValue = stats.maxHealth;
        UpdateHealthBar(currentHealth);
    }

    private void Update()
    {
        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(thisTransform);
        if (aiState == AIState.Idle)
        {
            isTargetingBuilding = false;
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
            currentTarget = null;
            currentDamageable = null;
            currentTargetTransform = null;
            if (defaultTargetIfNoOtherAvailable != null)
            {
                if ((defaultTargetIfNoOtherAvailable.transform.position - thisTransform.position).magnitude > defaultAgentStoppingDistance)
                {
                    GoToDefaultTarget();
                }
            }
            ChooseNewTarget(true);
        }
        else if (aiState == AIState.MovingToArea)
        {
            if ((agent.destination - thisTransform.position).magnitude <= agent.stoppingDistance + 0.01f)
            {
                aiState = AIState.Idle;
            }
            isTargetingBuilding = false;
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", true);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
        }
        else if (aiState == AIState.MovingToTarget)
        {
            if (currentTarget == null || currentDamageable == null)
            {
                aiState = AIState.GoingToDefaultTarget;
                SetAgentDestination(agent, thisTransform.position);
                return;
            }

            if(isTargetingBuilding)
            {
                SetAgentDestination(agent, currentDestination);
                Vector3 heading = currentDestination - thisTransform.position;
                
                if(heading.magnitude <= agent.stoppingDistance)
                {
                    GoToAttackState();
                }
            }
            else
            {
                SetAgentDestination(agent, currentTargetTransform.position);
            } 
           
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", true);
            anim.SetBool("isAttack", false);
        }
        else if (aiState == AIState.Attack)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", true);
            if (Time.time >= attackAnimTimestamp)
            {
                if (currentTarget != null && currentDamageable != null)
                {
                    LookAtTarget();
                    if (attackerType == AttackerType.Warrior)
                    {
                        currentDamageable.TakeDamage(stats.damage, this);
                    }
                    else
                    {
                        CastSpell();
                    }

                    attackAnimTimestamp = Time.time + attackInterval;
                }
                else
                {
                    currentTarget = null;
                    currentDamageable = null;
                    currentTargetTransform = null;
                    isTargetingBuilding = false;
                    GoToDefaultTarget();
                    Chochosan.ChochosanHelper.ChochosanDebug("NULL CAUGHT || ATTACKER", "red");
                }
            }
        }
        else if (aiState == AIState.GoingToDefaultTarget)
        {
            if (defaultTargetIfNoOtherAvailable == null)
            {
                aiState = AIState.Idle;
                return;
            }
            if ((defaultTargetIfNoOtherAvailable.transform.position - thisTransform.position).magnitude <= defaultAgentStoppingDistance)
            {
                aiState = AIState.Idle;
            }
            SetAgentDestination(agent, defaultTargetIfNoOtherAvailable.transform.position);
            agent.stoppingDistance = defaultAgentStoppingDistance;
            ChooseNewTarget(true);
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", true);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
        }


        float headingDistance = 10000;
        if (currentTarget != null)
        {
            if(currentTargetTransform == null)
            {
                currentTargetTransform = currentTarget.transform;
            }
            Vector3 heading;
            if(isTargetingBuilding)
            { 
                //if the AI is currently targetting a building then calculate the distance based on the static destination
                heading = currentDestination - thisTransform.position;
            }
            else
            {
                //if the AI is currently NOT targetting a building then calculate the distance based on the position of the target directly
                heading = currentTargetTransform.position - thisTransform.position;
            }
            // Vector3 heading = currentTarget.transform.position - thisTransform.position;
            
            headingDistance = heading.magnitude;

            if (/*headingDistance <= secondPureEnemySenseRange &&*/ headingDistance > agent.stoppingDistance) //if player is far but scented then go to him
            {
                GoIntoMovingToTarget();
            }
            else if (headingDistance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                GoToAttackState();
            }
            else
            {
                GoToIdle();
            }
        }
        else
        {
            GoToIdle();
        }

        if (debugState)
        {
            if(isTargetingBuilding)
            {
                Debug.Log("BUILDING TARGET");
            }
            else
            {
                Debug.Log("NON BUILDING TARGET");
            }
            // Chochosan.ChochosanHelper.ChochosanDebug(aiState.ToString(), "red");
        //    Debug.Log(agent.pathStatus);
        }
        if (isDummy)
        {
            agent.speed = 0;
        }
    }

    private void GoToAttackState()
    {
        if (aiState != AIState.Attack)
        {
            Debug.Log("ATTACK");
            aiState = AIState.Attack;
            SetAgentDestination(agent, thisTransform.position);
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            attackAnimTimestamp = Time.time + attackInterval;
            LookAtTarget();
        }
    }

    private void GoToDefaultTarget()
    {
        if (aiState != AIState.GoingToDefaultTarget)
        {
            agent.speed = walkSpeed;
            aiState = AIState.GoingToDefaultTarget;
            currentTarget = null;
            currentDamageable = null;
            currentTargetTransform = null;
            SetAgentDestination(agent, thisTransform.position);
        }
    }

    private void GoToIdle()
    {
        if (aiState != AIState.Idle && aiState != AIState.GoingToDefaultTarget && aiState != AIState.MovingToArea)
        {
            agent.speed = walkSpeed;
            aiState = AIState.Idle;
            currentTarget = null;
            currentDamageable = null;
            currentTargetTransform = null;
            SetAgentDestination(agent, thisTransform.position);
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
        }
    }

    private void GoIntoMovingToTarget()
    {
        if (aiState != AIState.MovingToTarget)
        {
            Debug.Log("GOTOTARGET");
            agent.speed = runSpeed;
            aiState = AIState.MovingToTarget;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            if (currentDamageable != null && attackerType != AttackerType.Wizard)
                agent.stoppingDistance = currentDamageable.GetCustomAgentStoppingDistance();
            else
                agent.stoppingDistance = defaultAgentStoppingDistance;

            if (currentTarget.GetComponent<BuildingController>() != null)
            {
                currentDestination = currentTarget.GetComponent<BuildingController>().GetRandomAttackSpot();
                agent.stoppingDistance = defaultAgentStoppingDistance;
                isTargetingBuilding = true;
            }
            else
            {
                currentDestination = currentTarget.transform.position;
                isTargetingBuilding = false;
            }
            currentTargetTransform = currentTarget.transform;
        }
    }

    private void GoToMovingToArea()
    {
        if (aiState != AIState.MovingToArea)
        {
            aiState = AIState.MovingToArea;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0;
            currentTarget = null;
            currentDamageable = null;
            currentTargetTransform = null;
        }
    }

    //set the initial stats of the unit and add it to the save list, only do that for a not loaded from a save unit
    //a.k.a for defaulted world units and for runtime spawning of units
    public void SetInitialStateNotLoadedFromSave()
    {
        SetInitialHP();
        AI_Attacker_Loader.AddAttackerToList(this);
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

    public void TakeDamage(float damage, AI_Attacker attacker)
    {
        if (attacker != null && currentTarget == null)
        {
            currentTarget = attacker.gameObject;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
        }
        currentHealth -= damage;
        UpdateHealthBar(currentHealth);

        //enable particle if not active then disable after some time
        if (!hitParticle.activeSelf)
        {
            hitParticle.SetActive(true);
            StartCoroutine(DisableHitParticle());
        }
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Instantiate(deathParticle, thisTransform.position, deathParticle.transform.rotation);
            AI_Attacker_Loader.RemoveAttackerFromList(this);
            if (isCountTowardsPopulation)
            {
                PlayerInventory.Instance.CurrentPopulation--;
                PlayerInventory.Instance.CurrentFoodConsumption -= stats.foodPerDayUpkeep;
            }

            Destroy(this.gameObject);
        }
    }

    private void UpdateHealthBar(float value)
    {
        if (hpBar == null)
            return;

        hpBar.value = value;
        if(hpBar.value <= hpBar.maxValue * 0.5f)
        {
            hpBarImage.color = Color.red;
        }
        else
        {
            hpBarImage.color = Color.green;
        }
    }

    private IEnumerator DisableHitParticle()
    {
        yield return new WaitForSeconds(0.3f);
        hitParticle.SetActive(false);
    }

    public float GetCustomAgentStoppingDistance()
    {
        return customAgentStoppingDistance;
    }

    public void ForceSetAgentArea(Vector3 destination)
    {
        GoToMovingToArea();
        agent.destination = destination;
    }

    public string GetSelectedUnitInfo()
    {
        return unitName;
    }

    public bool IsOpenUpgradePanel()
    {
        return false;
    }

    public void UpgradeUnit()
    {
        Debug.Log("Not upgradeable");
    }

    public void ForceSetSpecificTarget(GameObject target)
    {
        if (target.CompareTag("Enemy"))
        {
            currentTarget = target;
            GoIntoMovingToTarget();
        }
    }

    #region WIZARD
    private bool isStillSpawning = true;
    private int currentPoolItem = 0;

    private void CastSpell()
    {
        if (isStillSpawning) //if the pool is still not full
        {
            GameObject projectileCopy = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectilePrefab.transform.rotation);
            projectileCopy.GetComponent<Projectile_Controller>().SetTarget(currentTarget);
            AddObjectToPool(projectileCopy);
        }
        else //when full start using items from the pool
        {
            projectilePool[currentPoolItem].transform.position = projectileSpawnPoint.position;
            projectilePool[currentPoolItem].SetActive(true);
            projectilePool[currentPoolItem].GetComponent<Projectile_Controller>().SetTarget(currentTarget);
            currentPoolItem++;

            if (currentPoolItem >= projectilePool.Count)
            {
                currentPoolItem = 0;
            }
        }
    }

    //add objects to the pool during runtime while instantiating objects, when a certain limit is reached then disable instantiating
    //and start using the pool
    private void AddObjectToPool(GameObject objectToAdd)
    {
        projectilePool.Add(objectToAdd);

        if (projectilePool.Count >= projectilePoolSize)
        {
            isStillSpawning = false;
        }
    }
    #endregion

    public void CheckIfSelectedBySelector()
    {
        if (renderer.isVisible)
        {
            Vector3 camPos = mainCamera.WorldToScreenPoint(thisTransform.position);
            camPos.y = Unit_Controller.Instance.InvertMouseY(camPos.y);
            if (Unit_Controller.Instance.selectRect.Contains(camPos))
            {
                Unit_Controller.Instance.AddUnitToSelectedList(this.gameObject);
                ToggleSelectedIndicator(true);
            }
            else
            {
                ToggleSelectedIndicator(false);
            }
        }
    }

    public void ToggleSelectedIndicator(bool value)
    {
        selectedIndicator?.SetActive(value);
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
