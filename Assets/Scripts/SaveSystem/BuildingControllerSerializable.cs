using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Holds all information for BuildingController.cs in a serializable way.
/// </summary>
[System.Serializable]
public struct BuildingControllerSerializable
{
    public float buildingCurrentHP;
    public float x, y, z; //position
    public int currentBuildingLevel;
    public int buildingIndex;
    public float rotX, rotY, rotZ, rotW;
    public int numberOfVillagersAssigned;
    public float[] villagerXpositions;
    public float[] villagerYpositions;
    public float[] villagerZpositions;
    public string[] villagerTypeStrings;
}
