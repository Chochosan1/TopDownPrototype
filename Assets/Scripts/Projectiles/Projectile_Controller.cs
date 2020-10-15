using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Controller : MonoBehaviour
{
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private bool is_Homing;
    [SerializeField] private bool is_AoE_Projectile;
    private GameObject target;
    private Transform thisTransform;

    private void Start()
    {
        thisTransform = GetComponent<Transform>();
    }

    private void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            IDamageable tempInterface = collision.gameObject.GetComponent<IDamageable>();
            if (tempInterface != null)
            {
                tempInterface.TakeDamage(stats.damage, null);
            }
        }
        gameObject.SetActive(false); 
    }

    private void Update()
    {
        if(is_Homing && target != null)
        {
            thisTransform.position = Vector3.Lerp(thisTransform.position, target.transform.position, stats.travelSpeed * Time.deltaTime);
        }    
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
    }
}
