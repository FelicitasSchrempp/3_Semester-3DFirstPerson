using UnityEngine;

namespace HighlightPlus.Demos {

    public class ProximityPickupExample : MonoBehaviour {

        public HighlightManager manager;
        [Tooltip("These objects are never highlighted: the next nearest object is highlighted instead.")]
        public GameObject[] lockedObjects;

        void OnEnable () {
            if (manager != null) {
                manager.OnObjectHighlightStart += CanHighlight;
            }
        }

        void OnDisable () {
            if (manager != null) {
                manager.OnObjectHighlightStart -= CanHighlight;
            }
        }

        bool CanHighlight (GameObject obj) {
            return System.Array.IndexOf(lockedObjects, obj) < 0;
        }

        void Update () {
            if (InputProxy.GetKeyDown("e")) {
                PickUp();
            }
        }

        public void PickUp () {
            GameObject obj = manager.highlightedObject;
            if (obj != null) {
                obj.SetActive(false);
            }
        }
    }
}
