/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    A component that represents a hinge joint with specified rotation axis and limits.
    It exposes a Rotate function, that can be called from code to apply the rotation given the setup constraints.
    */

    public class JointHinge : MonoBehaviour
    {
        public enum RotationAxis
        {
            RootX,
            RootY,
            RootZ,
            LocalX,
            LocalY,
            LocalZ
        }

        public RotationAxis rotationAxis = RotationAxis.LocalX;
        public float minAngle = -20;
        public float maxAngle = 20;

        private Transform root;
        private Vector3 rotationAxisLocal;
        private Vector3 zeroTangentLocal;
        private Vector3 minTangentLocal;
        private Vector3 maxTangentLocal;

        private float currentAngle = 0; // Keeps track of the current state of rotation

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Reset();
        }

        private void OnDisable()
        {
            Reset();
        }

        public void Reset()
        {
            if (currentAngle != 0) Rotate(-currentAngle);
        }

        public void Initialize()
        {
            root = GetRoot();
            if (root == null && (rotationAxis == RotationAxis.RootX || rotationAxis == RotationAxis.RootY || rotationAxis == RotationAxis.RootZ))
            {
                Debug.LogWarning("JointHinge selected root rotation axis but can not retrieve Root Bone because an Animator Component could not be found. Falling back to LocalX");
                rotationAxis = RotationAxis.LocalX;
            }

            if (minAngle > maxAngle)
            {
                Debug.LogWarning("The minimum hinge angle on " + gameObject.name + " is larger than the maximum hinge angle. Adjusted maximum angle.");
                maxAngle = minAngle;
            }

            Vector3 r = Vector3.zero;
            Vector3 p = Vector3.zero;
            switch (rotationAxis)
            {
                case RotationAxis.RootX:
                    r = root.right;
                    p = root.forward;
                    break;
                case RotationAxis.RootY:
                    r = root.up;
                    p = root.right;
                    break;
                case RotationAxis.RootZ:
                    r = root.forward;
                    p = root.up;
                    break;
                case RotationAxis.LocalX:
                    r = transform.right;
                    p = transform.forward;
                    break;
                case RotationAxis.LocalY:
                    r = transform.up;
                    p = transform.right;
                    break;
                case RotationAxis.LocalZ:
                    r = transform.forward;
                    p = transform.right;
                    break;
            }

            rotationAxisLocal = transform.InverseTransformDirection(r);
            zeroTangentLocal = transform.InverseTransformDirection(p);

            //Zero Tangent should point to child if it exists instead of the preset perpendicular
            if (transform.childCount > 0)
            {
                Vector3 toChildLocal = transform.InverseTransformDirection(transform.GetChild(0).position - transform.position);
                Vector3 toChildPerpendicularLocal = Vector3.ProjectOnPlane(toChildLocal, rotationAxisLocal);
                if (toChildPerpendicularLocal != Vector3.zero)
                {
                    zeroTangentLocal = toChildPerpendicularLocal.normalized;
                }
            }

            //These two vectors are perpendicular by design
            rotationAxisLocal.Normalize();
            zeroTangentLocal.Normalize();
            minTangentLocal = Quaternion.AngleAxis(minAngle, rotationAxisLocal) * zeroTangentLocal;
            maxTangentLocal = Quaternion.AngleAxis(maxAngle, rotationAxisLocal) * zeroTangentLocal;
        }

        public void Rotate(float angle)
        {
            // Angle is assumed to be of the form (-180,180]

            // Apply constraints by clamping
            angle = Mathf.Clamp(currentAngle + angle, minAngle, maxAngle) - currentAngle;

            // Apply the rotation (In local space)
            transform.localRotation = transform.localRotation * Quaternion.AngleAxis(angle, rotationAxisLocal);

            currentAngle += angle;
        }

        public bool IsInitialized()
        {
            return rotationAxisLocal != Vector3.zero;
        }

        private Transform GetRoot()
        {
            Animator animator = GetComponentInParent<Animator>();
            return (animator != null) ? animator.avatarRoot : null;
        }

        public Vector3 GetRotationAxisWorld()
        {
            return transform.TransformDirection(rotationAxisLocal);
        }

        // The Tangents must be rotation-invariant, therefore we cancel the current rotation angle out
        public Vector3 GetZeroTangentWorld()
        {
            return transform.TransformDirection(Quaternion.AngleAxis(-currentAngle, rotationAxisLocal) * zeroTangentLocal);
        }

        public Vector3 GetMinTangentWorld()
        {
            return transform.TransformDirection(Quaternion.AngleAxis(-currentAngle, rotationAxisLocal) * minTangentLocal);
        }

        public Vector3 GetMaxTangentWorld()
        {
            return transform.TransformDirection(Quaternion.AngleAxis(-currentAngle, rotationAxisLocal) * maxTangentLocal);
        }

        public Vector3 GetCurrentTangentWorld()
        {
            return transform.TransformDirection(zeroTangentLocal);
        }

        public float GetAngleRange()
        {
            return maxAngle - minAngle;
        }

        public void DrawDebug()
        {
            float scale = 0.4f * UtilityFunctions.GetParentColliderScale(transform);
            Vector3 p = transform.position;
            Vector3 r = GetRotationAxisWorld();
            Vector3 minTangent = GetMinTangentWorld();
            Vector3 zeroTangent = GetZeroTangentWorld();
            Vector3 currentTangent = GetCurrentTangentWorld();

            //RotAxis
            GizmosDrawer.DrawLine(p, p + scale * r, Color.blue);

            //Pivot
            GizmosDrawer.DrawSphere(p, 0.05f * scale, Color.green);

            // Rotation Limit Arc
            GizmosDrawer.DrawSolidArc(p, r, minTangent, GetAngleRange(), 0.2f * scale, Color.yellow);

            // Current Rotation Used Arc
            GizmosDrawer.DrawSolidArc(p, r, zeroTangent, currentAngle, 0.1f * scale, Color.red);

            // Current Rotation used (same as above) just an additional line to emphasize
            GizmosDrawer.DrawLine(p, p + 0.2f * scale * currentTangent, Color.red);

            // Default Rotation
            GizmosDrawer.DrawLine(p, p + 0.2f * scale * zeroTangent, Color.magenta);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!UnityEditor.EditorApplication.isPlaying) Awake();
            DrawDebug();
        }
#endif
    }
}