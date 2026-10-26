using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HighlightPlus {

    [RequireComponent(typeof(HighlightEffect))]
    [DefaultExecutionOrder(100)]
    [HelpURL("https://kronnect.com/docs/highlight-plus/")]
    public class HighlightManager : MonoBehaviour {

        [Tooltip("Enables highlight when pointer is over this object.")]
        [SerializeField]
        bool _highlightOnHover = true;

        public bool highlightOnHover {
            get { return _highlightOnHover; }
            set {
                if (_highlightOnHover != value) {
                    _highlightOnHover = value;
                    if (!_highlightOnHover) {
                        ReleaseHoverEffect();
                        if (currentEffect != null) {
                            Highlight(false);
                        }
                        hoverEffect = null;
                    }

                }
            }
        }

        public enum DetectionMode {
            Pointer = 0,
            Proximity = 1
        }

        [Tooltip("Pointer highlights the object under the mouse or at the camera center. Proximity highlights the nearest object around a center transform, usually the player.")]
        public DetectionMode detectionMode = DetectionMode.Pointer;
        [Tooltip("Center of the proximity search, usually the player. If empty, the camera is used.")]
        public Transform proximityCenter;
        [Tooltip("Maximum distance from the center to an object for it to be highlighted.")]
        public float proximityRadius = 2f;
        [Tooltip("Only objects within this angle, measured from the forward direction of the center, are highlighted. 360 = all directions.")]
        [Range(1, 360)]
        public float proximityViewAngle = 360f;

        /// <summary>
        /// Object currently highlighted by the manager's automatic hover or proximity detection (the effect target), or null.
        /// </summary>
        public GameObject highlightedObject {
            get {
                if (hoverEffect == null || !hoverEffect.isActiveAndEnabled || !hoverEffect.highlighted) return null;
                Transform target = hoverEffect.target;
                return target != null ? target.gameObject : null;
            }
        }

        public LayerMask layerMask = -1;
        public Camera raycastCamera;
        public RayCastSource raycastSource = RayCastSource.MousePosition;
        [Tooltip("Objects in this layer mask will block the highlight if they are between the camera and the target.")]
        public LayerMask blockingLayerMask = -1;
        [Tooltip("Minimum distance for target.")]
        public float minDistance;
        [Tooltip("Maximum distance for target. 0 = infinity")]
        public float maxDistance;
        [Tooltip("Blocks interaction if pointer is over an UI element")]
        public bool respectUI = true;
        [Tooltip("Unhighlights the object when the pointer is over a UI element")]
        public bool unhighlightOnUI;

        [Tooltip("If the object will be selected by clicking with mouse or tapping on it.")]
        public bool selectOnClick;
        [Tooltip("Optional profile for objects selected by clicking on them")]
        public HighlightProfile selectedProfile;
        [Tooltip("Profile to use when object is selected and highlighted.")]
        public HighlightProfile selectedAndHighlightedProfile;
        [Tooltip("Automatically deselects other previously selected objects")]
        public bool singleSelection;
        [Tooltip("Toggles selection on/off when clicking object")]
        public bool toggle;
        [Tooltip("Keeps current selection when clicking outside of any selectable object")]
        public bool keepSelection = true;

        HighlightEffect baseEffect, currentEffect, hoverEffect;
        Transform currentObject;

        struct ProximityCandidate {
            public Transform transform;
            public Transform key;
            public HighlightEffect effect;
            public Bounds bounds;
            public float distance;
            public float centerSqrDistance;
        }

        const float ProximitySwitchMargin = 0.05f;
        Collider[] proximityColliders = new Collider[32];
        RaycastHit[] proximityHits = new RaycastHit[16];
        readonly List<ProximityCandidate> proximityCandidates = new List<ProximityCandidate>();
        static readonly Comparison<ProximityCandidate> proximityComparison = CompareProximityCandidates;
#if HP2_PHYSICS2D
        RaycastHit2D[] hitInfo2D;
#endif
        static RaycastHit blockingHit;
        readonly List<RaycastResult> cachedRaycastResults = new List<RaycastResult>();
        PointerEventData cachedPointerEventData;
        EventSystem cachedEventSystem;

        public readonly static List<HighlightEffect> selectedObjects = new List<HighlightEffect>();
        public event OnObjectSelectionEvent OnObjectSelected;
        public event OnObjectSelectionEvent OnObjectUnSelected;
        public event OnObjectHighlightEvent OnObjectHighlightStart;
        public event OnObjectHighlightEvent OnObjectHighlightStay;
        public event OnObjectHighlightEvent OnObjectHighlightEnd;
        public event OnObjectClickEvent OnObjectClicked;
        public static int lastTriggerFrame;

        static HighlightManager _instance;
        public static HighlightManager instance {
            get {
                if (_instance == null) {
                    _instance = Misc.FindObjectOfType<HighlightManager>();
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        static void DomainReloadDisabledSupport () {
            selectedObjects.Clear();
            lastTriggerFrame = 0;
            _instance = null;
        }

        void OnEnable () {
            currentObject = null;
            currentEffect = null;
            if (baseEffect == null) {
                if (!TryGetComponent(out baseEffect)) {
                    baseEffect = gameObject.AddComponent<HighlightEffect>();
                }
            }
            if (raycastCamera == null) {
                raycastCamera = GetCamera();
                if (raycastCamera == null && !(detectionMode == DetectionMode.Proximity && proximityCenter != null)) {
                    Debug.LogError("Highlight Manager: no camera found!");
                }
            }
#if HP2_PHYSICS2D
            hitInfo2D = new RaycastHit2D[1];
#endif
            InputProxy.Init();
        }


        void OnDisable () {
            SwitchesObject(null);
            internal_DeselectAll();
        }

        void Update () {
            if (detectionMode == DetectionMode.Proximity) {
                UpdateProximity();
                return;
            }

            if (raycastCamera == null)
                return;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (respectUI) {
                EventSystem es = EventSystem.current;
                if (es == null) {
                    es = CreateEventSystem();
                }
                cachedRaycastResults.Clear();
                if (cachedPointerEventData == null || cachedEventSystem != es) {
                    cachedPointerEventData = new PointerEventData(es);
                    cachedEventSystem = es;
                }
                Vector3 cameraPos = raycastCamera.transform.position;
                if (raycastSource == RayCastSource.MousePosition) {
                    cachedPointerEventData.position = InputProxy.mousePosition;
                } else {
                    cachedPointerEventData.position = new Vector2(raycastCamera.pixelWidth * 0.5f, raycastCamera.pixelHeight * 0.5f);
                }
                es.RaycastAll(cachedPointerEventData, cachedRaycastResults);
                int hitCount = cachedRaycastResults.Count;
                // check UI blocker
                bool blocked = false;
                for (int k = 0; k < hitCount; k++) {
                    RaycastResult rr = cachedRaycastResults[k];
                    if (rr.module is UnityEngine.UI.GraphicRaycaster) {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) {
                    if (unhighlightOnUI && currentEffect != null && currentEffect.highlighted) {
                        currentEffect.SetHighlighted(false);
                    }
                    return;
                }

                // compute blocking ray for distance checks
                Ray blockingRay;
                if (raycastSource == RayCastSource.MousePosition) {
                    blockingRay = raycastCamera.ScreenPointToRay(InputProxy.mousePosition);
                } else {
                    blockingRay = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
                }
                float uiBlockingDistance = GetBlockingDistance(blockingRay, maxDistance > 0 ? maxDistance : raycastCamera.farClipPlane);

                // look for our gameobject
                for (int k = 0; k < hitCount; k++) {
                    RaycastResult rr = cachedRaycastResults[k];
                    float distance = Vector3.Distance(rr.worldPosition, cameraPos);
                    if (distance < minDistance || (maxDistance > 0 && distance > maxDistance) || distance > uiBlockingDistance) continue;

                    GameObject theGameObject = rr.gameObject;
                    if ((layerMask & (1 << rr.gameObject.layer)) == 0) continue;

                    // is this object state controller by Highlight Trigger?
                    HighlightTrigger trigger = theGameObject.GetComponent<HighlightTrigger>();
                    if (trigger != null) return;

                    UpdateHitPosition(theGameObject.transform, rr.worldPosition, Misc.vector3Zero);

                    // Toggles selection
                    Transform t = theGameObject.transform;
                    if (InputProxy.GetMouseButtonDown(0)) {
                        if (selectOnClick) {
                            ToggleSelection(t, !toggle);
                        }
                    } else 
                        // Check if the object has a Highlight Effect
                        if (t != currentObject) {
                            SwitchesObject(t);
                        }
                }
            }

            // if not blocked by UI and no hit found, fallback to raycast (required if no PhysicsRaycaster is present on the camera)
#endif

            Ray ray;
            {
#if !(ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER)
                Vector2 screenPos = raycastSource == RayCastSource.MousePosition ? (Vector2)InputProxy.mousePosition : new Vector2(raycastCamera.pixelWidth * 0.5f, raycastCamera.pixelHeight * 0.5f);
                if (!CanInteractAt(screenPos)) {
                    if (unhighlightOnUI && currentEffect != null && currentEffect.highlighted) {
                        currentEffect.SetHighlighted(false);
                    }
                    return;
                }
#endif
                if (raycastSource == RayCastSource.MousePosition) {
                    ray = raycastCamera.ScreenPointToRay(InputProxy.mousePosition);
                }
                else {
                    ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
                }
            }

            VerifyHighlightStay();

            float blockingDistance = GetBlockingDistance(ray, maxDistance > 0 ? maxDistance : raycastCamera.farClipPlane);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxDistance > 0 ? maxDistance : raycastCamera.farClipPlane, layerMask) && Vector3.Distance(hitInfo.point, ray.origin) >= minDistance && hitInfo.distance <= blockingDistance) {
                Transform t = hitInfo.collider.transform;
                // is this object state controller by Highlight Trigger?
                if (t.TryGetComponent(out HighlightTrigger _)) {
                    return;
                }

                // Toggles selection
                if (InputProxy.GetMouseButtonDown(0)) {
                    if (selectOnClick) {
                        ToggleSelection(t, !toggle);
                    }
                }
                else
                    // Check if the object has a Highlight Effect
                    if (t != currentObject) {
                    SwitchesObject(t);
                }
                UpdateHitPosition(t, hitInfo.point, hitInfo.normal);
                return;
            }
#if HP2_PHYSICS2D
            else // check sprites
            if (Physics2D.GetRayIntersectionNonAlloc(ray, hitInfo2D, maxDistance > 0 ? maxDistance : raycastCamera.farClipPlane, layerMask) > 0 && Vector3.Distance(hitInfo2D[0].point, ray.origin) >= minDistance && Vector3.Distance(hitInfo2D[0].point, ray.origin) <= blockingDistance) {
                Transform t = hitInfo2D[0].collider.transform;
                // is this object state controller by Highlight Trigger?
                if (t.TryGetComponent(out HighlightTrigger _)) {
                    return;
                }

                // Toggles selection
                if (InputProxy.GetMouseButtonDown(0)) {
                    if (selectOnClick) {
                        ToggleSelection(t, !toggle);
                    }
                }
                else
                    // Check if the object has a Highlight Effect
                    if (t != currentObject) {
                    SwitchesObject(t);
                }
                UpdateHitPosition(t, hitInfo2D[0].point, Misc.vector3Zero);
                return;
            }
#endif

            // no hit
            if (selectOnClick && !keepSelection && InputProxy.GetMouseButtonDown(0) && lastTriggerFrame < Time.frameCount) {
                internal_DeselectAll();
            }
            SwitchesObject(null);
        }


#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        EventSystem CreateEventSystem() {
            GameObject eo = new GameObject("Event System created by Highlight Plus", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            return eo.GetComponent<EventSystem>();
        }
#endif
        void VerifyHighlightStay () {
            if (currentObject == null || currentEffect == null || !currentEffect.highlighted) return;
            if (OnObjectHighlightStay != null && !OnObjectHighlightStay(currentObject.gameObject)) {
                SwitchesObject(null);
            }
        }

        // Proximity detection

        Transform GetProximityCenter () {
            if (proximityCenter != null) return proximityCenter;
            return raycastCamera != null ? raycastCamera.transform : null;
        }

        void UpdateProximity () {
            Transform center = GetProximityCenter();
            if (center == null || !highlightOnHover || !(proximityRadius > 0f) || float.IsInfinity(proximityRadius)) {
                SwitchesObject(null);
                return;
            }

            Transform stayRejectedKey = null;
            if (currentObject != null) {
                Transform key = ResolveProximityTarget(currentObject, out _);
                VerifyHighlightStay();
                if (currentObject == null) {
                    stayRejectedKey = key;
                }
            }

            Vector3 origin = center.position;
            int colliderCount;
            while (true) {
                colliderCount = Physics.OverlapSphereNonAlloc(origin, proximityRadius, proximityColliders, layerMask);
                if (colliderCount < proximityColliders.Length) break;
                proximityColliders = new Collider[proximityColliders.Length * 2];
            }

            proximityCandidates.Clear();
            bool checkAngle = proximityViewAngle < 360f;
            float halfAngle = proximityViewAngle * 0.5f;
            Vector3 forward = center.forward;
            for (int k = 0; k < colliderCount; k++) {
                Collider col = proximityColliders[k];
                if (col == null) continue;
                Transform t = col.transform;
                if (t.IsChildOf(center) || center.IsChildOf(t)) continue;
                if (t.TryGetComponent(out HighlightTrigger _)) continue;
                Transform key = ResolveProximityTarget(t, out HighlightEffect effect);
                if (key == stayRejectedKey || effect == null || effect.ignore) continue;
                Bounds bounds = col.bounds;
                Vector3 toCenter = bounds.center - origin;
                if (checkAngle && toCenter.sqrMagnitude > 0f && Vector3.Angle(forward, toCenter) > halfAngle) continue;

                ProximityCandidate candidate;
                candidate.transform = t;
                candidate.key = key;
                candidate.effect = effect;
                candidate.bounds = bounds;
                candidate.distance = Mathf.Sqrt(bounds.SqrDistance(origin));
                candidate.centerSqrDistance = toCenter.sqrMagnitude;

                int existing = -1;
                for (int j = 0; j < proximityCandidates.Count; j++) {
                    if (proximityCandidates[j].key == key) {
                        existing = j;
                        break;
                    }
                }
                if (existing < 0) {
                    proximityCandidates.Add(candidate);
                } else if (CompareProximityCandidates(candidate, proximityCandidates[existing]) < 0) {
                    proximityCandidates[existing] = candidate;
                }
            }
            proximityCandidates.Sort(proximityComparison);

            Transform currentKey = currentObject != null ? ResolveProximityTarget(currentObject, out _) : null;
            int currentIndex = -1;
            if (currentKey != null) {
                for (int k = 0; k < proximityCandidates.Count; k++) {
                    if (proximityCandidates[k].key == currentKey) {
                        currentIndex = k;
                        break;
                    }
                }
            }
            bool currentValid = currentIndex >= 0 && IsActiveHoverHighlight(proximityCandidates[currentIndex].effect) && HasLineOfSight(origin, center, proximityCandidates[currentIndex]);

            int bestIndex = -1;
            int candidateCount = proximityCandidates.Count;
            for (int k = 0; k < candidateCount; k++) {
                if (k == currentIndex && currentValid) {
                    bestIndex = k;
                    break;
                }
                ProximityCandidate candidate = proximityCandidates[k];
                if (currentValid && candidate.distance + ProximitySwitchMargin >= proximityCandidates[currentIndex].distance) continue;
                if (!HasLineOfSight(origin, center, candidate)) continue;
                if (!AcceptsProximityCandidate(candidate)) continue;
                if (candidate.transform == null) continue;
                bestIndex = k;
                break;
            }

            if (bestIndex < 0) {
                SwitchesObject(null);
                return;
            }
            if (bestIndex == currentIndex && currentValid) return;
            SwitchesObject(proximityCandidates[bestIndex].transform, true);
        }

        bool IsActiveHoverHighlight (HighlightEffect effect) {
            return hoverEffect != null && hoverEffect == effect && hoverEffect.isActiveAndEnabled && hoverEffect.highlighted;
        }

        Transform ResolveProximityTarget (Transform t, out HighlightEffect effect) {
            if (t.TryGetComponent(out effect)) return t;
            HighlightEffect parentEffect = t.GetComponentInParent<HighlightEffect>();
            if (parentEffect != null && parentEffect.Includes(t)) {
                effect = parentEffect;
                return parentEffect.transform;
            }
            effect = baseEffect;
            return t;
        }

        bool AcceptsProximityCandidate (ProximityCandidate candidate) {
            if (OnObjectHighlightStart == null) return true;
            GameObject eventTarget;
            if (candidate.key != candidate.transform) {
                Transform target = candidate.effect.target;
                if (target == null) return true;
                eventTarget = target.gameObject;
            } else {
                eventTarget = candidate.transform.gameObject;
            }
            return OnObjectHighlightStart(eventTarget);
        }

        bool HasLineOfSight (Vector3 origin, Transform center, ProximityCandidate candidate) {
            if (blockingLayerMask == 0) return true;
            Vector3 direction = candidate.bounds.center - origin;
            float length = direction.magnitude;
            if (length < 0.0001f) return true;
            direction /= length;
            while (true) {
                int hitCount = Physics.RaycastNonAlloc(origin, direction, proximityHits, length, blockingLayerMask, QueryTriggerInteraction.Ignore);
                for (int k = 0; k < hitCount; k++) {
                    Transform hit = proximityHits[k].collider.transform;
                    if (hit.IsChildOf(candidate.key) || hit.IsChildOf(center) || center.IsChildOf(hit)) continue;
                    return false;
                }
                if (hitCount < proximityHits.Length) return true;
                proximityHits = new RaycastHit[proximityHits.Length * 2];
            }
        }

        static int CompareProximityCandidates (ProximityCandidate a, ProximityCandidate b) {
            int c = a.distance.CompareTo(b.distance);
            if (c != 0) return c;
            c = a.centerSqrDistance.CompareTo(b.centerSqrDistance);
            if (c != 0) return c;
            Vector3 ca = a.bounds.center;
            Vector3 cb = b.bounds.center;
            c = ca.x.CompareTo(cb.x);
            if (c != 0) return c;
            c = ca.y.CompareTo(cb.y);
            if (c != 0) return c;
            return ca.z.CompareTo(cb.z);
        }

        void OnDrawGizmosSelected () {
            if (detectionMode != DetectionMode.Proximity || !(proximityRadius > 0f) || float.IsInfinity(proximityRadius)) return;
            Transform center = proximityCenter;
            if (center == null) {
                Camera cam = raycastCamera != null ? raycastCamera : GetCamera();
                if (cam == null) return;
                center = cam.transform;
            }
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center.position, proximityRadius);
        }

        void ReleaseHoverEffect () {
            if (hoverEffect == null || hoverEffect == currentEffect) return;
            HighlightEffect selectionEffect = currentEffect;
            currentEffect = hoverEffect;
            Highlight(false);
            currentEffect = selectionEffect;
        }

        void SwitchesObject (Transform newObject, bool skipStartEvent = false) {
            ReleaseHoverEffect();
            HighlightEffect previousHover = hoverEffect;
            hoverEffect = null;
            if (currentEffect != null) {
                if (highlightOnHover || currentEffect == previousHover) {
                    Highlight(false);
                }
                currentEffect = null;
            }
            currentObject = newObject;
            if (newObject == null) return;
            if (newObject.TryGetComponent(out HighlightTrigger ht) && ht.enabled) {
                return;
            }

            if (!newObject.TryGetComponent(out HighlightEffect otherEffect)) {
                // Check if there's a parent highlight effect that includes this object
                HighlightEffect parentEffect = newObject.GetComponentInParent<HighlightEffect>();
                if (parentEffect != null && parentEffect.Includes(newObject)) {
                    currentEffect = parentEffect;
                    if (highlightOnHover) {
                        Highlight(true, skipStartEvent);
                        hoverEffect = currentEffect;
                    }
                    return;
                }
            }
            currentEffect = otherEffect != null ? otherEffect : baseEffect;
            baseEffect.enabled = currentEffect == baseEffect;
            currentEffect.SetTarget(currentObject);

            if (highlightOnHover) {
                Highlight(true, skipStartEvent);
                hoverEffect = currentEffect;
            }
        }

        void UpdateHitPosition (Transform target, Vector3 positionWS, Vector3 normalWS) {
            if (currentEffect == null) return;
            Vector3 localPosition = target.InverseTransformPoint(positionWS);
            currentEffect.SetHitPosition(target, localPosition, Misc.vector3Zero, normalWS);

            if (InputProxy.GetMouseButtonDown(0) && OnObjectClicked != null) {
                OnObjectClicked(target.gameObject, positionWS, normalWS);
            }
        }

#if !(ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER)
        bool CanInteractAt (Vector2 screenPosition) {
            if (!respectUI) return true;
            EventSystem es = EventSystem.current;
            if (es == null) return true;
            if (Application.isMobilePlatform && InputProxy.touchCount > 0) {
                if (es.IsPointerOverGameObject(InputProxy.GetFingerIdFromTouch(0))) {
                    return false;
                }
                if (raycastSource == RayCastSource.CameraDirection) {
                    return !IsUIAtScreenPosition(es, screenPosition);
                }
                return true;
            }
            if (raycastSource == RayCastSource.MousePosition) {
                return !es.IsPointerOverGameObject(-1);
            }
            return !IsUIAtScreenPosition(es, screenPosition);
        }

        bool IsUIAtScreenPosition (EventSystem es, Vector2 screenPosition) {
            cachedRaycastResults.Clear();
            if (cachedPointerEventData == null || cachedEventSystem != es) {
                cachedPointerEventData = new PointerEventData(es);
                cachedEventSystem = es;
            }
            cachedPointerEventData.position = screenPosition;
            es.RaycastAll(cachedPointerEventData, cachedRaycastResults);
            int hitCount = cachedRaycastResults.Count;
            for (int k = 0; k < hitCount; k++) {
                RaycastResult rr = cachedRaycastResults[k];
                if (rr.module is UnityEngine.UI.GraphicRaycaster) {
                    return true;
                }
            }
            return false;
        }
#endif

        float GetBlockingDistance (Ray ray, float maxRayDistance) {
            if (blockingLayerMask == 0) return float.MaxValue;
            if (Physics.Raycast(ray, out blockingHit, maxRayDistance, blockingLayerMask)) {
                return blockingHit.distance + 0.01f;
            }
            return float.MaxValue;
        }

        void ToggleSelection (Transform t, bool forceSelection) {

            if (t == null) return;
            // We need a highlight effect on each selected object
            HighlightEffect hb = t.GetComponent<HighlightEffect>();
            if (hb == null) {
                HighlightEffect parentEffect = t.GetComponentInParent<HighlightEffect>();
                if (parentEffect != null && parentEffect.Includes(t)) {
                    hb = parentEffect;
                    if (hb.previousSettings == null) {
                        hb.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                    }
                    hb.previousSettings.Save(hb);
                }
                else {
                    hb = t.gameObject.AddComponent<HighlightEffect>();
                    hb.camerasLayerMask = baseEffect.camerasLayerMask;
                    hb.ignoreObjectVisibility = baseEffect.ignoreObjectVisibility;
                    hb.reflectionProbes = baseEffect.reflectionProbes;
                    hb.normalsOption = baseEffect.normalsOption;
                    hb.optimizeSkinnedMesh = baseEffect.optimizeSkinnedMesh;
                    hb.GPUInstancing = baseEffect.GPUInstancing;
                    
                    hb.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                    // copy default highlight effect settings from this manager into this highlight plus component
                    hb.previousSettings.Save(baseEffect);
                    hb.previousSettings.Load(hb);
                }
            }

            bool currentState = hb.isSelected;
            bool newState = forceSelection ? true : !currentState;
            if (newState == currentState) return;

            if (newState) {
                if (OnObjectSelected != null && !OnObjectSelected(t.gameObject)) return;
            }
            else {
                if (OnObjectUnSelected != null && !OnObjectUnSelected(t.gameObject)) return;
            }

            if (singleSelection) {
                internal_DeselectAll();
            }

            currentEffect = hb;
            if (hoverEffect != null && hoverEffect != hb && hoverEffect.target == hb.target) {
                hoverEffect = hb;
            }
            currentEffect.isSelected = newState;
            baseEffect.enabled = false;

            if (currentEffect.isSelected) {
                if (currentEffect.previousSettings == null) {
                    currentEffect.previousSettings = ScriptableObject.CreateInstance<HighlightProfile>();
                }
                hb.previousSettings.Save(hb);

                if (!selectedObjects.Contains(currentEffect)) {
                    selectedObjects.Add(currentEffect);
                }
            }
            else {
                if (currentEffect.previousSettings != null) {
                    currentEffect.previousSettings.Load(hb);
                }
                if (selectedObjects.Contains(currentEffect)) {
                    selectedObjects.Remove(currentEffect);
                }
            }

            Highlight(newState);
        }

        void Highlight (bool state, bool skipStartEvent = false) {
            if (currentEffect == null) return;

            if (state) {
                if (!currentEffect.highlighted && !skipStartEvent) {
                    if (OnObjectHighlightStart != null && currentEffect.target != null) {
                        if (!OnObjectHighlightStart(currentEffect.target.gameObject)) {
                            currentObject = null; // allows re-checking so it keeps checking with the event
                            return;
                        }
                    }
                }
            }
            else {
                if (currentEffect.highlighted) {
                    if (OnObjectHighlightEnd != null && currentEffect.target != null) {
                        OnObjectHighlightEnd(currentEffect.target.gameObject);
                    }
                }
            }
            if (selectOnClick || currentEffect.isSelected) {
                if (currentEffect.isSelected) {
                    if (state && selectedAndHighlightedProfile != null) {
                        selectedAndHighlightedProfile.Load(currentEffect);
                    }
                    else if (selectedProfile != null) {
                        selectedProfile.Load(currentEffect);
                    }
                    else {
                        currentEffect.previousSettings.Load(currentEffect);
                    }
                    if (currentEffect.highlighted && currentEffect.fading != HighlightEffect.FadingState.FadingOut) {
                        currentEffect.UpdateMaterialProperties();
                    }
                    else {
                        currentEffect.SetHighlighted(true);
                    }
                    return;
                }
                else if (!highlightOnHover) {
                    currentEffect.SetHighlighted(false);
                    return;
                }
            }
            currentEffect.SetHighlighted(state);
        }

        public static Camera GetCamera () {
            Camera raycastCamera = Camera.main;
            if (raycastCamera == null) {
                raycastCamera = Misc.FindObjectOfType<Camera>();
            }
            return raycastCamera;
        }

        void internal_DeselectAll () {
            int count = selectedObjects.Count;
            for (int k = 0; k < count; k++) {
                HighlightEffect hb = selectedObjects[k];
                if (hb != null && hb.gameObject != null) {
                    if (OnObjectUnSelected != null) {
                        if (!OnObjectUnSelected(hb.gameObject)) continue;
                    }
                    hb.RestorePreviousHighlightEffectSettings();
                    hb.isSelected = false;
                    hb.SetHighlighted(false);
                }
            }
            selectedObjects.Clear();
        }

        /// <summary>
        /// Deselects any selected object in the scene
        /// </summary>
        public static void DeselectAll () {
            if (instance != null) {
                _instance.internal_DeselectAll();
                return;
            }

            foreach (HighlightEffect hb in selectedObjects) {
                if (hb != null && hb.gameObject != null) {
                    hb.RestorePreviousHighlightEffectSettings();
                    hb.isSelected = false;
                    hb.SetHighlighted(false);
                }
            }
            selectedObjects.Clear();
        }

        /// <summary>
        /// Unselects all objects in the scene
        /// </summary>
        public void UnselectObjects () {
            DeselectAll();
        }


        /// <summary>
        /// Manually causes highlight manager to select an object
        /// </summary>
        public void SelectObject (Transform t) {
            ToggleSelection(t, true);
        }

        /// <summary>
        /// Manually causes highlight manager to select multiple objects
        /// </summary>
        /// <param name="objects">Array of objects to select</param>
        public void SelectObjects (Transform[] objects) {
            foreach (var obj in objects) {
                SelectObject(obj);
            }
        }

        public void SelectObjects (List<Transform> objects) {
            foreach (var obj in objects) {
                SelectObject(obj);
            }
        }

        /// <summary>
        /// Manually causes highlight manager to toggle selection on an object
        /// </summary>
        public void ToggleObject (Transform t) {
            ToggleSelection(t, false);
        }

        /// <summary>
        /// Manually causes highlight manager to unselect an object
        /// </summary>
        public void UnselectObject (Transform t) {
            if (t == null) return;
            HighlightEffect hb = t.GetComponent<HighlightEffect>();
            if (hb == null) return;

            if (hb.isSelected) {
                ToggleSelection(t, false);
            }
        }


    }

}