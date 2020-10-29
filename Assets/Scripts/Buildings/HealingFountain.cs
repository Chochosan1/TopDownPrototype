using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : AI_Base
{
    [Header("Properties")]
    [SerializeField] private float healCooldown = 10f;
    [SerializeField] private float flatHealAmount = 10f;
    [SerializeField] private Vector3 healingPositionOffset;
    private float healTimestamp;
    private Collider[] objectsToHeal;

    private void Update()
    {
        if (Time.time >= healTimestamp)
        {
            healTimestamp = Time.time + healCooldown;
            objectsToHeal = GetAllInRange(healingPositionOffset);
            if (objectsToHeal.Length > 0)
                HealDamageables();         
        }
    }

    private void HealDamageables()
    {
        Debug.Log(objectsToHeal.Length);
        for(int i = 0; i < objectsToHeal.Length; i++)
        {
            Debug.Log("HEALING " + objectsToHeal[i].name);
              objectsToHeal[i].gameObject.GetComponent<IDamageable>().Heal(flatHealAmount);
         //   objectsToHeal[i].gameObject.GetComponent<IDamageable>().TakeDamage(flatHealAmount, null);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + healingPositionOffset, firstPureEnemySenseRange);
    }
}
