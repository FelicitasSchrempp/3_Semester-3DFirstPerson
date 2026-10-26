using UnityEngine;
using UnityEngine.Events;


public abstract class BaseInteractable : MonoBehaviour
{
    #region Inspector

    [SerializeField] protected bool canInteract = true;

    [Header("Events")]
    [SerializeField] protected UnityEvent onInteracted = new UnityEvent();
    [SerializeField] protected UnityEvent onHovered = new UnityEvent();
    [SerializeField] protected UnityEvent onUnhovered = new UnityEvent();

    #endregion

    #region Private Variables

    protected bool _isHovered;

    public bool CanInteract => canInteract;
    public bool IsHovered => _isHovered;

    #endregion

    #region Unity Events

    protected virtual void OnDisable()
    {
        Unhover();
    }

    #endregion

    #region Interaction Methods

    public virtual void Interact()
    {
        if (!canInteract) return;

        onInteracted.Invoke();
    }

    public virtual void Hover()
    {
        if (_isHovered || !canInteract) return;

        _isHovered = true;
        onHovered.Invoke();
    }

    public virtual void Unhover()
    {
        if (!_isHovered) return;

        _isHovered = false;
        onUnhovered.Invoke();
    }

    #endregion
}