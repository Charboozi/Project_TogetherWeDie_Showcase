/*
This file is part of Unity-Procedural-IK-Wall-Walking-Spider on github.com/PhilS94
Copyright (C) 2023 Philipp Schofield - All Rights Reserved
If purchased through stores (such as the Unity Asset Store) the corresponding EULA holds.
*/

//#define ENABLE_DEBUG_LOGGING

using UnityEngine;

namespace ProceduralSpider
{
    /*
    This class provides the inverse kinematics solver. The implemented algorithm is based on CCD.
    */

    public class IKSolver
    {
        public const int maxIterations = 5;             // The maximum amount of iterations the IK Solver can perform.
        public const float tolerance = 0.0001f;         // The tolerance of the IK solver. If the EndEffector is closer to Target than this value, it will stop the solving.
        public const float singularityRadius = 0.05f;   // If the target is this close to a given joint, that joint will not rotate. This reduces jitter from the IK Solver.
        public const float epsilonError = 0.000001f;    // If errorSqr changes less than epsilonErrorSqr in an iteration, the IK Solver will give up. This will significantly increase performance in cases where we solve to an unreachable target.

        /*
        Solves the IK Problem of the chain with given target using the CCD algorithm.
        @param joints: Contains all the hinge joints of the IK chain.
        @param endEffector: The end effector of the IK chain. It is not included in the list of hinge joints since it is not equipped with a AHingeJoint component.
        @param target: The target information the algorithm should solve for.
        @param weights: The weights for each of the hinge joints.
        @param hasFoot: If set to true, the last joint will adjust to the normal given by the target.
        @param footAngle: The angle the foot should have to the ground if hasFoot is enabled.
        @param scale: Will scale the tolerance, singularity radius and epsilon error.
         */
        public static float SolveIKChainCCD(ref JointHinge[] joints, Transform endEffector, ref IKTargetInfo target, ref float[] weights, bool hasFoot = false, float footAngle = 20, float scale = 1)
        {
            int itr = 0;
            float errorSqr = Vector3.SqrMagnitude(endEffector.position - target.position);
            float toleranceSqr = tolerance * tolerance * scale * scale;
            float singularityRadiusSqr = singularityRadius * singularityRadius * scale * scale;
            float epsilonErrorSqr = epsilonError * epsilonError * scale * scale;
            int n = hasFoot ? joints.Length - 1 : joints.Length;

            while (itr < maxIterations && errorSqr > toleranceSqr)
            {
                itr++;
                if (hasFoot) SolveFoot(joints[n], endEffector, target.normal, footAngle); // We solve foot before we solve other joints
                for (int i = 0; i < n; i++)
                {
                    SolveJointHinge(joints[i], endEffector, ref target, weights[i], singularityRadiusSqr);
                }

                float prevErrorSqr = errorSqr;
                errorSqr = Vector3.SqrMagnitude(endEffector.position - target.position);
                if (Mathf.Abs(errorSqr - prevErrorSqr) < epsilonErrorSqr) break;
            }

#if ENABLE_DEBUG_LOGGING
            if (itr > 0)
            {
                string name = endEffector.GetComponentInParent<IKChain>().name;
                Debug.Log($"IKSolver({name}): Iterations({itr}/{maxIterations}) Error({Mathf.Sqrt(errorSqr):F5}) Tolerance({tolerance}) Solved({errorSqr < toleranceSqr})");
            }
#endif
            return errorSqr;
        }

        /* Solves the specific hinge joint */
        private static void SolveJointHinge(JointHinge joint, Transform endEffector, ref IKTargetInfo target, float weight, float singularityRadiusSqr)
        {
            Vector3 position = joint.transform.position;
            Vector3 rotAxis = joint.GetRotationAxisWorld();
            Vector3 toTarget = Vector3.ProjectOnPlane(target.position - position, rotAxis);
            Vector3 toEnd = Vector3.ProjectOnPlane(endEffector.position - position, rotAxis);

            // If in singularity radius, skip.
            if (toTarget.sqrMagnitude < singularityRadiusSqr) return;
            if (toTarget == Vector3.zero || toEnd == Vector3.zero) return;

            float angle = weight * Vector3.SignedAngle(toEnd, toTarget, rotAxis);
            joint.Rotate(angle);
        }

        /* Solves the specific hinge joint as a foot */
        private static void SolveFoot(JointHinge joint, Transform endEffector, Vector3 normal, float footAngle)
        {
            Vector3 position = joint.transform.position;
            Vector3 rotAxis = joint.GetRotationAxisWorld();
            Vector3 toEnd = Vector3.ProjectOnPlane(endEffector.position - position, rotAxis);

            float angle = footAngle + 90.0f - Vector3.SignedAngle(Vector3.ProjectOnPlane(normal, rotAxis), toEnd, rotAxis);
            if (angle > 180) angle -= 360;
            joint.Rotate(angle);
        }
    }
}