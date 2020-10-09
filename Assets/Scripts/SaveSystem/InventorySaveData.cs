using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InventorySaveData
{
    public float currentWood, currentGold, currentIron;
    public float maxWood, maxGold, maxIron;
    public int maxPopulation;
    public float currentCharisma;
    public float currentFood, maxFood, currentAutoFoodGeneration;
    public int currentDay;
    public float currentDayTimestamp;
}
