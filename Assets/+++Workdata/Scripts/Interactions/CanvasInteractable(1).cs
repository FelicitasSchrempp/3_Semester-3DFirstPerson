using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasInteractable : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float blendTime;
    
    [SerializeField] private Button firstSelectable;


    private Coroutine _fadeCoroutine;
    
    public void Interact()
    {
        FadeTo(1);
    }

    public void Deselect()
    {
        FadeTo(0, false);
    }

    private void FadeTo(float fadeValue, bool isInteractable = true)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(WaitForBlend(fadeValue, isInteractable));
    }

    IEnumerator WaitForBlend(float targetValue, bool isInteractable)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < blendTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blendTime;

            canvasGroup.alpha = LerpUtils.Lerp(startAlpha, targetValue, t);
            yield return null;
        }

        canvasGroup.alpha = targetValue;
        canvasGroup.interactable = isInteractable;

        Selectable newSelectable = firstSelectable;
        yield return null;
        newSelectable.Select();

    }
}