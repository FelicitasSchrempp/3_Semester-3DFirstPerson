using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum InputDeviceType { KeyboardMouse, Gamepad }

 public class InputDeviceTracker : MonoBehaviour
{
    [SerializeField] private float magnitudeThreshold = 0.2f;

    public InputDeviceType currentDevice = InputDeviceType.KeyboardMouse;

    public static event Action<InputDeviceType> DeviceTypeChanged;

    private void OnEnable() => InputSystem.onEvent += OnInputEvent;
    private void OnDisable() => InputSystem.onEvent -= OnInputEvent;

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        InputDeviceType? type = device switch
        {
            Gamepad => InputDeviceType.Gamepad,
            Keyboard or Mouse => InputDeviceType.KeyboardMouse,
            _ => null
        };

        if (type == null || type == currentDevice)
            return;

        foreach (var _ in eventPtr.EnumerateChangedControls(device, magnitudeThreshold))
        {
            currentDevice = type.Value;
            DeviceTypeChanged?.Invoke(currentDevice);
            return;
        }
    }
}