using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Auto-aiming weapon that uses object pooling for its bullets.
/// </summary>
public class Turret : AI_Base, ISpawnedAtWorld
{
    [SerializeField] private float castCooldown;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float shootForce;
    [SerializeField] private float enableTurretAfterSeconds;
    [SerializeField] private Vector3 offsetVector;
    private float castTimestamp;
    private bool isTurretEnabled = false;

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
        isTurretEnabled = true;
        projectilePool = new List<GameObject>();
    }

    void Update()
    {
     
        if (currentTarget != null)
        {     
            CastSpell();
        }
        else
        {
            ChooseNewTarget(false);
        }
    }

    private void CastSpell()
    {
        if (currentTarget != null && Time.time >= castTimestamp && isTurretEnabled)
        {
            Debug.Log("TURRET SHOOT");
            castTimestamp = Time.time + castCooldown;
            if (isStillSpawning) //if the pool is still not full
            {
                GameObject projectileCopy = Instantiate(projectilePrefab, projectileSpawnPoint.position + offsetVector, projectilePrefab.transform.rotation);
               // projectileCopy.GetComponent<Rigidbody>().AddForce((currentTarget.transform.position - transform.position) * shootForce, ForceMode.Impulse);
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
