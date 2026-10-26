using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerControlInput), typeof(PlayerController), typeof(PlayerState))]
public class PlayerCrouchAbility : MonoBehaviour
{
    private enum CrouchSetting {Hold, Toggle}

    #region Inspector

    [Header("Crouch Settings")] 
    [SerializeField] private CrouchSetting crouchSetting;

    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float cameraCrouchingHeight = 0.9f;
    [SerializeField] private float crouchingSpeed = 2f;

    [SerializeField] private LayerMask ceilingMask = ~0;

    [Header("Events")] 
    [SerializeField] private UnityEvent onCrouchStarted;
    [SerializeField] private UnityEvent onCrouchEnded;

    #endregion

    #region Private Variables

    private PlayerControlInput _playerControlInput;
    private PlayerController _playerController;
    private PlayerState _playerState;

    private bool _wantsToStandUp;

    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _playerControlInput = GetComponent<PlayerControlInput>();
        _playerController = GetComponent<PlayerController>();
        _playerState = GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        _playerControlInput.CrouchChanged += Crouch;
        _playerState.StateChanged += OnStateChanged;
    }

    //Delete if not needed
    private void Update()
    {
        if (_wantsToStandUp)
        {
            TryStandUp();
        }
    }

    private void OnDisable()
    {
        _playerControlInput.CrouchChanged -= Crouch;
        _playerState.StateChanged -= OnStateChanged;
    }

    #endregion
    
    #region Crouch Methods

    private void Crouch(bool isPressed)
    {
        switch (crouchSetting)
        {
            case CrouchSetting.Hold:
                if (isPressed)
                {
                    CrouchInitiate();
                }
                else
                {
                    CrouchRelease();
                }
                break;
            
            case CrouchSetting.Toggle:
                if (!isPressed) break;

                if (_playerState.CompareState(MovementState.Crouching) && !_wantsToStandUp)
                {
                    CrouchRelease();
                }
                else
                {
                    CrouchInitiate();
                }
                break;
        }
    }

    private void CrouchInitiate()
    {
        _wantsToStandUp = false;
        _playerState.SetState(MovementState.Crouching);
    }

    private void CrouchRelease()
    {
        if (!_playerState.CompareState(MovementState.Crouching)) return;

        _wantsToStandUp = true;
        TryStandUp();
    }

    private void TryStandUp()
    {
        if (!_playerController.CanStandUp(ceilingMask)) return;

        _wantsToStandUp = false;
        _playerState.SetState(MovementState.Walking);
    }

    private void OnStateChanged(MovementState oldState, MovementState newState)
    {
        if (newState == MovementState.Crouching)
        {
            _playerController.SetHeight(crouchingHeight, cameraCrouchingHeight);
            _playerController.SetMoveSpeed(crouchingSpeed);
            onCrouchStarted?.Invoke();
        }
        else if (oldState == MovementState.Crouching)
        {
            _playerController.ResetHeight();
            _playerController.ResetMoveSpeed();
            onCrouchEnded?.Invoke();
        }
    }
    
    #endregion
}