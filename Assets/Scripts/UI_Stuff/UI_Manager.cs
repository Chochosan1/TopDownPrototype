using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Resources Texts")]
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI ironText;
        [SerializeField] private TextMeshProUGUI currentPopulationText;
        [SerializeField] private TextMeshProUGUI maxPopulationText;
        [SerializeField] private TextMeshProUGUI currentCharismaText;
        [SerializeField] private TextMeshProUGUI currentFoodText;

        [Header("Building Buttons")]
        [SerializeField] private Button townHallButton;
        [SerializeField] private Button woodCampButton;
        [SerializeField] private Button ironMineButton;
        [SerializeField] private Button goldMineButton;
        [SerializeField] private Button houseButton;
        [SerializeField] private Button turretButton;

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
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged += RefreshTextInfo;
            Chochosan.EventManager.Instance.OnBuildingBuiltFinally += UpdateBuildingUI;
        }

        private void OnDisable()
        {
            PlayerInventory.Instance.OnInventoryValueChanged -= UpdateTextValue;
            Unit_Controller.Instance.OnUnitSelected -= ActivateUnitSelectionUI;
            Unit_Controller.Instance.OnUnitDeselected -= DeactivateAllSelectionUI;
            Chochosan.EventManager.Instance.OnDisplayedUIValueChanged -= RefreshTextInfo;
            Chochosan.EventManager.Instance.OnBuildingBuiltFinally -= UpdateBuildingUI;
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

        private void RefreshTextInfo(ISelectable selectable)
        {
            selectedUnitText.text = selectable.GetSelectedUnitInfo();
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
                    woodText.text = value.ToString("F0");
                    break;
                case "gold":
                    goldText.text = value.ToString("F0");
                    break;
                case "iron":
                    ironText.text = value.ToString("F0");
                    break;
                case "currentPopulation":
                    currentPopulationText.text = value.ToString();
                    break;
                case "maxPopulation":
                    maxPopulationText.text = value.ToString();
                    break;
                case "charisma":
                    currentCharismaText.text = value.ToString("F0");
                    break;
                case "food":
                    currentFoodText.text = value.ToString("F0");
                    break;
            }        
        }

        private void UpdateBuildingUI(BuildingController bc, Buildings buildingType)
        {
            switch (buildingType)
            {
                case Buildings.TownHall:
                    townHallButton.interactable = false;
                    woodCampButton.interactable = true;
                    houseButton.interactable = true;
                    break;
                case Buildings.Woodcamp:                 
                    ironMineButton.interactable = true;
                    break;
                case Buildings.Ironmine:
                    goldMineButton.interactable = true;
                    break;
                case Buildings.Goldmine:
                    turretButton.interactable = true;
                    break;
            }
        }

        public void DisplayWarningMessage()
        {
            Chochosan.ChochosanHelper.ChochosanDebug("NOT ENOUGH RESOURCES", "red");
        }

        #region ButtonAssignables
        public void DeleteSave()
        {
            Chochosan.SaveLoadManager.SeriouslyDeleteAllSaveFiles();
        }
        #endregion
    }
}
