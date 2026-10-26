using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
    [CustomEditor(typeof(HighlightManager))]
    public class HighlightManagerEditor : UnityEditor.Editor {

        SerializedProperty highlightOnHover, layerMask, raycastCamera, raycastSource, blockingLayerMask, minDistance, maxDistance, respectUI, unhighlightOnUI;
        SerializedProperty selectOnClick, selectedProfile, selectedAndHighlightedProfile, singleSelection, toggleOnClick, keepSelection;
        SerializedProperty detectionMode, proximityCenter, proximityRadius, proximityViewAngle;

        static readonly GUIContent lineOfSightLabel = new GUIContent("Line Of Sight Blockers", "Objects in these layers block the highlight if they are between the center and the object. Set to Nothing to disable the line of sight check.");
        static readonly GUIContent fallbackCameraLabel = new GUIContent("Raycast Camera", "Used as the center because Proximity Center is empty.");

        void OnEnable() {
            highlightOnHover = serializedObject.FindProperty("_highlightOnHover");
            layerMask = serializedObject.FindProperty("layerMask");
            raycastCamera = serializedObject.FindProperty("raycastCamera");
            raycastSource = serializedObject.FindProperty("raycastSource");
            blockingLayerMask = serializedObject.FindProperty("blockingLayerMask");
            minDistance = serializedObject.FindProperty("minDistance");
            maxDistance = serializedObject.FindProperty("maxDistance");
            respectUI = serializedObject.FindProperty("respectUI");
            selectOnClick = serializedObject.FindProperty("selectOnClick");
            selectedProfile = serializedObject.FindProperty("selectedProfile");
            selectedAndHighlightedProfile = serializedObject.FindProperty("selectedAndHighlightedProfile");
            singleSelection = serializedObject.FindProperty("singleSelection");
            toggleOnClick = serializedObject.FindProperty("toggle");
            keepSelection = serializedObject.FindProperty("keepSelection");
            unhighlightOnUI = serializedObject.FindProperty("unhighlightOnUI");
            detectionMode = serializedObject.FindProperty("detectionMode");
            proximityCenter = serializedObject.FindProperty("proximityCenter");
            proximityRadius = serializedObject.FindProperty("proximityRadius");
            proximityViewAngle = serializedObject.FindProperty("proximityViewAngle");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            bool proximity = detectionMode.intValue == (int)HighlightManager.DetectionMode.Proximity;
            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox(proximity ? "Only objects with a 3D collider can be highlighted automatically." : "Only objects with a collider can be highlighted automatically.", MessageType.Info);

            EditorGUILayout.PropertyField(detectionMode);
            if (proximity) {
                EditorGUILayout.PropertyField(proximityCenter);
                EditorGUILayout.PropertyField(proximityRadius);
                EditorGUILayout.PropertyField(proximityViewAngle);
                EditorGUILayout.PropertyField(layerMask);
                if (layerMask.intValue == -1) {
                    EditorGUILayout.HelpBox("Layer Mask includes every layer: the floor, walls and any other collider near the center can be highlighted. Choose the layers of the objects that should be highlighted.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(blockingLayerMask, lineOfSightLabel);
                if (proximityCenter.objectReferenceValue == null) {
                    EditorGUILayout.PropertyField(raycastCamera, fallbackCameraLabel);
                }
                if (!highlightOnHover.boolValue) {
                    EditorGUILayout.HelpBox("Automatic highlighting is turned off (highlightOnHover is false), so nothing will be highlighted.", MessageType.Warning);
                    if (GUILayout.Button("Turn On")) {
                        highlightOnHover.boolValue = true;
                    }
                }
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.PropertyField(layerMask);
            EditorGUILayout.PropertyField(raycastCamera);
            EditorGUILayout.PropertyField(raycastSource);
            EditorGUILayout.PropertyField(blockingLayerMask);
            EditorGUILayout.PropertyField(minDistance);
            EditorGUILayout.PropertyField(maxDistance);
            EditorGUILayout.PropertyField(respectUI);
            if (respectUI.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(unhighlightOnUI);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(highlightOnHover);
            EditorGUILayout.PropertyField(selectOnClick);
            if (selectOnClick.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(selectedProfile);
                EditorGUILayout.PropertyField(selectedAndHighlightedProfile);
                EditorGUILayout.PropertyField(singleSelection);
                EditorGUILayout.PropertyField(toggleOnClick);
                EditorGUILayout.PropertyField(keepSelection);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }


        [MenuItem("GameObject/Effects/Highlight Plus/Create Highlight Manager", false, 10)]
		static void CreateManager (MenuCommand menuCommand) {
			HighlightManager manager = Misc.FindObjectOfType<HighlightManager> ();
			if (manager == null) {
				GameObject managerGO = new GameObject ("HighlightPlusManager");
				manager = managerGO.AddComponent<HighlightManager> ();
				// Register root object for undo.
				Undo.RegisterCreatedObjectUndo (manager, "Create Highlight Plus Manager");
			}
			Selection.activeObject = manager;
		}

    }

}
