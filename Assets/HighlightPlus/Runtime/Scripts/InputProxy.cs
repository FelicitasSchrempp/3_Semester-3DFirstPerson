using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
#endif

namespace HighlightPlus {

    public static class InputProxy {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

        static Vector3 lastPointerPosition;

        public static void Init() {
            if (!EnhancedTouch.EnhancedTouchSupport.enabled) {
                EnhancedTouch.EnhancedTouchSupport.Enable();
            }
        }

        public static Vector3 mousePosition {
            get {
                if (touchCount > 0) {
                    lastPointerPosition = EnhancedTouch.Touch.activeTouches[0].screenPosition;
                } else {
                    Mouse m = Mouse.current;
                    if (m != null) {
                        lastPointerPosition = m.position.ReadValue();
                    }
                }
                return lastPointerPosition;
            }
        }

        public static bool GetMouseButtonDown(int buttonIndex) {
            if (touchCount > 0) {
                return EnhancedTouch.Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began;
            } else {
                Mouse m = Mouse.current;
                if (m == null) return false;
                switch (buttonIndex) {
                    case 1: return m.rightButton.wasPressedThisFrame;
                    case 2: return m.middleButton.wasPressedThisFrame;
                    default: return m.leftButton.wasPressedThisFrame;
                }
            }
        }

        public static int touchCount { get { return EnhancedTouch.Touch.activeTouches.Count; } }

        public static int GetFingerIdFromTouch(int touchIndex) {
            EnhancedTouch.Touch touch = EnhancedTouch.Touch.activeTouches[touchIndex];
            return touch.finger.index;
        }

        public static bool GetKeyDown(string name) {
            Keyboard k = Keyboard.current;
            return k != null && ((KeyControl)k[name]).wasPressedThisFrame;
        }

        public static Vector2 movementAxis {
            get {
                Vector2 axis = Vector2.zero;
                Keyboard k = Keyboard.current;
                if (k == null) return axis;
                if (k.aKey.isPressed) axis.x = -1f;
                if (k.dKey.isPressed) axis.x = 1f;
                if (k.wKey.isPressed) axis.y = 1f;
                if (k.sKey.isPressed) axis.y = -1f;
                return axis;
            }
        }

        public static Vector2 mouseDelta {
            get {
                Mouse m = Mouse.current;
                return m != null ? m.delta.ReadValue() * 0.1f : Vector2.zero;
            }
        }

#else
        public static void Init() {}

        public static Vector3 mousePosition {
            get {
                return Input.mousePosition;
            }
        }

        public static bool GetMouseButtonDown(int buttonIndex) {
            return Input.GetMouseButtonDown(buttonIndex);
        }

        public static int touchCount {
            get { return Input.touchCount; }
        }

        public static int GetFingerIdFromTouch(int touchIndex) {
            return Input.GetTouch(touchIndex).fingerId;
        }

        public static bool GetKeyDown(string name) {
            return Input.GetKeyDown(name);
        }

        public static Vector2 movementAxis {
            get {
                Vector2 axis = Vector2.zero;
                if (Input.GetKey(KeyCode.A)) axis.x = -1f;
                if (Input.GetKey(KeyCode.D)) axis.x = 1f;
                if (Input.GetKey(KeyCode.W)) axis.y = 1f;
                if (Input.GetKey(KeyCode.S)) axis.y = -1f;
                return axis;
            }
        }

        public static Vector2 mouseDelta {
            get {
                return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
        }

#endif

    }
}
