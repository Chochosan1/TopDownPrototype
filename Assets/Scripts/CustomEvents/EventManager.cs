using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

       
    }
}
