using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Inheritted when the instantiated object needs to have some functiality when it first spawns in the world.
/// </summary>
public interface ISpawnedAtWorld
{
    void StartInitialSetup();
}
