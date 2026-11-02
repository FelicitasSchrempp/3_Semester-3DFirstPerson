using System.Collections;
using UnityEngine;

public class MultiInteractable : BaseInteractable
{
    #region Inspector

    [Min(0f)]
    [SerializeField] private float timeForNextInteraction = 0.5f;

    #endregion

    #region Private Variables

    private bool _isStillHovered;
    private Coroutine _cooldownRoutine;

    #endregion

    #region Unity Events

    protected override void OnDisable()
    {
        if (_cooldownRoutine != null)
        {
            StopCoroutine(_cooldownRoutine);
            _cooldownRoutine = null;
        }

        canInteract = true;
        base.OnDisable();
    }

    #endregion

    #region Interaction Methods

    public override void Interact()
    {
        if (!canInteract) return;

        base.Interact();

        canInteract = false;
        base.Unhover(); 
        _cooldownRoutine = StartCoroutine(WaitForInteraction());
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

    private IEnumerator WaitForInteraction()
    {
        yield return new WaitForSeconds(timeForNextInteraction);

        canInteract = true;
        _cooldownRoutine = null;

        if (_isStillHovered)
        {
            base.Hover();
        }
    }

    #endregion
}
