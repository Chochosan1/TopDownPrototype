using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Anything that can be harvested must inherit this.
/// </summary>
public interface IHarvestable
{
    void Harvest();
}
