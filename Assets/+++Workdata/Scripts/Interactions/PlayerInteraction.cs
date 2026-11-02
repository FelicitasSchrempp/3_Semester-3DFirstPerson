using System;
using UnityEngine;

[RequireComponent(typeof(PlayerControlInput), typeof(PlayerRaycast))]
public class PlayerInteraction : MonoBehaviour
{
    #region Private Variables

    private PlayerControlInput _playerControlInput;
    private PlayerRaycast _playerRaycast;

    #endregion

    #region Unity Events

    private void Awake()
    {
        _playerControlInput = GetComponent<PlayerControlInput>();
        _playerRaycast = GetComponent<PlayerRaycast>();
    }

    private void OnEnable()
    {
        _playerControlInput.InteractPressed += Interact;
    }

    private void OnDisable()
    {
        _playerControlInput.InteractPressed -= Interact;
    }

    #endregion

    #region Interaction Methods

    private void Interact()
    {
        BaseInteractable target = _playerRaycast.CurrentInteractable;
        if (target == null)
        {
            print("No Interaction found!");
            return;
        }
        target.Interact();
    }

    #endregion
}
