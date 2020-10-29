using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIState { Idle, GoingToDefaultTarget, MovingToTarget, Attack, MovingToArea }
public enum AttackerType { Melee, Ranged }
public class AI_Attacker : AI_Base, IDamageable, ISelectable
{
    public bool debugState, isDummy;
    private bool isSelected;
    public bool isDefaultedWorldObject;
    [Header("Additional AI options")]
    private IDamageable currentDamageable;
    [SerializeField] private AttackerType attackerType;
    [SerializeField] private bool isSelectable = false;
    [SerializeField] private int attackerIndex;
    [SerializeField] private bool isUsingDefaultTarget = false;
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

    [Header("Ranged")]
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
    private bool canExitAttackState = true;

    Vector3 direction;
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
        if(isUsingDefaultTarget)
        {
            defaultTargetIfNoOtherAvailable = GameObject.Find("TownHallNew(Clone)");
        }

        projectilePool = new List<GameObject>();
        thisTransform = transform;
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        GoToDefaultTargetState();
        if (isDefaultedWorldObject)
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
                    GoToDefaultTargetState();
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
                GoToDefaultTargetState();
              //  SetAgentDestination(agent, thisTransform.position);
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
                    if (attackerType == AttackerType.Melee)
                    {
                        currentDamageable.TakeDamage(stats.damage, this);
                    }
                    else
                    {
                        CastSpell();
                    }

                    attackAnimTimestamp = Time.time + attackInterval;
                    canExitAttackState = true;
                }
                else
                {
                    currentTarget = null;
                    currentDamageable = null;
                    currentTargetTransform = null;
                    isTargetingBuilding = false;
                    GoToDefaultTargetState();
                    Chochosan.ChochosanHelper.ChochosanDebug("NULL CAUGHT || ATTACKER", "red");
                    canExitAttackState = true;
                }
            }
        }
        else if (aiState == AIState.GoingToDefaultTarget)
        {
            if (defaultTargetIfNoOtherAvailable == null)
            {
                aiState = AIState.Idle;
                SetAgentDestination(agent, thisTransform.position);
                return;
            }
            //if ((defaultTargetIfNoOtherAvailable.transform.position - thisTransform.position).magnitude <= defaultAgentStoppingDistance)
            //{
            //  aiState = AIState.Idle;  
            //}
            SetAgentDestination(agent, defaultTargetIfNoOtherAvailable.transform.position);
            agent.stoppingDistance = defaultAgentStoppingDistance;
            ChooseNewTarget(true);
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", true);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
        }
     
        if (currentTarget != null)
        {
            float directionDistance = 10000;
            if (currentTargetTransform == null)
            {
                currentTargetTransform = currentTarget.transform;
            }

            if(isTargetingBuilding)
            { 
                //if the AI is currently targetting a building then calculate the distance based on the static destination
                direction = currentDestination - thisTransform.position;
            }
            else
            {
                //if the AI is currently NOT targetting a building then calculate the distance based on the position of the target directly
                direction = currentTargetTransform.position - thisTransform.position;
            }

            directionDistance = direction.magnitude;

            if (/*headingDistance <= secondPureEnemySenseRange &&*/ directionDistance > agent.stoppingDistance) //if player is far but scented then go to him
            {
                if (!canExitAttackState)
                    return;
                GoToMovingToTargetState();
            }
            else if (directionDistance <= agent.stoppingDistance) //if player is within stop range then go to attack state
            {
                GoToAttackState();
            }
            else
            {
                if (!canExitAttackState)
                    return;
                GoToIdleState();
            }
        }
        else
        {
            GoToIdleState();
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
            aiState = AIState.Attack;
            SetAgentDestination(agent, thisTransform.position);
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            attackAnimTimestamp = Time.time + attackInterval;
            canExitAttackState = false;
            LookAtTarget();
        }
    }

    private void GoToDefaultTargetState()
    {
        if (aiState != AIState.GoingToDefaultTarget)
        {
            canExitAttackState = true;
            agent.speed = walkSpeed;
            aiState = AIState.GoingToDefaultTarget;
            currentTarget = null;
            currentDamageable = null;
            currentTargetTransform = null;
            SetAgentDestination(agent, thisTransform.position);
        }
    }

    private void GoToIdleState()
    {
        if (aiState != AIState.Idle && aiState != AIState.GoingToDefaultTarget && aiState != AIState.MovingToArea)
        {
            canExitAttackState = true;
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

    private void GoToMovingToTargetState()
    {
        if (aiState != AIState.MovingToTarget)
        {
            canExitAttackState = true;
            agent.speed = runSpeed;
            aiState = AIState.MovingToTarget;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            if (currentDamageable != null && attackerType != AttackerType.Ranged)
                agent.stoppingDistance = currentDamageable.GetCustomAgentStoppingDistance();
            else
                agent.stoppingDistance = defaultAgentStoppingDistance;

            //if the currentTarget is a building then use "currentDestination" to move to one of its attack points
            if (currentTarget.GetComponent<BuildingController>() != null)
            {
                currentDestination = currentTarget.GetComponent<BuildingController>().GetRandomAttackSpot();
                agent.stoppingDistance = defaultAgentStoppingDistance;
                isTargetingBuilding = true;
            }
            else //if the current target is not a building then move directly to the target's transform
            {
               // currentDestination = currentTarget.transform.position;
                isTargetingBuilding = false;
            }
            currentTargetTransform = currentTarget.transform;
        }
    }

    private void GoToMovingToAreaState()
    {
        if (aiState != AIState.MovingToArea)
        {
            canExitAttackState = true;
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

    public void SetDefaultTarget(GameObject defaultedTarget)
    {
        defaultTargetIfNoOtherAvailable = defaultedTarget;
    }

    public void SetIsUsingDefaultTarget(bool value)
    {
        isUsingDefaultTarget = value;
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
        if (attacker != null && (currentTarget == null || isTargetingBuilding))
        {
            currentTarget = attacker.gameObject;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            GoToMovingToTargetState();
        }

        currentHealth -= damage;
        UpdateHealthBar(currentHealth);

        if (isSelected)
        {
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged?.Invoke(this);
        }

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
        GoToMovingToAreaState();
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
            GoToMovingToTargetState();
        }
    }

    #region RANGED
    private bool isStillSpawning = true;
    private int currentPoolItem = 0;

    private void CastSpell()
    {
        if (isStillSpawning) //if the pool is still not full
        {
            GameObject projectileCopy = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectilePrefab.transform.rotation);
            projectileCopy.GetComponent<Projectile_Controller>().SetTarget(currentTarget, this);
            AddObjectToPool(projectileCopy);
        }
        else //when full start using items from the pool
        {
            projectilePool[currentPoolItem].transform.position = projectileSpawnPoint.position;
            projectilePool[currentPoolItem].SetActive(true);
            projectilePool[currentPoolItem].GetComponent<Projectile_Controller>().SetTarget(currentTarget, this);
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
        isSelected = value;
    }

    public AI_Attacker_Serializable GetAttackerData()
    {
        AI_Attacker_Serializable acs = new AI_Attacker_Serializable();
        acs.x = transform.position.x;
        acs.y = transform.position.y;
        acs.z = transform.position.z;
        acs.currentHP = currentHealth;
        acs.attackerIndex = attackerIndex;
        acs.isUsingDefaultTarget = isUsingDefaultTarget;
        return acs;
    }
}
