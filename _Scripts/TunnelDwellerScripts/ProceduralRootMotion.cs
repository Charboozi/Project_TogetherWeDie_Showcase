/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    This component adds procedural root motion to the spider.
    The root bone will move and rotate in accordance to its legs, specifically the height of each leg.
    It can also add a breathing movement.
    The root bone is given by the Avatar of the Animator Component.
    */

    [RequireComponent(typeof(Spider))]
    public class ProceduralRootMotion : MonoBehaviour
    {
        [Header("Root Bone")]
        [Tooltip("The height we will set the root bone to be at runtime.")]
        public float rootHeight = 0.1f;

        [Header("Root Movement")]
        [Tooltip("If enabled, will translate the root bone according to legs.")]
        public bool useRootMovement = true;

        [Tooltip("How fast the root bone will move.")]
        [Range(0, 100)]
        public float rootMovementSpeed = 20;

        [Tooltip("The higher the vertical weight, the more the root bone will move upwards.")]
        [Range(0, 1)]
        public float rootMovementVerticalWeight = 0.3f;

        [Tooltip("The higher the horizontal weight, the more the root bone will move sideways.")]
        [Range(0, 1)]
        public float rootMovementHorizontalWeight = 0.1f;

        [Header("Root Rotation")]
        [Tooltip("If enabled, will rotate the root bone according to legs.")]
        public bool useRootRotation = true;

        [Tooltip("How fast the root bone will rotate.")]
        [Range(0, 100)]
        public float rootRotationSpeed = 20;

        [Tooltip("The higher the value, the more the root bone will rotate according to legs.")]
        [Range(0, 1)]
        public float rootRotationWeight = 0.1f;

        [Header("Breathing")]
        [Tooltip("If enabled, will periodically move root bone up and down like breathing.")]
        public bool useBreathing = true;

        [Tooltip("The time in seconds to finish one breath. Higher values will make it move slower.")]
        [Range(0.01f, 20)]
        public float breathePeriod = 10;

        [Tooltip("The magnitude of how much the root bone should move up/down.")]
        [Range(0, 1)]
        public float breatheMagnitude = 0.1f;

        [Header("Debug")]
        [Tooltip("Enable this to draw debug drawings in the viewport.")]
        public bool showDebug = false;

        private Transform rootBone;
        private Spider spider;
        private IKChain[] legs;
        private Vector3 rootYLocal;
        private Vector3 rootZLocal;
        private Vector3 rootDefaultPositionLocal;
        private Quaternion rootDefaultRotationLocal;
        private Vector3 rootPosition;

        private void Awake()
        {
            spider = GetComponent<Spider>();
            Animator[] animators = GetComponentsInChildren<Animator>();
            if (animators.Length == 0)
            {
                Debug.LogError("Can not apply Procedural Root Motion because an Animator Component could not be found. Make sure Spider Component has some child with an Animator Component.");
                return;
            }

            if (legs == null || legs.Length == 0) legs = transform.GetComponentsInChildren<IKChain>();

            rootBone = animators[0].avatarRoot;
            rootYLocal = rootBone.transform.InverseTransformDirection(transform.up);
            rootZLocal = rootBone.transform.InverseTransformDirection(transform.forward);
            rootPosition = rootBone.transform.position + spider.GetColliderHeight() * rootHeight * transform.up;
            rootDefaultPositionLocal = transform.InverseTransformPoint(rootPosition);
            rootDefaultRotationLocal = rootBone.transform.localRotation;
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
            rootPosition = GetRootDefaultPosition();
            rootBone.transform.position = rootPosition;
            rootBone.transform.localRotation = rootDefaultRotationLocal;
        }

        // Could be in Update, but since spider moves on a fixed frame basis, the position smoothing would jitter.
        void FixedUpdate()
        {
            //Apply root bone translation
            rootPosition = useRootMovement ? Vector3.Lerp(rootPosition, GetLegsCentroid(), Time.fixedDeltaTime * rootMovementSpeed) : GetRootDefaultPosition();
            rootBone.transform.position = rootPosition;

            //Apply root bone rotation
            if (useRootRotation)
            {
                Vector3 Y = rootBone.TransformDirection(rootYLocal);
                Vector3 newNormal = GetLegsPlaneNormal();

                //Use Global X for  pitch
                Vector3 X = transform.right;
                float angleX = Vector3.SignedAngle(Vector3.ProjectOnPlane(Y, X), Vector3.ProjectOnPlane(newNormal, X), X);
                angleX = Mathf.LerpAngle(0, angleX, Time.fixedDeltaTime * rootRotationSpeed);
                rootBone.transform.rotation = Quaternion.AngleAxis(angleX, X) * rootBone.transform.rotation;

                //Use Local Z for roll. With the above global X for pitch, this avoids any kind of yaw happening.
                Vector3 Z = rootBone.TransformDirection(rootZLocal);
                float angleZ = Vector3.SignedAngle(Y, Vector3.ProjectOnPlane(newNormal, Z), Z);
                angleZ = Mathf.LerpAngle(0, angleZ, Time.fixedDeltaTime * rootRotationSpeed);
                rootBone.transform.rotation = Quaternion.AngleAxis(angleZ, Z) * rootBone.transform.rotation;
            }

            //Apply Breathing
            if (useBreathing)
            {
                float t = (Time.time * 2 * Mathf.PI / breathePeriod) % (2 * Mathf.PI);
                float amplitude = breatheMagnitude * spider.GetColliderHeight() / 2;
                Vector3 direction = rootBone.TransformDirection(rootYLocal);
                rootBone.transform.position = rootPosition + amplitude * (Mathf.Sin(t) + 1f) * direction;
            }
        }

        /*
        * Calculate the centroid (center of gravity) given by all end effector positions of the legs
        */
        private Vector3 GetLegsCentroid()
        {
            if (legs == null || legs.Length == 0) return GetRootDefaultPosition();

            Vector3 defaultPosition = GetRootDefaultPosition();

            // Calculate the centroid of legs position
            Vector3 centroid = Vector3.zero;
            float k = 0;
            for (int i = 0; i < legs.Length; i++)
            {
                centroid += legs[i].GetEndEffector().position;
                k++;
            }
            centroid /= k;

            // Offset the calculated centroid so it respects default height
            Vector3 offset = Vector3.Project(defaultPosition - spider.GetColliderBottomPoint(), transform.up);
            centroid += offset;

            //Calculate the delta that we need to apply to default
            Vector3 deltaCentroid = centroid - defaultPosition;

            // Split delta into vertical and horizontal parts
            Vector3 vPart = Vector3.Project(deltaCentroid, transform.up);
            Vector3 hPart = Vector3.ProjectOnPlane(deltaCentroid, transform.up);

            // Clamp centroid displacement
            float maxDisplacement = spider.GetColliderHeight() / 2;
            float vMagnitude = vPart.magnitude;
            float hMagnitude = hPart.magnitude;
            if (vMagnitude > maxDisplacement) vPart = maxDisplacement * (vPart / vMagnitude);
            if (hMagnitude > maxDisplacement) hPart = maxDisplacement * (hPart / hMagnitude);

            Vector3 smoothDeltaCentroid = Vector3.Lerp(Vector3.zero, vPart, rootMovementVerticalWeight) + Vector3.Lerp(Vector3.zero, hPart, rootMovementHorizontalWeight);

            return defaultPosition + smoothDeltaCentroid;
        }

        /*
        * Calculate the normal of the plane defined by leg positions, so we know how to rotate the root bone
        */
        private Vector3 GetLegsPlaneNormal()
        {
            if (legs == null) return transform.up;
            if (rootRotationWeight <= 0f) return transform.up;

            Vector3 newNormal = transform.up;
            Vector3 toEnd;
            Vector3 currentTangent;

            for (int i = 0; i < legs.Length; i++)
            {
                toEnd = legs[i].GetEndEffector().position - transform.position;
                currentTangent = Vector3.ProjectOnPlane(toEnd, transform.up);

                if (currentTangent == Vector3.zero) continue; // Actually here we would have a 90degree rotation but there is no choice of a tangent.

                newNormal = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(currentTangent, toEnd), rootRotationWeight) * newNormal;
            }
            return newNormal;
        }

        public Vector3 GetRootDefaultPosition()
        {
            return transform.TransformPoint(rootDefaultPositionLocal);
        }

        private void DrawDebug()
        {
            //Draw the root bone up Y
            Gizmos.color = Color.blue;
            Debug.DrawLine(transform.position, transform.position + spider.GetColliderHeight() * rootBone.TransformDirection(rootYLocal), Color.blue);

            //Draw the default centroid
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(GetRootDefaultPosition(), Vector3.one * 0.1f);

            //Draw the centroid given by legs
            Gizmos.color = Color.red;
            Gizmos.DrawCube(GetLegsCentroid(), Vector3.one * 0.1f);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!showDebug) return;
            if (!UnityEditor.Selection.Contains(transform.gameObject)) return;
            if (!UnityEditor.EditorApplication.isPlaying) Awake();
            DrawDebug();
        }
#endif
    }
}