/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ProceduralSpider
{
    [CustomEditor(typeof(JointHinge))]
    [CanEditMultipleObjects]
    public class JointHingeEditor : Editor
    {
        private SerializedProperty rotationAxisProperty;
        private SerializedProperty minAngleProperty;
        private SerializedProperty maxAngleProperty;

        private void OnEnable()
        {
            rotationAxisProperty = serializedObject.FindProperty("rotationAxis");
            minAngleProperty = serializedObject.FindProperty("minAngle");
            maxAngleProperty = serializedObject.FindProperty("maxAngle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawCustomInspector();
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        private void DrawCustomInspector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(rotationAxisProperty, new GUIContent("RotationAxis", "The axis around which this hinge joint rotates."));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            CustomPropertySlider(minAngleProperty, maxAngleProperty, -90, 90, new GUIContent("Angle Constraints", "The lower and upper bound for our hinge joint."));
        }

        static private void CustomPropertySlider(SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit, GUIContent label)
        {
            Debug.Assert(minLimit <= 0 && 0 <= maxLimit);
            float minSlider = minProperty.floatValue;
            float maxSlider = maxProperty.floatValue;
            float minSliderPrev = minSlider;
            float maxSliderPrev = maxSlider;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            EditorGUILayout.PropertyField(minProperty, new GUIContent(GUIContent.none));
            EditorGUILayout.Space(2);
            EditorGUILayout.MinMaxSlider(ref minSlider, ref maxSlider, minLimit, maxLimit);
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(maxProperty, new GUIContent(GUIContent.none));
            EditorGUILayout.EndHorizontal();

            // We apply slider values only if they differ
            // This will make sure to not override field values if they were changed
            if (minSliderPrev != minSlider || maxSliderPrev != maxSlider)
            {
                minProperty.floatValue = minSlider;
                maxProperty.floatValue = maxSlider;
            }

            minProperty.floatValue = Mathf.Clamp(minProperty.floatValue, minLimit, 0);
            maxProperty.floatValue = Mathf.Clamp(maxProperty.floatValue, 0, maxLimit);
        }
    }
}
#endif