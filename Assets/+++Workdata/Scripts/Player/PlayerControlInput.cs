using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlInput : MonoBehaviour
{
    #region Action Events
    public event Action<bool> CrouchChanged;
    public event Action<bool> SprintChanged;
    public event Action JumpPressed;
    public event Action InteractPressed;
    #endregion
    
    private InputSystem_Actions _inputActions;
    
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _interactAction;
    private InputAction _crouchAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;
    
    private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;
    public Vector2 LookInput => _lookAction.ReadValue<Vector2>();
    
    

    #region Unity Events
    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _moveAction = _inputActions.Player.Move;
        _lookAction = _inputActions.Player.Look;
        _crouchAction = _inputActions.Player.Crouch;
        _sprintAction = _inputActions.Player.Sprint;
        _jumpAction = _inputActions.Player.Jump;
        _interactAction = _inputActions.Player.Interact;
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
        _moveAction.performed += Move;
        _moveAction.canceled += Move;
        
        _interactAction.started += InteractStarted;
        _interactAction.performed += InteractPerformed;
        _interactAction.canceled += InteractCanceled;

        _crouchAction.performed += Crouch;
        _crouchAction.canceled += Crouch;
        
        _sprintAction.performed += Sprint;
        _sprintAction.canceled += Sprint;
        
        _jumpAction.performed += Jump;
    }

    private void UnsubscribeInputs()
    {
        _moveAction.performed -= Move;
        _moveAction.canceled -= Move;
        
        _interactAction.started -= InteractStarted;
        _interactAction.performed -= InteractPerformed;
        _interactAction.canceled -= InteractCanceled;
        
        _crouchAction.performed -= Crouch;
        _crouchAction.canceled -= Crouch;
        
        _sprintAction.performed -= Sprint;
        _sprintAction.canceled -= Sprint;
        
        _jumpAction.performed -= Jump;
    }

    public void EnableInputs()
    {
        _inputActions.Enable();
    }

    public void DisableInputs()
    {
        _inputActions.Disable();
        _moveInput = Vector2.zero;
    }
    #endregion

    #region Input Callbacks

    private void Move(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }
    
    private void InteractStarted(InputAction.CallbackContext ctx)
    {
        InteractPressed?.Invoke();
    }
    
    private void InteractPerformed(InputAction.CallbackContext ctx)
    {
        print("Interaction perfromed");
    }
    
    private void InteractCanceled(InputAction.CallbackContext ctx)
    {
        print("Interaction canceled");
    }

    private void Crouch(InputAction.CallbackContext ctx)
    {
        CrouchChanged?.Invoke(ctx.performed);
    }
    
    private void Sprint(InputAction.CallbackContext ctx)
    {
        SprintChanged?.Invoke(ctx.performed);
    }
    
    private void Jump(InputAction.CallbackContext ctx)
    {
        JumpPressed?.Invoke();
    }
    

    #endregion
}