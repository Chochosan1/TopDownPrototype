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
        [SerializeField] private GameObject selectedUnitInfoPanel;
        [SerializeField] private GameObject selectedBuildingUpgradePanel;
        [SerializeField] private TextMeshProUGUI selectedUnitText;
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
            Unit_Controller.Instance.OnUnitSelected += ActivateUnitSelectionUI;
            Unit_Controller.Instance.OnUnitDeselected += DeactivateAllSelectionUI;
        }

        private void OnDisable()
        {
            PlayerInventory.Instance.OnInventoryValueChanged -= UpdateTextValue;
            Unit_Controller.Instance.OnUnitSelected -= ActivateUnitSelectionUI;
            Unit_Controller.Instance.OnUnitDeselected -= DeactivateAllSelectionUI;
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

        //force a state upon the panel
        public void ToggleSelectedUnitInfo(bool state, string name)
        {
            selectedUnitInfoPanel.SetActive(state);
            selectedUnitText.text = name;
        }

        //activate the selection UI
        private void ActivateUnitSelectionUI(ISelectable unitSelectable)
        {
            RefreshPanel(selectedUnitInfoPanel);
            selectedUnitInfoPanel.SetActive(true);
            selectedUnitText.text = unitSelectable.GetSelectedUnitInfo();
            if(unitSelectable.IsOpenUpgradePanel()) //activate panel for upgrades if true and add click events
            {
                selectedBuildingUpgradePanel.SetActive(true);
                selectedBuildingUpgradePanel.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners(); //very important to first clear all other listenes
                selectedBuildingUpgradePanel.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(unitSelectable.UpgradeUnit);
            }      
            else
            {
                selectedBuildingUpgradePanel.SetActive(false);
            }
        }

        private void RefreshPanel(GameObject panel)
        {
            panel.SetActive(false);
            panel.SetActive(true);
        }

        //clear all active UI
        private void DeactivateAllSelectionUI()
        {
            selectedUnitInfoPanel.SetActive(false);
            selectedUnitText.text = "";
            selectedBuildingUpgradePanel.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            selectedBuildingUpgradePanel.SetActive(false);         
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
