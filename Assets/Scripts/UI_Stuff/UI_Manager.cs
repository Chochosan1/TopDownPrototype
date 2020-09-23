using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Chochosan
{
    public class UI_Manager : MonoBehaviour
    {
        public static UI_Manager Instance;

        [SerializeField] private GameObject objectManipulationInfoPanel;
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI ironText;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            PlayerInventory.Instance.OnInventoryValueChanged += UpdateTextValue;
        }

        private void OnDisable()
        {
            PlayerInventory.Instance.OnInventoryValueChanged -= UpdateTextValue;
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
        }

        public void UpdateTextValue(string valueName, float value)
        {
            switch (valueName)
            {
                case "wood":
                    woodText.text = value.ToString();
                    break;
                case "gold":
                    goldText.text = value.ToString();
                    break;
                case "iron":
                    ironText.text = value.ToString();
                    break;
            }        
        }

        public void DisplayWarningMessage()
        {
            Chochosan.ChochosanHelper.ChochosanDebug("NOT ENOUGH RESOURCES", "red");
        }
    }
}
