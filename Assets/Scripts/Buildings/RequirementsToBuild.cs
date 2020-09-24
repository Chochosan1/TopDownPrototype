using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Attached to every preview buildable object. These resources will be used to evaluate whether the building can be built or not.
/// </summary>
public class RequirementsToBuild : MonoBehaviour
{
    [SerializeField] private SO_CostRequirements costReq;

    public SO_CostRequirements GetRequirements()
    {
        return costReq;
    }
}
