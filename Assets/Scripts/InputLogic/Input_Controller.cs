using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chochosan
{
    public class Input_Controller : MonoBehaviour
    {
        public ControlsAsset controls;

        private void Awake()
        {
            controls = new ControlsAsset();
        }

        private void OnEnable()
        {
            controls.MainControls.SpawnBuilding.performed += HandleSpawnObjectAtWorld;
            controls.MainControls.CancelBuilding.performed += HandleCancelBuilding;
            controls.MainControls.AllowBuildingRotation.performed += HandleAllowBuildingRotation;
            controls.MainControls.AllowBuildingRotation.canceled += HandleAllowBuildingRotation;
            controls.MainControls.RotateObject.performed += HandleBuildingRotation;

            controls.MainControls.SpawnBuilding.Enable();
            controls.MainControls.CancelBuilding.Enable();
            controls.MainControls.AllowBuildingRotation.Enable();
            controls.MainControls.RotateObject.Enable();
        }

        private void OnDisable()
        {
            controls.MainControls.SpawnBuilding.performed -= HandleSpawnObjectAtWorld;
            controls.MainControls.CancelBuilding.performed -= HandleCancelBuilding;
            controls.MainControls.AllowBuildingRotation.performed -= HandleAllowBuildingRotation;
            controls.MainControls.AllowBuildingRotation.canceled -= HandleAllowBuildingRotation;
            controls.MainControls.RotateObject.performed -= HandleBuildingRotation;

            controls.MainControls.SpawnBuilding.Disable();
            controls.MainControls.CancelBuilding.Disable();
            controls.MainControls.AllowBuildingRotation.Disable();
            controls.MainControls.RotateObject.Disable();
        }

        private void HandleSpawnObjectAtWorld(InputAction.CallbackContext obj)
        {
            ObjectSpawner.Instance.SpawnObjectAtWorld();
        }

        private void HandleCancelBuilding(InputAction.CallbackContext obj)
        {
            ObjectSpawner.Instance.CancelObject();
        }

        private void HandleAllowBuildingRotation(InputAction.CallbackContext obj)
        {
            ObjectSpawner.Instance.ToggleObjectRotation();
            RTS_CameraController.ToggleCameraZooming();
        }

        private void HandleBuildingRotation(InputAction.CallbackContext obj)
        {
            ObjectSpawner.Instance.RotateCurrentObject(obj);
        }
    }
}
