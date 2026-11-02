using System;
using UnityEngine;
using UnityEngine.Events;

public class Interaction : MonoBehaviour
{
    #region Inspector

    [Header("References")] 
    public string interactableId;
    
    [SerializeField]
    private InteractionGroup[] interactionGroups;

    [SerializeField] private int interactionIndex;
    
    #endregion

    #region Interaction Functions

    public void Execute()
    {
        if (interactionGroups.Length < interactionIndex) return;
        
        InteractionGroup interactionGroup = interactionGroups[interactionIndex];
        interactionGroup.onInteracted?.Invoke();

        if (interactionGroup.nextInteractionId != -1)
        {
            interactionIndex = interactionGroup.nextInteractionId;
        }
        
    }


    public void SetInteractionIndex(int index)
    {
        interactionIndex = index;
    }

    public void SetInteractionIndexAndExecute(int index)
    {
        interactionIndex = index;
        Execute();
    }

    #endregion
}

[Serializable]
public class InteractionGroup
{
    public string interactionName;
    public UnityEvent onInteracted;
    public int nextInteractionId = -1;
}
