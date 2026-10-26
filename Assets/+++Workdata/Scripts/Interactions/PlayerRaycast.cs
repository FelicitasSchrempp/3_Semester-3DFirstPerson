using System;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    public event Action<BaseInteractable> InteractableChanged;
    
    #region Inspector

    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private LayerMask rayMask = ~0;
    
    #endregion

    #region Private Variables

    private BaseInteractable _currentInteractable;
    public BaseInteractable CurrentInteractable => _currentInteractable;

    #endregion

    #region Unity Events

    private void Update()
    {
        HandleRaycast();
    }

    private void OnDisable()
    {
        SetCurrentInteractable(null);
    }

    #endregion

    #region RaycastEvents

    private void HandleRaycast()
    {
        BaseInteractable currentInteractable = null;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, rayDistance, rayMask,
                QueryTriggerInteraction.Ignore))
        {
            currentInteractable = hit.collider.GetComponentInParent<BaseInteractable>();
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, currentInteractable != null ? Color.green : Color.red);
        
        if (currentInteractable == _currentInteractable) return;

        SetCurrentInteractable(currentInteractable);
    }

    private void SetCurrentInteractable(BaseInteractable newBaseInteractable)
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Unhover();
        }

        _currentInteractable = newBaseInteractable;

        if (_currentInteractable != null)
        {
            _currentInteractable.Hover();
        }
        
        InteractableChanged?.Invoke(_currentInteractable);
    }

    #endregion
}