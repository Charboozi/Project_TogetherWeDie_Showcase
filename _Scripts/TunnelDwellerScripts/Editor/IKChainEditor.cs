/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralSpider
{
    [CustomEditor(typeof(IKChain))]
    [CanEditMultipleObjects]
    public class IKChainEditor : Editor
    {
        private IKChain instance;
        private SerializedProperty jointsProperty;
        private SerializedProperty weightsProperty;
        private SerializedProperty endEffectorProperty;
        private SerializedProperty useFootProperty;
        private SerializedProperty footAngleProperty;
        private SerializedProperty debugTargetProperty;
        private SerializedProperty physicsBasedEndEffectorVelocityProperty;

        private void OnEnable()
        {
            instance = (IKChain)target;
            jointsProperty = serializedObject.FindProperty("joints");
            weightsProperty = serializedObject.FindProperty("weights");
            endEffectorProperty = serializedObject.FindProperty("endEffector");
            useFootProperty = serializedObject.FindProperty("useFoot");
            footAngleProperty = serializedObject.FindProperty("footAngle");
            debugTargetProperty = serializedObject.FindProperty("debugTarget");
            physicsBasedEndEffectorVelocityProperty = serializedObject.FindProperty("physicsBasedEndEffectorVelocity");
            AutoFindJoints();
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
            EditorGUI.indentLevel++;
            {
                for (int i = 0; i < instance.joints.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(jointsProperty.GetArrayElementAtIndex(i), new GUIContent($"Joint {i + 1}"));
                    GUI.enabled = true;
                    SerializedProperty weightProperty = weightsProperty.GetArrayElementAtIndex(i);
                    weightProperty.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent("Weight", "The weight of this joint. The IK Solver will use this value. Lower values will make this joint tend to rotate less."), weightProperty.floatValue), 0, 1);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.PropertyField(endEffectorProperty, new GUIContent("End Effector", "The end effector of this chain. Is the child transform of last joint."));
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Auto Create Joints")) AutoCreateJoints();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useFootProperty, new GUIContent("Has Foot", "If enabled, the last joint will rotate to the specified foot angle. This will make it act like a foot."));
            if (useFootProperty.boolValue) EditorGUILayout.Slider(footAngleProperty, 0, 90, new GUIContent("Foot Angle", "The foot angle in degrees used if foot is enabled. 0 corresponds to foot flat on the ground."));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(debugTargetProperty, new GUIContent("IK Target", "If set, will use it as target for this IK Chain. Useful for debugging purposes. Leave this empty if IKStepManager should manage Targets."));
            EditorGUILayout.PropertyField(physicsBasedEndEffectorVelocityProperty, new GUIContent("Physics Based End Effector Velocity", "If set, will calculate end effectors velocity based on physics fixed update. This will make velocity frame rate independent. If not set, a leg may step slower/faster for lower/higher frame rates as the step time of the leg scales with this velocity."));
        }

        private void AutoFindJoints()
        {
            //Find Joints
            JointHinge[] newJoints = instance.GetComponentsInChildren<JointHinge>();

            //Find Weights
            List<float> newWeights = new List<float>();
            for (int i = 0; i < newJoints.Length; i++)
            {
                JointHinge newJoint = newJoints[i];
                bool isNew = true;
                for (int j = 0; j < instance.joints.Length; j++)
                {
                    if (instance.joints[j] == newJoint && j < instance.weights.Length)
                    {
                        isNew = false;
                        newWeights.Add(instance.weights[j]);
                        break;
                    }
                }
                if (isNew) newWeights.Add(1);
            }
            Debug.Assert(newJoints.Length == newWeights.Count);

            //Find End Effector
            Transform newEndEffector = null;
            if (newJoints.Length > 0)
            {
                Transform last = newJoints[newJoints.Length - 1].transform;
                if (last.childCount > 0) newEndEffector = last.GetChild(0);
                else Debug.LogWarning($"IKChain has {last.name} as last joint, but it has no child to use as end effector.");
            }

            //Apply Values
            if (!IsEqual(newJoints, newWeights.ToArray(), newEndEffector))
            {
                Undo.RecordObject(instance, "Auto Find Joints for IKChain");
                instance.joints = newJoints;
                instance.weights = newWeights.ToArray();
                instance.endEffector = newEndEffector;
                EditorUtility.SetDirty(instance);
            }
        }

        private bool IsEqual(JointHinge[] joints, float[] weights, Transform endEffector)
        {
            return instance.joints.SequenceEqual(joints)
            && instance.weights.SequenceEqual(weights)
            && instance.endEffector == endEffector;
        }

        private void AutoCreateJoints()
        {
            AddJointToChildrenRecursive(instance.gameObject);
            AutoFindJoints();
        }

        private void AddJointToChildrenRecursive(GameObject g)
        {
            bool hasChildren = g.transform.childCount > 0;
            if (hasChildren)
            {
                JointHinge joint = g.GetComponent<JointHinge>();

                //If there is no joint yet, add it
                if (joint == null)
                {
                    Debug.Log($"Auto Setup: Added JointHinge Component to {g.name}");
                    joint = g.AddComponent<JointHinge>();

                    bool isFirstJoint = g.GetComponent<IKChain>() != null;
                    if (isFirstJoint)
                    {
                        joint.rotationAxis = JointHinge.RotationAxis.RootY;
                    }
                }

                //Recursion on first child
                AddJointToChildrenRecursive(g.transform.GetChild(0).gameObject);
            }
        }
    }
}
#endif