/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

using UnityEngine;

namespace ProceduralSpider
{
    /*
    A component that represents a chain of Joint Hinge with an end effector.
    Given a target transform, the chain will perform IK (inverse kinematics) to reach it on late update stage.
    The IK Solver implemented is a CCD (Cyclic Coordinate Descent) algorithm.
    One can manually supply the target transform, or leave it empty which allows IKStepManager to set it.
    */

    public class IKChain : MonoBehaviour
    {
        public JointHinge[] joints = new JointHinge[0];
        public float[] weights = new float[0];
        public Transform endEffector;
        public bool useFoot = true;
        public float footAngle = 20;
        public Transform debugTarget;
        public bool physicsBasedEndEffectorVelocity = true;

        private float chainLength = 0.0f;
        private IKTargetInfo currentTarget;
        private float scale = -1;
        private float errorSqr = 0.0f;
        private bool isSolveEnabled = true;
        private Vector3 endEffectorVelocity;
        private Vector3 lastEndEffectorPos;
        private int physicsUpdateCounter = 0;

        private void Awake()
        {
            if (joints.Length != weights.Length) Debug.LogWarning("There has to be the same amount of joints as weights.");
            if (endEffector != null) lastEndEffectorPos = endEffector.position;
            if (debugTarget != null) currentTarget = GetDebugTarget();
            CalculateChainLength();
        }

        private void OnEnable()
        {
            Reset();
            foreach (JointHinge joint in joints) joint.enabled = true;
        }

        private void OnDisable()
        {
            Reset();
            foreach (JointHinge joint in joints) joint.enabled = false;

        }

        public void Reset()
        {
            endEffectorVelocity = Vector3.zero;
            lastEndEffectorPos = endEffector.position;
            errorSqr = 0;
        }

        /* Late Update because IK solver should run late */
        private void LateUpdate()
        {
            if (debugTarget) currentTarget = GetDebugTarget();

            if (isSolveEnabled) errorSqr = IKSolver.SolveIKChainCCD(ref joints, endEffector, ref currentTarget, ref weights, useFoot, footAngle, GetScale());
            else errorSqr = Vector3.SqrMagnitude(endEffector.position - currentTarget.position);

            if (physicsBasedEndEffectorVelocity)
            {
                // We have to update velocity only when physics has ran as the spider and procedural root motion is updated on fixed update as well.
                // Otherwise the delta distance is discontinuous and e.g. high frame rates get a too large velocity due to small dt.
                // We can't do it in FixedUpdate directly as we want to make sure the end effector is solved before calculating it.
                if (physicsUpdateCounter > 0)
                {
                    float dt = physicsUpdateCounter * Time.fixedDeltaTime;
                    endEffectorVelocity = (endEffector.position - lastEndEffectorPos) / dt;
                    lastEndEffectorPos = endEffector.position;
                    physicsUpdateCounter = 0;
                }
            }
            else
            {
                endEffectorVelocity = (endEffector.position - lastEndEffectorPos) / Time.deltaTime;
                lastEndEffectorPos = endEffector.position;
            }
        }

        private void FixedUpdate()
        {
            if (physicsBasedEndEffectorVelocity)
                physicsUpdateCounter++;
        }

        public void CalculateChainLength()
        {
            chainLength = 0;
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 p = joints[i].transform.position;
                Vector3 q = (i != joints.Length - 1) ? joints[i + 1].transform.position : endEffector.position;
                chainLength += Vector3.Distance(p, q);
            }
        }

        public float GetChainLength()
        {
            if (chainLength == 0) CalculateChainLength();
            return chainLength;
        }

        private IKTargetInfo GetDebugTarget()
        {
            return new IKTargetInfo(debugTarget.position, debugTarget.up);
        }

        public void SetTarget(IKTargetInfo target)
        {
            if (!IsIKTargetSetAllowed()) return;

            currentTarget = target;

            // ✅ New: Align the foot (end effector) with the target normal
            if (useFoot && endEffector != null && target.isGround)
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, target.normal);
                if (forward.sqrMagnitude < 0.001f)
                    forward = Vector3.ProjectOnPlane(transform.up, target.normal); // Fallback

                Quaternion targetRot = Quaternion.LookRotation(forward.normalized, target.normal);
                endEffector.rotation = Quaternion.Slerp(endEffector.rotation, targetRot, 0.5f); // 0.5f = smoothing factor

            }
        }


        public JointHinge GetFirstJoint()
        {
            return joints[0];
        }

        public JointHinge GetLastJoint()
        {
            return joints[joints.Length - 1];
        }

        public Transform GetEndEffector()
        {
            return endEffector;
        }

        public IKTargetInfo GetTarget()
        {
            return currentTarget;
        }

        public bool IsIKTargetSetAllowed()
        {
            return debugTarget == null;
        }

        public void SetSolveEnabled(bool value)
        {
            isSolveEnabled = value;
        }

        public float GetErrorSqr()
        {
            return errorSqr;
        }

        public float GetScale()
        {
            if (scale < 0) scale = UtilityFunctions.GetParentColliderScale(transform);
            return scale;
        }

        public float GetTolerance()
        {
            return IKSolver.tolerance * GetScale();
        }

        public float GetSingularityRadius()
        {
            return IKSolver.singularityRadius * GetScale();
        }

        public Vector3 GetEndEffectorVelocity()
        {
            return endEffectorVelocity;
        }

        public void DrawDebug()
        {
            //Draw the Chain
            for (int k = 0; k < joints.Length - 1; k++) GizmosDrawer.DrawLine(joints[k].transform.position, joints[k + 1].transform.position, Color.green);
            if (joints.Length > 1) GizmosDrawer.DrawLine(joints[joints.Length - 1].transform.position, endEffector.position, Color.green);

            //Draw the tolerance as a sphere
            GizmosDrawer.DrawSphere(endEffector.position, GetTolerance(), Color.red);

            //Draw the singularity radius
            for (int k = 0; k < joints.Length; k++) GizmosDrawer.DrawWireSphere(joints[k].transform.position, GetSingularityRadius(), Color.red);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Animator animator = GetComponentInParent<Animator>();
            bool isAnimatorSelected = animator && UnityEditor.Selection.Contains(animator.gameObject);

            if (!UnityEditor.Selection.Contains(transform.gameObject) && !isAnimatorSelected) return;
            if (!endEffector) return;
            if (!UnityEditor.EditorApplication.isPlaying) Awake();

            DrawDebug();
            foreach (JointHinge joint in joints)
            {
                if (!joint.IsInitialized()) joint.Initialize();
                joint.DrawDebug();
            }
        }
#endif
    }
}