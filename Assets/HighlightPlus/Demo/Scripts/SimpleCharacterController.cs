using UnityEngine;

namespace HighlightPlus.Demos {

    public class SimpleCharacterController : MonoBehaviour {
        public float moveSpeed = 5f;
        public float mouseSensitivity = 2f;
        public Transform playerCamera;
        private float rotationX = 0f;

        void Start () {
            Cursor.lockState = CursorLockMode.Locked; // Locks cursor to center
            Cursor.visible = true; // Keeps cursor visible
        }

        void Update () {
            // Movement input using WASD
            Vector2 movement = InputProxy.movementAxis;
            float moveX = movement.x, moveZ = movement.y;

            Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Mouse look input
            Vector2 mouseDelta = InputProxy.mouseDelta;
            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevents flipping

            playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            // Keep cursor locked in center
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }
}