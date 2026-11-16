using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    private Button currentSelectable;

    public void SetNewSelection(Button newSelectable)
    {
        currentSelectable = newSelectable;
        currentSelectable.Select();
    }
    
}
