using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Creates the stats for the AI units.
/// </summary>
[CreateAssetMenu(fileName = "AIStats", menuName = "Chochosan/AI/StatsAsset", order = 1)]
public class AI_Stats : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
}
