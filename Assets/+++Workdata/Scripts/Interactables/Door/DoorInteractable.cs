using System;
using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public event Action<int> OnInteract;
    
    public bool isOpen = false;
    public bool interactable = true;
    
    public void Interact()
    {
        if (!interactable) return;
        
        isOpen = !isOpen;

        if (isOpen)
        {
            Debug.Log("closing");
            OnInteract?.Invoke(20); // closing
        }
        else
        {
            Debug.Log("opening");
            OnInteract?.Invoke(10); // opening
        }
    }
}
