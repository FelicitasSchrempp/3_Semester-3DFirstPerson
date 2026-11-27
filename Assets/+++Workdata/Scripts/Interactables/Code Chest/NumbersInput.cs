using TMPro;
using UnityEngine;

public class NumbersInput : MonoBehaviour
{
    private TextMeshProUGUI _numbersDisplay;

    private string _fullInput;
    public string correctInput;

    private void Awake()
    {
        _numbersDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AddNumber(string number)
    {
        if (_fullInput == null)
        {
            _fullInput = number;
        }
        else
        {
            _fullInput += number;
        }
        
        SetText();
    }

    public void DeleteNumber()
    {
        if (_fullInput == null) return;
        
        _fullInput = _fullInput.Substring(0, _fullInput.Length - 1);

        if (_fullInput.Length == 0)
        {
            _fullInput = null;
        }
        
        SetText();
    }

    public void DeleteAllNumbers()
    {
        _fullInput = null;
        
        SetText();
    }

    private void SetText()
    {
        _numbersDisplay.text = _fullInput;
    }
}
