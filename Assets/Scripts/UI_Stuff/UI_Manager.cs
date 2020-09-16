using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class UI_Manager : MonoBehaviour
    {
        public static UI_Manager Instance;

        [SerializeField] private GameObject objectManipulationInfoPanel;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        //automatically toggles on/off depending on the current state
        public void ToggleObjectManipulationInfo()
        {
            objectManipulationInfoPanel.SetActive(!objectManipulationInfoPanel.activeSelf);
        }

        //force a state upon the panel
        public void ToggleObjectManipulationInfo(bool state)
        {
            objectManipulationInfoPanel.SetActive(state);
        }

        public void PlayHoverAnimation(Animator anim)
        {
            anim.SetBool("OnHoverTrigger", true);
            //Testgit
        }
    }
}
