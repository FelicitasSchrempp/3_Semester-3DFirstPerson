using System;
using UnityEngine;

public class PlayerJumpAbility : MonoBehaviour
{
    #region Inspector

    [Header("Player Jump")] 
    [SerializeField] private float jumpBufferTime = 0.15f;

    [SerializeField] private float coyoteTime = 0.2f;

    #endregion

    #region Private Variables

    private PlayerControlInput _playerControlInput;
    private PlayerController _playerController;

    private float _lastJumpPressedTime = float.NegativeInfinity;
    private float _lastGroundedTime = float.NegativeInfinity;

    #endregion
    
    #region Unity Events

    private void Awake()
    {
        _playerControlInput = GetComponent<PlayerControlInput>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _playerControlInput.JumpPressed += BufferJumpInput;
    }
    
    private void OnDisable()
    {
        _playerControlInput.JumpPressed -= BufferJumpInput;
    }
    
    
    private void Update()
    {
        TrackGroundTime();
        TryJump();
    }
    
    #endregion

    #region Jump Methods

    private void TrackGroundTime()
    {
        if (_playerController.CanJump)
        {
            _lastGroundedTime = Time.time;
        }
    }

    private void BufferJumpInput()
    {
        _lastJumpPressedTime = Time.time;
    }

    private void TryJump()
    {
        bool hasBufferedJump = Time.time - _lastJumpPressedTime <= jumpBufferTime;
        bool hasCoyoteWindow = Time.time - _lastGroundedTime <= coyoteTime;

        if (!hasBufferedJump || !hasCoyoteWindow) return;

        _playerController.Jump();
        _lastJumpPressedTime = float.NegativeInfinity;
        _lastGroundedTime = float.NegativeInfinity;
    }

    #endregion
}