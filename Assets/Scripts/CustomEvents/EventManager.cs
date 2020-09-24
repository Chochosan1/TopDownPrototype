using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Global events come here. Single instances of classes hold their own events in order to preserve the encapsulation (e.g ObjectSpawner.cs)
/// Multiple instances of classes (e.g. BuildingController.cs) are controlled with global events to avoid holding local events in every instance.
/// </summary>
namespace Chochosan
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        //event subscribed to for example in PlayerInventory in order to spend resources 
        public delegate void OnBuildingUpgradedDelegate(SO_CostRequirements requirements);
        public OnBuildingUpgradedDelegate OnBuildingUpgraded;
    }
}
