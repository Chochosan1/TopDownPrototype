using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Auto-aiming weapon that uses object pooling for its bullets.
/// </summary>
public class Turret : AI_Base, ISpawnedAtWorld
{
    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Properties")]
    [SerializeField] private float castCooldown;
    [Tooltip("How fast the projectile will move.")]
    [SerializeField] private float shootForce;
    [SerializeField] private float enableTurretAfterSeconds;
    [SerializeField] private Vector3 offsetVector;
    private float castTimestamp;
    private bool isTurretEnabled = false;
    private float distanceToTarget;
    private Transform thisTransform;

    //pooling
    [SerializeField]
    [Tooltip("The size of the object pool. Make sure it is big enough so that it can support even a fast attack rate. The pool will get filled during runtime, when this limit is reached items from the pool will be reused.")]
    private int projectilePoolSize = 15;
    private int currentPoolItem = 0; //used to iterate through the pool of items
    private List<GameObject> projectilePool;
    private bool isStillSpawning = true;

    private void Start()
    {
        StartInitialSetup();
        thisTransform = transform;
        isTurretEnabled = true;
        projectilePool = new List<GameObject>();
    }

    void Update()
    {
        if (currentTarget != null)
        {       
            CastSpell();

            //if out of range
            if ((currentTarget.transform.position - thisTransform.position).magnitude > secondPureEnemySenseRange)
            {
                currentTarget = null;
            }
        }           
        else
            ChooseNewTarget(false);
    }

    private void CastSpell()
    {
        if (currentTarget != null && Time.time >= castTimestamp && isTurretEnabled)
        {
            castTimestamp = Time.time + castCooldown;
            if (isStillSpawning) //if the pool is still not full
            {
                GameObject projectileCopy = Instantiate(projectilePrefab, projectileSpawnPoint.position + offsetVector, projectilePrefab.transform.rotation);
                projectileCopy.GetComponent<Projectile_Controller>().SetTarget(currentTarget, null);
                AddObjectToPool(projectileCopy);
            }
            else //when full start using items from the pool
            {
                projectilePool[currentPoolItem].transform.position = transform.position + offsetVector;
                projectilePool[currentPoolItem].SetActive(true);
                projectilePool[currentPoolItem].GetComponent<Projectile_Controller>().SetTarget(currentTarget, null);
                currentPoolItem++;

                if (currentPoolItem >= projectilePool.Count)
                {
                    currentPoolItem = 0;
                }
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

    private IEnumerator EnableTurretAfter()
    {
        yield return new WaitForSeconds(enableTurretAfterSeconds);
        Debug.Log("Turret enabled");
        isTurretEnabled = true;
    }

    public void EnableTurret()
    {
        isTurretEnabled = true;
    }

    public void StartInitialSetup()
    {
        StartCoroutine(EnableTurretAfter());
    }
}
