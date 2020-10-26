using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// All selectable objects must inherit this.
/// </summary>
public interface ISelectable
{
    //if the agent is movable use this
    void ForceSetAgentArea(Vector3 destination);

    //general info for the selected object that will be displayed in the UI
    string GetSelectedUnitInfo();

    //subscribed to the shown UI button
    void UpgradeUnit();

    void ForceSetSpecificTarget(GameObject target);

    void CheckIfSelectedBySelector();
    void ToggleSelectedIndicator(bool value);
}
