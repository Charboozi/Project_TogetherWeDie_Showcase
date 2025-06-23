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
    [CustomEditor(typeof(IKStepManager))]
    public class IKStepManagerEditor : Editor
    {
        private IKStepManager instance;
        private SerializedProperty stepLayerProperty;
        private SerializedProperty stepHeightProperty;
        private SerializedProperty stepCurveProperty;
        private SerializedProperty stepTimeMultiplierProperty;
        private SerializedProperty stepTimeMaxProperty;
        private SerializedProperty showDebugProperty;
        private SerializedProperty ikChainsProperty;
        private SerializedProperty ikSteppersProperty;

        struct IKStepperSerializer
        {
            public IKStepperSerializer(SerializedProperty property)
            {
                asyncChainsProperty = property.FindPropertyRelative("asyncChains");
                anchorReachProperty = property.FindPropertyRelative("anchorReach");
                anchorStrideProperty = property.FindPropertyRelative("anchorStride");
                anchorMultiplierProperty = property.FindPropertyRelative("anchorMultiplier");
            }
            public SerializedProperty asyncChainsProperty;
            public SerializedProperty anchorReachProperty;
            public SerializedProperty anchorStrideProperty;
            public SerializedProperty anchorMultiplierProperty;
        }
        private IKStepperSerializer[] ikStepperSerializers;

        int GetLegCount()
        {
            Debug.Assert(instance.ikSteppers.Length == instance.ikChains.Length);
            return instance.ikChains.Length;
        }

        private void OnEnable()
        {
            instance = (IKStepManager)target;
            stepLayerProperty = serializedObject.FindProperty("stepLayer");
            stepHeightProperty = serializedObject.FindProperty("stepHeight");
            stepCurveProperty = serializedObject.FindProperty("stepCurve");
            stepTimeMultiplierProperty = serializedObject.FindProperty("stepTimeMultiplier");
            stepTimeMaxProperty = serializedObject.FindProperty("stepTimeMax");
            showDebugProperty = serializedObject.FindProperty("showDebug");
            ikChainsProperty = serializedObject.FindProperty("ikChains");
            ikSteppersProperty = serializedObject.FindProperty("ikSteppers");
            AutoFindIKChains();
            ikStepperSerializers = new IKStepperSerializer[GetLegCount()];
            for (int i = 0; i < GetLegCount(); i++)
                ikStepperSerializers[i] = new IKStepperSerializer(ikSteppersProperty.GetArrayElementAtIndex(i));
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
            EditorGUILayout.LabelField("Step Layer", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(stepLayerProperty, new GUIContent("Layer", "The Layer in which we raycast to find a suitable target."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Step Animation", EditorStyles.boldLabel);
            EditorGUILayout.Slider(stepHeightProperty, 0, 2, new GUIContent("Height", "The height each step should take."));
            EditorGUILayout.PropertyField(stepCurveProperty, new GUIContent("Curve", "The Animation Curve each step should follow. An inverse parabola is recommended."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Step Duration", EditorStyles.boldLabel);
            EditorGUILayout.Slider(stepTimeMultiplierProperty, 0, 1, new GUIContent("Step Duration Multiplier", "A higher value will lead to slower steps. A smaller value to faster steps.  The step duration is dynamic and relative to current velocity."));
            EditorGUILayout.Slider(stepTimeMaxProperty, 0, 2, new GUIContent("Step Duration (Max)", "The maximum step duration. This makes sure we never step slower than this value."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"Leg Properties ({GetLegCount()})", EditorStyles.boldLabel);
            if (GUILayout.Button("Auto Setup Asynchronous Legs")) AutoSetupAsyncLegsAll();
            for (int i = 0; i < GetLegCount(); i++) DrawCustomIKStepperInspector(i);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showDebugProperty, new GUIContent("Draw Debug", "Enable this to draw debug drawings in the viewport."));
        }

        private void DrawCustomIKStepperInspector(int i)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField(instance.ikChains[i], typeof(IKChain), false);
            GUI.enabled = true;
            EditorGUI.indentLevel++;
            {
                IKStepperSerializer ser = ikStepperSerializers[i];
                EditorGUILayout.PropertyField(ser.asyncChainsProperty, new GUIContent("Asynchronous Legs", "The legs to which we should step asynchronously. If one of them is in the process of stepping, this chain is blocked from stepping. It is recommended to only add the leg to the left/right and the leg in front."));
                EditorGUILayout.Slider(ser.anchorReachProperty, -1, 1, new GUIContent("Anchor Reach", "A higher value will set targets more outwards. A lower value will set targets more inwards."));
                EditorGUILayout.Slider(ser.anchorStrideProperty, -1, 1, new GUIContent("Anchor Stride", "A higher value will set targets more to the right. A lower value will set targets more to the left."));
                EditorGUILayout.Slider(ser.anchorMultiplierProperty, 1, 2, new GUIContent("Anchor Multiplier", "A higher value will lead to larger steps. A value of 1 will make every step land exactly on the anchor position."));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void AutoFindIKChains()
        {
            //Find all non-empty IKChains
            IKChain[] ikChains = instance.GetComponentsInChildren<IKChain>(true).Where(ikChain => ikChain.joints.Length > 0).ToArray();

            //Create all IKSteppers
            List<IKStepper> temp = new List<IKStepper>();
            foreach (IKChain ikChain in ikChains)
            {
                bool alreadyExists = false;
                for (int i = 0; i < instance.ikChains.Length; i++)
                {
                    if (ikChain == instance.ikChains[i])
                    {
                        temp.Add(instance.ikSteppers[i]);
                        alreadyExists = true;
                        break;
                    }
                }
                if (!alreadyExists)
                {
                    temp.Add(new IKStepper());
                }
            }
            IKStepper[] ikSteppers = temp.ToArray();
            Debug.Assert(ikSteppers.Length == ikChains.Length);

            //Apply Values
            if (!IsEqual(ikChains, ikSteppers))
            {
                Undo.RecordObject(instance, "Auto Find IK Chains for IK Step Manager");
                instance.ikChains = ikChains;
                instance.ikSteppers = ikSteppers;
                EditorUtility.SetDirty(instance);
            }
        }

        private bool IsEqual(IKChain[] ikChains, IKStepper[] ikSteppers)
        {
            return instance.ikChains.SequenceEqual(ikChains)
            && instance.ikSteppers.SequenceEqual(ikSteppers);
        }

        private void AutoSetupAsyncLegsAll()
        {
            Animator[] animators = instance.GetComponentsInChildren<Animator>();
            if (animators.Length != 0)
            {
                Transform rootBone = animators[0].avatarRoot;
                Undo.RecordObject(instance, "Auto setup asynchronous legs");
                for (int i = 0; i < GetLegCount(); i++)
                {
                    AutoSetupAsyncLegs(rootBone, instance.ikSteppers[i], instance.ikChains[i], instance.ikChains);
                }
                EditorUtility.SetDirty(instance);
            }
            else Debug.LogWarning($"Could not auto initialize IK Steppers, because there was no Animator Component in children.");
        }

        private static void AutoSetupAsyncLegs(Transform rootBone, IKStepper ikStepper, IKChain ikChain, IKChain[] all)
        {
            IKChain inFront = FindIKChainInFront(rootBone, ikChain, all);
            IKChain opposite = FindIKChainOpposite(rootBone, ikChain, all);
            List<IKChain> result = new List<IKChain>(2);
            if (inFront != null) result.Add(inFront);
            if (opposite != null) result.Add(opposite);
            Debug.Log($"Auto Setup({ikChain.name}): Async Chains: [{string.Join(", ", result)}]");
            ikStepper.asyncChains = result.ToArray();
        }

        private static IKChain FindIKChainInFront(Transform rootBone, IKChain me, IKChain[] all)
        {
            IKChain result = null;
            float error = float.MaxValue;
            foreach (IKChain other in all)
            {
                if (other == me) continue; // Skip ourselves

                Vector3 v1 = me.transform.position - rootBone.transform.position;
                Vector3 v2 = other.transform.position - rootBone.transform.position;
                float sgn1 = Mathf.Sign(Vector3.Dot(v1, rootBone.transform.right));
                float sgn2 = Mathf.Sign(Vector3.Dot(v2, rootBone.transform.right));
                if (sgn1 * sgn2 < 0) continue; // Skip those that are on the other side

                float angle = Vector3.SignedAngle(v1, v2, sgn1 * rootBone.transform.up);
                if (angle > 0) continue; // Skip those those that are behind us

                float newError = Mathf.Abs(angle);
                if (newError < error)
                {
                    error = newError;
                    result = other;
                }
            }
            return result;
        }

        private static IKChain FindIKChainOpposite(Transform rootBone, IKChain me, IKChain[] all)
        {
            IKChain result = null;
            float error = float.MaxValue;
            foreach (IKChain other in all)
            {
                if (other == me) continue; // Skip ourselves

                Vector3 fwd = rootBone.transform.forward;
                Vector3 v1 = me.transform.position - rootBone.transform.position;
                Vector3 v2 = other.transform.position - rootBone.transform.position;
                float sgn1 = Mathf.Sign(Vector3.Dot(v1, rootBone.transform.right));
                float sgn2 = Mathf.Sign(Vector3.Dot(v2, rootBone.transform.right));
                if (sgn1 * sgn2 > 0) continue; // Skip those that are on same side

                float newError = Mathf.Abs(Vector3.Angle(v1, fwd) - Vector3.Angle(v2, fwd));
                if (newError < error)
                {
                    error = newError;
                    result = other;
                }
            }
            return result;
        }
    }
}
#endif