using UnityEngine;
using System;
public enum MovementState
{
    Walking, 
    Running, 
    Crouching
}
public class PlayerState : MonoBehaviour
{
    public event Action<MovementState, MovementState> StateChanged;

    [SerializeField] private MovementState currentState = MovementState.Walking;
    public MovementState CurrentState => currentState;

    public bool CompareState(MovementState state)
    {
        return currentState == state;
    }

    public void SetState(MovementState newState)
    {
        if (newState == currentState) return;

        MovementState oldState = currentState;
        currentState = newState;
        StateChanged?.Invoke(oldState, newState);
    }
}