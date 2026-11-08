using UnityEngine;
using UnityEngine.Events;

public class DoorBehavior : MonoBehaviour
{
    public static int Hash_ActionID = Animator.StringToHash("ActionID");
    public static int Hash_ActionTrigger = Animator.StringToHash("ActionTrigger");
    
    public UnityEvent DoInteractAgain;
    
    public enum DoorStates {Open, Closed}
    public enum DoorType {Slide, Rotate, Double}
    
    public DoorStates doorStates;
    public DoorType doorType;

    public Vector3 doorOpenPosition;
    public Vector3 doorClosePosition;
    
    public Vector3 doorOpenRotation;
    public Vector3 doorCloseRotation;

    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        
        _animator.enabled = false;

        if (doorType == DoorType.Slide || doorType == DoorType.Double)
        {
            if (doorStates == DoorStates.Open)
            {
                gameObject.transform.localPosition = doorOpenPosition;
            }
        
            else if (doorStates == DoorStates.Closed)
            {
                gameObject.transform.localPosition = doorClosePosition;
            }
        }
        else
        {
            if (doorStates == DoorStates.Open)
            {
                gameObject.transform.localRotation = Quaternion.Euler(doorOpenRotation);
            }
        
            else if (doorStates == DoorStates.Closed)
            {
                gameObject.transform.localRotation = Quaternion.Euler(doorCloseRotation);
            }
        }
    }

    public void ToggleDoor()
    {
        if (doorStates == DoorStates.Open)
        {
            //gameObject.transform.localPosition = doorClosePosition;
            doorStates = DoorStates.Closed;
        }
        
        else if (doorStates == DoorStates.Closed)
        {
            Debug.Log("Open Door");
           // gameObject.transform.localPosition = doorOpenPosition;
            doorStates = DoorStates.Open;
        }
    }

    public void ToggleDoorAnimation()
    {
        _animator.enabled = true;
        
        if (doorStates == DoorStates.Open)
        {
            AnimationSetAction(20);
        }
        
        else if (doorStates == DoorStates.Closed)
        {
            AnimationSetAction(10);
        }
    }
    
    public void AnimationSetAction(int ID)
    {
        _animator.SetTrigger(Hash_ActionTrigger);
        _animator.SetInteger(Hash_ActionID, ID);
    }
}