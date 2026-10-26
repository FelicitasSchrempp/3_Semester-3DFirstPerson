using Unity.U2D.Physics;
using UnityEngine;

[RequireComponent(typeof(PlayerControlInput), typeof(PlayerController), typeof(PlayerState))]
public class PlayerSprintAbility : MonoBehaviour
{
    private enum SprintSetting { Hold, Toggle }

    #region Inspector

    [Header("Sprint Settings")]
    [SerializeField] private SprintSetting sprintSetting = SprintSetting.Hold;
    [SerializeField] private float sprintingSpeed = 9f;
    
    #endregion

    #region Private Variables

    private PlayerControlInput _playerControlInput;
    private PlayerController _playerController;
    private PlayerState _playerState;

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
        _playerControlInput.SprintChanged += Sprint;
        _playerState.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        _playerControlInput.SprintChanged -= Sprint;
        _playerState.StateChanged -= OnStateChanged;
    }

    #endregion

    #region Sprint Methods

    private void Sprint(bool isPressed)
    {
        switch (sprintSetting)
        {
            case SprintSetting.Hold:
                if (isPressed)
                {
                    SprintInitiate();
                }
                else
                {
                    SprintRelease();
                }
                break;

            case SprintSetting.Toggle:
                if (!isPressed) break;

                if (_playerState.CompareState(MovementState.Running))
                {
                    SprintRelease();
                }
                else
                {
                    SprintInitiate();
                }
                break;
        }
    }

    private void SprintInitiate()
    {
        if (_playerState.CompareState(MovementState.Crouching)) return;

        _playerState.SetState(MovementState.Running);
    }

    private void SprintRelease()
    {
        if (!_playerState.CompareState(MovementState.Running)) return;

        _playerState.SetState(MovementState.Walking);
    }

    private void OnStateChanged(MovementState oldState, MovementState newState)
    {
        if (oldState == MovementState.Running)
        {
            print("sprinting end");
        }
        
        if (newState != MovementState.Running) return;
        
        _playerController.SetMoveSpeed(sprintingSpeed);
    }

    #endregion
}