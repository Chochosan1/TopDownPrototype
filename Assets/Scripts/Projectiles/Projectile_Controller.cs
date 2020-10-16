using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Controller : MonoBehaviour
{
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private GameObject mainParticle;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private bool is_Homing;
    [SerializeField] private bool is_AoE_Projectile;
    private GameObject target;
    private Transform thisTransform, targetTransform;
    private Vector3 dir;
    private void Start()
    {
        thisTransform = GetComponent<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
          //  Debug.Log("HIT ENEMY");
            IDamageable tempInterface = other.gameObject.GetComponent<IDamageable>();
            if (tempInterface != null)
            {
                tempInterface.TakeDamage(stats.damage, null);
            }
            DeactivateProjectileOnHit();
        }     
    }

    private void Update()
    {
        if(is_Homing && target != null)
        {
            //  thisTransform.position = Vector3.Lerp(thisTransform.position, target.transform.position + new Vector3(0f, 1f, 0f), stats.travelSpeed * Time.deltaTime);
            dir = (targetTransform.position - thisTransform.position) + new Vector3(0f, 1f, 0f);
            transform.Translate(dir.normalized * stats.travelSpeed * Time.deltaTime);
        }    
        else
        {
            DeactivateProjectileOnHit();
        }
    }

    public void SetTarget(GameObject target)
    {
        mainParticle.SetActive(true);
        this.target = target;
        targetTransform = target.transform;
    }

    private void DeactivateProjectileOnHit()
    {
        hitParticle.SetActive(true);
        mainParticle.SetActive(false);
        StartCoroutine(DeactivateHitParticleAfter());
    }

   private IEnumerator DeactivateHitParticleAfter()
    {
        yield return new WaitForSeconds(0.65f);
        hitParticle.SetActive(false);
        gameObject.SetActive(false);
    }
}
