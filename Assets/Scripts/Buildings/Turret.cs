using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Turret : AI_Base, ISpawnedAtWorld
{
    [SerializeField] private float castCooldown;
    [SerializeField] private GameObject projectilePrefab;
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
    public List<GameObject> projectilePool;
    private bool isStillSpawning = true;

    void Update()
    {
        ChooseNewTarget();

        if (currentTarget != null)
        {
        //    SetTargetToNullIfTargetIsDead();        
            CastSpell();
        }
    }

    private void CastSpell()
    {
        if (currentTarget != null && Time.time >= castTimestamp && isTurretEnabled)
        {
            castTimestamp = Time.time + castCooldown;
            if (isStillSpawning) //if the pool is still not full
            {
                GameObject projectileCopy = Instantiate(projectilePrefab, transform.position + offsetVector, projectilePrefab.transform.rotation);
                projectileCopy.GetComponent<Rigidbody>().AddForce((currentTarget.transform.position - transform.position) * shootForce, ForceMode.Impulse);
                AddObjectToPool(projectileCopy);
            }
            else //when full start using items from the pool
            {
                // Debug.Log("I COME FROM THE POOL");
                Rigidbody tempRb = projectilePool[currentPoolItem].GetComponent<Rigidbody>();
                tempRb.velocity = new Vector3(0, 0, 0);
                projectilePool[currentPoolItem].transform.position = transform.position + offsetVector;
                projectilePool[currentPoolItem].SetActive(true);
                tempRb.AddForce((currentTarget.transform.position - transform.position) * shootForce, ForceMode.Impulse);
                currentPoolItem++;

                if (currentPoolItem >= projectilePool.Count)
                {
                    currentPoolItem = 0;
                    //   Debug.Log("MAX OBJECT REACHED, STARTING FROM THE START");
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
        isTurretEnabled = true;
    }

    public void StartInitialSetup()
    {
        StartCoroutine(EnableTurretAfter());
    }
}
