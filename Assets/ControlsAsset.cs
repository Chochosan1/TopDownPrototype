// GENERATED AUTOMATICALLY FROM 'Assets/ControlsAsset.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @ControlsAsset : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @ControlsAsset()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ControlsAsset"",
    ""maps"": [
        {
            ""name"": ""MainControls"",
            ""id"": ""dd2d841d-717e-4e8d-b3e8-af3909b3c846"",
            ""actions"": [
                {
                    ""name"": ""SpawnBuilding"",
                    ""type"": ""Button"",
                    ""id"": ""02e28757-d28e-4538-8bea-b8d1ad38518b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CancelBuilding"",
                    ""type"": ""Button"",
                    ""id"": ""410e3ac7-7899-43d9-916c-879faf3bae54"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""AllowBuildingRotation"",
                    ""type"": ""Button"",
                    ""id"": ""7093fcb6-1647-49a3-a729-67d2c631d4b2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateObject"",
                    ""type"": ""Value"",
                    ""id"": ""09365f5a-9636-4e75-b2b0-5c2e2c05130b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""961ba7ad-aa26-45f1-8edd-53211fc54e6e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpawnBuilding"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cbe6d488-f5e1-4417-9a47-dd65df1f02f6"",
                    ""path"": ""<Keyboard>/backquote"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CancelBuilding"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""89cd19bf-4144-4001-bfbf-731b9f167ea0"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AllowBuildingRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""72a357db-ab15-4cc9-8fb7-41cdcb3386f9"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RotateObject"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MainControls
        m_MainControls = asset.FindActionMap("MainControls", throwIfNotFound: true);
        m_MainControls_SpawnBuilding = m_MainControls.FindAction("SpawnBuilding", throwIfNotFound: true);
        m_MainControls_CancelBuilding = m_MainControls.FindAction("CancelBuilding", throwIfNotFound: true);
        m_MainControls_AllowBuildingRotation = m_MainControls.FindAction("AllowBuildingRotation", throwIfNotFound: true);
        m_MainControls_RotateObject = m_MainControls.FindAction("RotateObject", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // MainControls
    private readonly InputActionMap m_MainControls;
    private IMainControlsActions m_MainControlsActionsCallbackInterface;
    private readonly InputAction m_MainControls_SpawnBuilding;
    private readonly InputAction m_MainControls_CancelBuilding;
    private readonly InputAction m_MainControls_AllowBuildingRotation;
    private readonly InputAction m_MainControls_RotateObject;
    public struct MainControlsActions
    {
        private @ControlsAsset m_Wrapper;
        public MainControlsActions(@ControlsAsset wrapper) { m_Wrapper = wrapper; }
        public InputAction @SpawnBuilding => m_Wrapper.m_MainControls_SpawnBuilding;
        public InputAction @CancelBuilding => m_Wrapper.m_MainControls_CancelBuilding;
        public InputAction @AllowBuildingRotation => m_Wrapper.m_MainControls_AllowBuildingRotation;
        public InputAction @RotateObject => m_Wrapper.m_MainControls_RotateObject;
        public InputActionMap Get() { return m_Wrapper.m_MainControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MainControlsActions set) { return set.Get(); }
        public void SetCallbacks(IMainControlsActions instance)
        {
            if (m_Wrapper.m_MainControlsActionsCallbackInterface != null)
            {
                @SpawnBuilding.started -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnSpawnBuilding;
                @SpawnBuilding.performed -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnSpawnBuilding;
                @SpawnBuilding.canceled -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnSpawnBuilding;
                @CancelBuilding.started -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnCancelBuilding;
                @CancelBuilding.performed -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnCancelBuilding;
                @CancelBuilding.canceled -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnCancelBuilding;
                @AllowBuildingRotation.started -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnAllowBuildingRotation;
                @AllowBuildingRotation.performed -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnAllowBuildingRotation;
                @AllowBuildingRotation.canceled -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnAllowBuildingRotation;
                @RotateObject.started -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnRotateObject;
                @RotateObject.performed -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnRotateObject;
                @RotateObject.canceled -= m_Wrapper.m_MainControlsActionsCallbackInterface.OnRotateObject;
            }
            m_Wrapper.m_MainControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SpawnBuilding.started += instance.OnSpawnBuilding;
                @SpawnBuilding.performed += instance.OnSpawnBuilding;
                @SpawnBuilding.canceled += instance.OnSpawnBuilding;
                @CancelBuilding.started += instance.OnCancelBuilding;
                @CancelBuilding.performed += instance.OnCancelBuilding;
                @CancelBuilding.canceled += instance.OnCancelBuilding;
                @AllowBuildingRotation.started += instance.OnAllowBuildingRotation;
                @AllowBuildingRotation.performed += instance.OnAllowBuildingRotation;
                @AllowBuildingRotation.canceled += instance.OnAllowBuildingRotation;
                @RotateObject.started += instance.OnRotateObject;
                @RotateObject.performed += instance.OnRotateObject;
                @RotateObject.canceled += instance.OnRotateObject;
            }
        }
    }
    public MainControlsActions @MainControls => new MainControlsActions(this);
    public interface IMainControlsActions
    {
        void OnSpawnBuilding(InputAction.CallbackContext context);
        void OnCancelBuilding(InputAction.CallbackContext context);
        void OnAllowBuildingRotation(InputAction.CallbackContext context);
        void OnRotateObject(InputAction.CallbackContext context);
    }
}
