using System.Collections;
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
    [SerializeField] private AI_Stats stats;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 4.5f;
    [Tooltip("This stopping distance will be used by the agent that is attacking THIS target. This is useful when THIS target requires the agent to stop further from it (e.g big buildings)")]
    [SerializeField] private float customAgentStoppingDistance;
    [SerializeField] private float defaultAgentStoppingDistance = 1;

    [Header("Wizard")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootForce;
    [SerializeField] private Vector3 offsetVector;
    [SerializeField] private int projectilePoolSize;
    private List<GameObject> projectilePool;

    private float attackAnimTimestamp;
    private AIState aiState;
    private NavMeshAgent agent;
    private Animator anim;
    private new SkinnedMeshRenderer renderer;
    private Camera mainCamera;
    private Transform thisTransform;
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
        if(attackerType == AttackerType.Wizard)
        {
            defaultAgentStoppingDistance *= 12f;
        }
        thisTransform = transform;
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = AIState.GoingToDefaultTarget;
        //  defaultTargetIfNoOtherAvailable = GameObject.FindGameObjectWithTag("DefaultTargetToProtect");
        if (!Chochosan.SaveLoadManager.IsSaveExists())
        {
            SetInitialHP();
            AI_Attacker_Loader.AddAttackerToList(this);
        }
    }

    private void Update()
    {
        if (aiState == AIState.Idle)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isAttack", false);
            currentTarget = null;
            currentDamageable = null;
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
            SetAgentDestination(agent, currentTarget.transform.position);
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
                    currentDamageable.TakeDamage(stats.damage, this);
                    attackAnimTimestamp = Time.time + attackInterval;
                    Chochosan.ChochosanHelper.ChochosanDebug("Attack" + gameObject.name, "green");
                }
                else
                {
                    currentTarget = null;
                    currentDamageable = null;
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
            if((defaultTargetIfNoOtherAvailable.transform.position - thisTransform.position).magnitude <= defaultAgentStoppingDistance)
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
            Vector3 heading = currentTarget.transform.position - thisTransform.position;
            headingDistance = heading.magnitude;

            if (headingDistance <= secondPureEnemySenseRange && headingDistance > agent.stoppingDistance) //if player is far but scented then go to him
            {
                GoIntoMovingToTarget();
            }
            else if (headingDistance <= agent.stoppingDistance) //if player is within stop range and can attack go to attack state
            {
                if (aiState != AIState.Attack)
                {
                    aiState = AIState.Attack;
                    currentDamageable = currentTarget.GetComponent<IDamageable>();
                    attackAnimTimestamp = Time.time + attackInterval;
                    LookAtTarget();
                }
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
            Chochosan.ChochosanHelper.ChochosanDebug(aiState.ToString(), "red");
        }
        if(isDummy)
        {
            agent.speed = 0;
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
            agent.speed = runSpeed;
            aiState = AIState.MovingToTarget;
            currentDamageable = currentTarget.GetComponent<IDamageable>();
            if (currentDamageable != null && attackerType != AttackerType.Wizard)
                agent.stoppingDistance = currentDamageable.GetCustomAgentStoppingDistance();
            else
                agent.stoppingDistance = defaultAgentStoppingDistance;
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

    public void TakeDamage(float damage, AI_Attacker attacker)
    {
        if (attacker != null && currentTarget == null)
        {
            currentTarget = attacker.gameObject;
        }
        currentHealth -= damage;

        //enable particle if not active then disable after some time
        if (!hitParticle.activeSelf)
        {
            hitParticle.SetActive(true);
            StartCoroutine(DisableHitParticle());
        }
        if (currentHealth <= 0)
        {
            Instantiate(deathParticle, thisTransform.position, deathParticle.transform.rotation);
            AI_Attacker_Loader.RemoveAttackerFromList(this);
            Destroy(this.gameObject);
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
    private bool isStillSpawning;
    private int currentPoolItem;

    private void CastSpell()
    {
        if (isStillSpawning) //if the pool is still not full
        {
            GameObject projectileCopy = Instantiate(projectilePrefab, transform.position + offsetVector, projectilePrefab.transform.rotation);
            projectileCopy.GetComponent<Projectile_Controller>().SetTarget(currentTarget);
            AddObjectToPool(projectileCopy);
        }
        else //when full start using items from the pool
        {
            projectilePool[currentPoolItem].transform.position = transform.position + offsetVector;
            projectilePool[currentPoolItem].GetComponent<Projectile_Controller>().SetTarget(currentTarget);
            projectilePool[currentPoolItem].SetActive(true);
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
