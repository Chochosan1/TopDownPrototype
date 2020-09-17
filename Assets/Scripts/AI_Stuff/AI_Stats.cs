using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIStats", menuName = "Chochosan/AI/StatsAsset", order = 1)]
public class AI_Stats : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
}
