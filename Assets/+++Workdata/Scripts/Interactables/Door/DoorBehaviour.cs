using System;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    private Animator _animator;
    public DoorInteractable doorInteractable;
    private MeshCollider _collider;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<MeshCollider>();
    }
    
    private void OnEnable()
    {
        doorInteractable.OnInteract += SetAnimator;
    }

    private void OnDisable()
    {
        doorInteractable.OnInteract -= SetAnimator;
    }

    private void SetAnimator(int id)
    {
        if (id == 10)
        {
            _collider.enabled = false;
        }
        
        doorInteractable.interactable = false;
        
        _animator.SetTrigger("ActionTrigger");
        _animator.SetInteger("ActionID", id);
    }

    public void DoorClosed()
    {
        _collider.enabled = true;
    }

    public void DoorStoppedMoving()
    {
        Debug.Log("doorclosed");
        doorInteractable.interactable = true;
    }
}
