using System;
using System.Collections;
using UnityEngine;

public class DoorInteractable : BaseInteractable
{
    #region Private Variables

    private bool _isStillHovered;
    private Coroutine _cooldownRoutine;

    #endregion

    #region Unity Events

    /*protected override void OnDisable()
    {
        if (_cooldownRoutine != null)
        {
            StopCoroutine(_cooldownRoutine);
            _cooldownRoutine = null;
        }

        canInteract = true;
        base.OnDisable();
    }*/

    #endregion

    #region Interaction Methods

    public override void Interact()
    {
        if (!canInteract) return;

        base.Interact();

        canInteract = false;
        base.Unhover(); 
    }

    public void CanInteractAgain()
    {
        canInteract = true;
        
        if (_isStillHovered)
        {
            base.Hover();
        }
    }

    public override void Hover()
    {
        _isStillHovered = true;
        base.Hover();
    }

    public override void Unhover()
    {
        _isStillHovered = false;
        base.Unhover();
    }

    #endregion
}