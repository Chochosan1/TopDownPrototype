using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, AI_Attacker attacker);
    float GetCustomAgentStoppingDistance();
}
