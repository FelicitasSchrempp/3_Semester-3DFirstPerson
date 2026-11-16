using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : MonoBehaviour
{
    #region Action Events
    public event Action<bool> CrouchChanged;
    public event Action<bool> SprintChanged;
    public event Action JumpPressed;
    public event Action InteractPressed;
    #endregion
    
    private InputSystem_Actions _inputActions;
    
    private InputAction _escapeAction;
    private InputAction _interactAction;

    #region Unity Events
    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _escapeAction = _inputActions.UI.Escape;
        _interactAction = _inputActions.UI.Interact;
    }

    private void OnEnable()
    {
        SubscribeInputs();
        EnableInputs();
    }

    private void OnDisable()
    {
        UnsubscribeInputs();
        DisableInputs();
    }

    private void OnDestroy()
    {
        _inputActions.Dispose();
    }
    
    #endregion

    #region Input Methods

    private void SubscribeInputs()
    {
        _escapeAction.performed += Escape;
        _interactAction.performed += Interact;
    }

    private void UnsubscribeInputs()
    {
        _escapeAction.performed -= Escape;
        _interactAction.performed -= Interact;
    }

    public void EnableInputs()
    {
        _inputActions.Enable();
    }

    public void DisableInputs()
    {
        _inputActions.Disable();
    }
    #endregion

    #region Input Callbacks

    private void Escape(InputAction.CallbackContext ctx)
    {
        
    }
    
    private void Interact(InputAction.CallbackContext ctx)
    {
        
    }
    

    #endregion
}
