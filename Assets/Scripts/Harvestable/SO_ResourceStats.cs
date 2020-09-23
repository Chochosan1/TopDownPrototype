using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The stats for each HarvestableController
/// </summary>
[CreateAssetMenu(fileName = "ResourceStats", menuName = "Chochosan/Harvestable/StatsAsset", order = 1)]
public class SO_ResourceStats : ScriptableObject
{
    public string harvestableName;
    public float maxResourcesToHarvest;
    public float resourcePerSingleHarvest;
}
