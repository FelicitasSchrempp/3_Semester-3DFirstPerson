using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class VolumeBlender : MonoBehaviour
{
    #region Inspector

    [Min(0.01f)]
    [SerializeField] private float blendSpeed = 2f;

    #endregion

    #region Private Variables

    private Volume _volume;
    private Coroutine _blendRoutine;

    #endregion

    #region Unity Events

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    #endregion

    #region Blend Methods

    public void BlendTo(float targetWeight)
    {
        if (_blendRoutine != null)
        {
            StopCoroutine(_blendRoutine);
        }

        _blendRoutine = StartCoroutine(BlendRoutine(Mathf.Clamp01(targetWeight)));
    }

    private IEnumerator BlendRoutine(float targetWeight)
    {
        while (!Mathf.Approximately(_volume.weight, targetWeight))
        {
            _volume.weight = Mathf.MoveTowards(_volume.weight, targetWeight, blendSpeed * Time.deltaTime);
            yield return null;
        }

        _volume.weight = targetWeight;
        _blendRoutine = null;
    }

    #endregion
}